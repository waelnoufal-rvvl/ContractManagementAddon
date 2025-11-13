using System;
using System.Globalization;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using DI = SAPbobsCOM;
using ContractManagement.Infrastructure.Data;

namespace ContractManagementAddon
{
    internal class RetentionReleaseForm
    {
        private readonly string _initialContractCode;
        private Form _form;
        private EditText _txtContract;
        private EditText _txtAmount;
        private EditText _txtRef;

        public RetentionReleaseForm(string contractCode)
        {
            _initialContractCode = contractCode;
        }

        public void Show()
        {
            var app = SAPbouiCOM.Framework.Application.SBO_Application;
            var fcp = (FormCreationParams)app.CreateObject(BoCreatableObjectType.cot_FormCreationParams);
            fcp.UniqueID = "RVCM_RET_RELEASE";
            fcp.BorderStyle = BoFormBorderStyle.fbs_Sizable;
            fcp.FormType = "RVCM_RET_RELEASE";
            _form = app.Forms.AddEx(fcp);
            _form.Title = "Release Retention";
            _form.Width = 420;
            _form.Height = 210;

            int left = 10, top = 20, w = 150, h = 15, gap = 10;

            var lblCtr = _form.Items.Add("lblCtr", BoFormItemTypes.it_STATIC);
            lblCtr.Left = left; lblCtr.Top = top; lblCtr.Width = w; lblCtr.Height = h;
            ((StaticText)lblCtr.Specific).Caption = "Contract Code";

            var txtCtr = _form.Items.Add("txtCtr", BoFormItemTypes.it_EDIT);
            txtCtr.Left = left + w + gap; txtCtr.Top = top; txtCtr.Width = 200; txtCtr.Height = h;
            _txtContract = (EditText)txtCtr.Specific;

            top += h + gap;
            var lblAmt = _form.Items.Add("lblAmt", BoFormItemTypes.it_STATIC);
            lblAmt.Left = left; lblAmt.Top = top; lblAmt.Width = w; lblAmt.Height = h;
            ((StaticText)lblAmt.Specific).Caption = "Release Amount";

            var txtAmt = _form.Items.Add("txtAmt", BoFormItemTypes.it_EDIT);
            txtAmt.Left = left + w + gap; txtAmt.Top = top; txtAmt.Width = 200; txtAmt.Height = h;
            _txtAmount = (EditText)txtAmt.Specific;

            top += h + gap;
            var lblRef = _form.Items.Add("lblRef", BoFormItemTypes.it_STATIC);
            lblRef.Left = left; lblRef.Top = top; lblRef.Width = w; lblRef.Height = h;
            ((StaticText)lblRef.Specific).Caption = "Reference";

            var txtRef = _form.Items.Add("txtRef", BoFormItemTypes.it_EDIT);
            txtRef.Left = left + w + gap; txtRef.Top = top; txtRef.Width = 200; txtRef.Height = h;
            _txtRef = (EditText)txtRef.Specific;

            top += h + (2 * gap);
            var btnOk = _form.Items.Add("btnOk", BoFormItemTypes.it_BUTTON);
            btnOk.Left = left + w + gap; btnOk.Top = top; btnOk.Width = 80; btnOk.Height = 20;
            ((Button)btnOk.Specific).Caption = "Release";
            var btnCancel = _form.Items.Add("btnCancel", BoFormItemTypes.it_BUTTON);
            btnCancel.Left = btnOk.Left + 90; btnCancel.Top = top; btnCancel.Width = 80; btnCancel.Height = 20;
            ((Button)btnCancel.Specific).Caption = "Cancel";

            _form.Visible = true;

            _txtContract.Value = _initialContractCode ?? string.Empty;
            _txtAmount.Value = string.Empty;
            _txtRef.Value = string.Empty;

            SAPbouiCOM.Framework.Application.SBO_Application.ItemEvent += OnAppItemEvent;
        }

        private void OnAppItemEvent(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;
            if (!string.Equals(formUID, "RVCM_RET_RELEASE", StringComparison.OrdinalIgnoreCase)) return;
            if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED && pVal.BeforeAction)
            {
                if (pVal.ItemUID == "btnOk")
                {
                    TryRelease();
                }
                else if (pVal.ItemUID == "btnCancel")
                {
                    _form.Close();
                }
            }
        }

        private void TryRelease()
        {
            var contract = _txtContract.Value?.Trim();
            if (string.IsNullOrEmpty(contract))
            {
                SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage("Contract Code is required.", BoMessageTime.bmt_Short, true);
                return;
            }
            if (!decimal.TryParse(_txtAmount.Value?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
            {
                SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage("Enter a positive numeric amount.", BoMessageTime.bmt_Short, true);
                return;
            }
            var reference = _txtRef.Value?.Trim() ?? string.Empty;

            try
            {
                var di = (DI.Company)SAPbouiCOM.Framework.Application.SBO_Application.Company.GetDICompany();
                var db = new ContractManagement.Infrastructure.Data.DiRecordsetHanaDb(di);
                var amtStr = amount.ToString(CultureInfo.InvariantCulture);
                var refSql = reference.Replace("'", "''");
                var ctrSql = contract.Replace("'", "''");
                db.ExecuteNonQuery($"CALL RVCM_RELEASE_RETENTION('{ctrSql}', {amtStr}, '{refSql}')");
                SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage("Retention released and balances updated.", BoMessageTime.bmt_Short, false);
            }
            catch (Exception ex)
            {
                SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage($"Release failed: {ex.Message}", BoMessageTime.bmt_Long, true);
            }
        }
    }
}
