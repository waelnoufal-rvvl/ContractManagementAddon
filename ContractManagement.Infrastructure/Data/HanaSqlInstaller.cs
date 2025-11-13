using System;

namespace ContractManagement.Infrastructure.Data
{
    public class HanaSqlInstaller
    {
        private readonly IHanaDb _db;

        public HanaSqlInstaller(IHanaDb db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        // Full ICP calculation aligned to Doc7
        private const string CreateIcpCalcProc = @"
CREATE OR REPLACE PROCEDURE RVCM_CALC_ICP (IN ICPCode NVARCHAR(50))
LANGUAGE SQLSCRIPT AS
BEGIN
    DECLARE GrossValue DECIMAL(19,6);
    SELECT :GrossValue = COALESCE(SUM(ROUND(""U_ThisQty"" * ""U_UnitPrice"", 2)), 0)
    FROM ""@RVCM_ICP1"" WHERE ""Code"" = :ICPCode;

    DECLARE AdvanceDeduct DECIMAL(19,6);
    SELECT :AdvanceDeduct = COALESCE(SUM(""U_Amount""), 0)
    FROM ""@RVCM_DEDUCT"" WHERE ""U_ICPCode"" = :ICPCode AND ""U_DeductType"" = 'Advance';

    DECLARE MaterialDeduct DECIMAL(19,6);
    SELECT :MaterialDeduct = COALESCE(SUM(""U_Amount""), 0)
    FROM ""@RVCM_DEDUCT"" WHERE ""U_ICPCode"" = :ICPCode AND ""U_DeductType"" = 'Material';

    DECLARE OtherDeduct DECIMAL(19,6);
    SELECT :OtherDeduct = COALESCE(SUM(""U_Amount""), 0)
    FROM ""@RVCM_DEDUCT"" WHERE ""U_ICPCode"" = :ICPCode AND ""U_DeductType"" = 'Other';

    DECLARE RetentionPct DECIMAL(19,6);
    SELECT :RetentionPct = COALESCE(""U_RetentionPct"", 0) FROM ""@RVCM_ICP"" WHERE ""Code"" = :ICPCode;

    DECLARE RetentionAmt DECIMAL(19,6);
    SELECT :RetentionAmt = ROUND(:GrossValue * (:RetentionPct / 100), 2) FROM DUMMY;

    DECLARE Subtotal DECIMAL(19,6);
    SELECT :Subtotal = ROUND(:GrossValue - :AdvanceDeduct - :RetentionAmt - :MaterialDeduct - :OtherDeduct, 2) FROM DUMMY;

    IF :Subtotal < 0 THEN
        SIGNAL SQL_ERROR 'ICP_NEGATIVE_SUBTOTAL' SET MESSAGE_TEXT = 'Net amount cannot be negative. Total deductions exceed gross value.';
    END IF;

    DECLARE TaxRate DECIMAL(19,6);
    SELECT tc.""Rate"" INTO TaxRate
    FROM ""@RVCM_ICP"" i
    INNER JOIN ""@RVCM_CNTRCT"" c ON i.""U_ContractCode"" = c.""Code""
    INNER JOIN ""OSTC"" tc ON c.""U_TaxCode"" = tc.""Code""
    WHERE i.""Code"" = :ICPCode;

    DECLARE VATAmount DECIMAL(19,6);
    SELECT :VATAmount = ROUND(:Subtotal * (:TaxRate / 100), 2) FROM DUMMY;

    DECLARE NetPayment DECIMAL(19,6);
    SELECT :NetPayment = ROUND(:Subtotal + :VATAmount, 2) FROM DUMMY;

    UPDATE ""@RVCM_ICP""
    SET ""U_PeriodValue"" = :GrossValue,
        ""U_AdvDeduct"" = :AdvanceDeduct,
        ""U_RetentionAmt"" = :RetentionAmt,
        ""U_MaterialDeduct"" = :MaterialDeduct,
        ""U_OtherDeduct"" = :OtherDeduct,
        ""U_VATAmount"" = :VATAmount,
        ""U_NetPayment"" = :NetPayment
    WHERE ""Code"" = :ICPCode;
END;";

        // Validation against remaining contract value
        private const string CreateIcpValidateProc = @"
CREATE OR REPLACE PROCEDURE RVCM_VALIDATE_ICP (IN ICPCode NVARCHAR(50))
LANGUAGE SQLSCRIPT AS
BEGIN
    DECLARE ContractCode NVARCHAR(50);
    SELECT :ContractCode = ""U_ContractCode"" FROM ""@RVCM_ICP"" WHERE ""Code"" = :ICPCode;

    DECLARE ContractTotal DECIMAL(19,6);
    SELECT :ContractTotal = COALESCE(""U_TotalValue"", 0) FROM ""@RVCM_CNTRCT"" WHERE ""Code"" = :ContractCode;

    DECLARE TotalBilled DECIMAL(19,6);
    SELECT :TotalBilled = COALESCE(SUM(CASE WHEN i2.""Code"" <> :ICPCode THEN i2.""U_NetPayment"" ELSE 0 END), 0)
    FROM ""@RVCM_ICP"" i2
    WHERE i2.""U_ContractCode"" = :ContractCode AND i2.""U_Status"" IN ('A','P');

    DECLARE Remaining DECIMAL(19,6);
    SELECT :Remaining = :ContractTotal - :TotalBilled FROM DUMMY;

    DECLARE ThisNet DECIMAL(19,6);
    SELECT :ThisNet = COALESCE(""U_NetPayment"", 0) FROM ""@RVCM_ICP"" WHERE ""Code"" = :ICPCode;

    IF :ThisNet > :Remaining THEN
        SIGNAL SQL_ERROR 'ICP_EXCEEDS_REMAINING' SET MESSAGE_TEXT = 'ICP amount exceeds remaining contract value';
    END IF;
END;";

        // Retention recompute per contract
        private const string CreateRetentionProc = @"
CREATE OR REPLACE PROCEDURE RVCM_RECALC_RETENTION (IN ContractCode NVARCHAR(50))
LANGUAGE SQLSCRIPT AS
BEGIN
    DECLARE TotalRetained DECIMAL(19,6);
    SELECT :TotalRetained = COALESCE(SUM(""U_RetentionAmt""), 0)
    FROM ""@RVCM_RETEN"" WHERE ""U_ContractCode"" = :ContractCode;

    DECLARE TotalReleased DECIMAL(19,6);
    SELECT :TotalReleased = COALESCE(SUM(""U_ReleasedAmt""), 0)
    FROM ""@RVCM_RETEN"" WHERE ""U_ContractCode"" = :ContractCode;

    DECLARE Balance DECIMAL(19,6);
    SELECT :Balance = :TotalRetained - :TotalReleased FROM DUMMY;

    UPDATE ""@RVCM_RETEN"" SET ""U_Balance"" = :Balance WHERE ""U_ContractCode"" = :ContractCode;
END;";

        public void EnsureProcedures()
        {
            _db.ExecuteNonQuery(CreateIcpCalcProc);
            _db.ExecuteNonQuery(CreateIcpValidateProc);
            _db.ExecuteNonQuery(CreateRetentionProc);
            _db.ExecuteNonQuery(CreateReleaseRetentionProc);
            _db.ExecuteNonQuery(CreateSummaryView);
            _db.ExecuteNonQuery(CreateIndexProc);
            _db.ExecuteNonQuery("CALL RVCM_INSTALL_INDEXES()");
        }

        private const string CreateReleaseRetentionProc = @"
CREATE OR REPLACE PROCEDURE RVCM_RELEASE_RETENTION (IN ContractCode NVARCHAR(50), IN ReleaseAmt DECIMAL(19,6), IN Ref NVARCHAR(100))
LANGUAGE SQLSCRIPT AS
BEGIN
    IF :ReleaseAmt <= 0 THEN
        SIGNAL SQL_ERROR 'RET_INVALID_AMT' SET MESSAGE_TEXT = 'Release amount must be positive';
    END IF;

    DECLARE NewCode NVARCHAR(50);
    SELECT TO_NVARCHAR(CURRENT_TIMESTAMP, 'YYYYMMDDHH24MISSFF3') INTO NewCode FROM DUMMY;

    INSERT INTO ""@RVCM_RETEN"" (""Code"",""Name"",""U_ContractCode"",""U_ICPCode"",""U_RetentionPct"",""U_RetentionAmt"",""U_ReleasedAmt"",""U_Balance"",""U_Status"")
    VALUES (:NewCode,:Ref,:ContractCode,'',NULL,0,:ReleaseAmt,0,'RELEASED');

    CALL RVCM_RECALC_RETENTION(:ContractCode);
END;";

        private const string CreateSummaryView = @"
CREATE OR REPLACE VIEW RVCM_V_ICP_SUMMARY AS
SELECT i.""Code"" AS ""ICPCode"", i.""U_ContractCode"" AS ""ContractCode"", i.""U_CertDate"" AS ""CertDate"",
       i.""U_PeriodValue"" AS ""GrossValue"", i.""U_RetentionAmt"" AS ""RetentionAmt"", i.""U_AdvDeduct"" AS ""AdvanceDeduct"",
       i.""U_MaterialDeduct"" AS ""MaterialDeduct"", i.""U_OtherDeduct"" AS ""OtherDeduct"",
       i.""U_VATAmount"" AS ""VATAmount"", i.""U_NetPayment"" AS ""NetPayment"", i.""U_Status"" AS ""Status"",
       c.""U_CardCode"" AS ""CardCode"", c.""U_TaxCode"" AS ""TaxCode""
FROM ""@RVCM_ICP"" i
JOIN ""@RVCM_CNTRCT"" c ON c.""Code"" = i.""U_ContractCode"";";

        private const string CreateIndexProc = @"
CREATE OR REPLACE PROCEDURE RVCM_INSTALL_INDEXES()
LANGUAGE SQLSCRIPT AS
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION BEGIN END;
    EXECUTE IMMEDIATE 'CREATE INDEX ""IX_RVCM_ICP_Contract"" ON ""@RVCM_ICP""(""U_ContractCode"")';
    EXECUTE IMMEDIATE 'CREATE INDEX ""IX_RVCM_ICP_Status"" ON ""@RVCM_ICP""(""U_Status"")';
    EXECUTE IMMEDIATE 'CREATE INDEX ""IX_RVCM_ICP1_Code"" ON ""@RVCM_ICP1""(""Code"")';
    EXECUTE IMMEDIATE 'CREATE INDEX ""IX_RVCM_DEDUCT_ICP"" ON ""@RVCM_DEDUCT""(""U_ICPCode"")';
END;";
    }
}
