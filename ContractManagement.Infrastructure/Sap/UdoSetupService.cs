using System;
using SAPbobsCOM;

namespace ContractManagement.Infrastructure.Sap
{
    public class UdoSetupService
    {
        private readonly Company _company;

        public UdoSetupService(Company company)
        {
            _company = company ?? throw new ArgumentNullException(nameof(company));
        }

        public void EnsureUdos()
        {
            EnsureContractUdo();
            EnsureIcpUdo();
        }

        private void EnsureContractUdo()
        {
            UserObjectsMD udo = null;
            try
            {
                udo = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                if (udo.GetByKey("RVCM_CNTRCT")) return;

                udo.Code = "RVCM_CNTRCT";
                udo.Name = "Contract Management";
                udo.ObjectType = BoUDOObjType.boud_MasterData;
                udo.TableName = "RVCM_CNTRCT";

                udo.CanArchive = BoYesNoEnum.tYES;
                udo.CanCancel = BoYesNoEnum.tYES;
                udo.CanClose = BoYesNoEnum.tYES;
                udo.CanDelete = BoYesNoEnum.tYES;
                udo.CanFind = BoYesNoEnum.tYES;
                udo.ManageSeries = BoYesNoEnum.tYES;
                udo.CanYearTransfer = BoYesNoEnum.tYES;

                udo.ChildTables.TableName = "RVCM_CNTRCT1";
                udo.ChildTables.Add();

                var res = udo.Add();
                if (res != 0)
                {
                    _company.GetLastError(out var code, out var msg);
                    throw new InvalidOperationException($"Failed to create UDO RVCM_CNTRCT: {code} - {msg}");
                }
            }
            finally
            {
                if (udo != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(udo);
            }
        }

        private void EnsureIcpUdo()
        {
            UserObjectsMD udo = null;
            try
            {
                udo = (UserObjectsMD)_company.GetBusinessObject(BoObjectTypes.oUserObjectsMD);
                if (udo.GetByKey("RVCM_ICP")) return;

                udo.Code = "RVCM_ICP";
                udo.Name = "Interim Certificate Payment";
                udo.ObjectType = BoUDOObjType.boud_Document;
                udo.TableName = "RVCM_ICP";

                udo.CanArchive = BoYesNoEnum.tYES;
                udo.CanCancel = BoYesNoEnum.tYES;
                udo.CanClose = BoYesNoEnum.tYES;
                udo.CanDelete = BoYesNoEnum.tYES;
                udo.CanFind = BoYesNoEnum.tYES;
                udo.ManageSeries = BoYesNoEnum.tYES;
                udo.CanYearTransfer = BoYesNoEnum.tYES;

                udo.ChildTables.TableName = "RVCM_ICP1";
                udo.ChildTables.Add();

                var res = udo.Add();
                if (res != 0)
                {
                    _company.GetLastError(out var code, out var msg);
                    throw new InvalidOperationException($"Failed to create UDO RVCM_ICP: {code} - {msg}");
                }
            }
            finally
            {
                if (udo != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(udo);
            }
        }
    }
}

