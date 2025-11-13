using System;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using DI = SAPbobsCOM;
using ContractManagement.Infrastructure.Sap;

namespace ContractManagementAddon
{
    internal class ConfigForm
    {
        private Form _form;
        private EditText _txtAccount;
        private CheckBox _chkDraft;

        public void Show()
        {
            var app = SAPbouiCOM.Framework.Application.SBO_Application;
            var fcp = (FormCreationParams)app.CreateObject(BoCreatableObjectType.cot_FormCreationParams);
            fcp.UniqueID = "RVCM_CFG_FORM";
            fcp.BorderStyle = BoFormBorderStyle.fbs_Sizable;
            fcp.FormType = "RVCM_CFG_FORM";
            _form = app.Forms.AddEx(fcp);
            _form.Title = "Contract Management Settings";
            _form.Width = 380;
            _form.Height = 180;

            int left = 10, top = 20, w = 120, h = 15, gap = 10;

            var lblAcct = _form.Items.Add("lblAcct", BoFormItemTypes.it_STATIC);
            lblAcct.Left = left; lblAcct.Top = top; lblAcct.Width = w; lblAcct.Height = h;
            ((StaticText)lblAcct.Specific).Caption = "Deduction Account";

            var txtAcct = _form.Items.Add("txtAcct", BoFormItemTypes.it_EDIT);
            txtAcct.Left = left + w + gap; txtAcct.Top = top; txtAcct.Width = 200; txtAcct.Height = h;
            _txtAccount = (EditText)txtAcct.Specific;

            top += h + gap;
            var lblDraft = _form.Items.Add("lblDraft", BoFormItemTypes.it_STATIC);
            lblDraft.Left = left; lblDraft.Top = top; lblDraft.Width = w; lblDraft.Height = h;
            ((StaticText)lblDraft.Specific).Caption = "Post As Draft";

            var chkDraft = _form.Items.Add("chkDraft", BoFormItemTypes.it_CHECK_BOX);
            chkDraft.Left = left + w + gap; chkDraft.Top = top; chkDraft.Width = 100; chkDraft.Height = h;
            _chkDraft = (CheckBox)chkDraft.Specific;

            top += h + (2 * gap);
            var btnOk = _form.Items.Add("btnOk", BoFormItemTypes.it_BUTTON);
            btnOk.Left = left + w + gap; btnOk.Top = top; btnOk.Width = 70; btnOk.Height = 20;
            ((Button)btnOk.Specific).Caption = "Save";
            var btnCancel = _form.Items.Add("btnCancel", BoFormItemTypes.it_BUTTON);
            btnCancel.Left = btnOk.Left + 80; btnCancel.Top = top; btnCancel.Width = 70; btnCancel.Height = 20;
            ((Button)btnCancel.Specific).Caption = "Cancel";

            _form.Visible = true;

            LoadValues();

            SAPbouiCOM.Framework.Application.SBO_Application.ItemEvent += OnAppItemEvent;
        }

        private void LoadValues()
        {
            var di = (DI.Company)SAPbouiCOM.Framework.Application.SBO_Application.Company.GetDICompany();
            var svc = new ContractManagement.Infrastructure.Sap.SettingsService(di);
            var s = svc.Get();
            _txtAccount.Value = s.DeductionAccount ?? string.Empty;
            _chkDraft.Checked = s.PostAsDraft;
        }

        private void OnAppItemEvent(string formUID, ref ItemEvent pVal, out bool bubbleEvent)
        {
            bubbleEvent = true;
            if (!string.Equals(formUID, "RVCM_CFG_FORM", StringComparison.OrdinalIgnoreCase)) return;
            if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED && pVal.BeforeAction)
            {
                if (pVal.ItemUID == "btnOk")
                {
                    try
                    {
                        var di = (DI.Company)SAPbouiCOM.Framework.Application.SBO_Application.Company.GetDICompany();
                        var svc = new ContractManagement.Infrastructure.Sap.SettingsService(di);
                        svc.Save(new ContractManagement.Infrastructure.Sap.AddonSettings
                        {
                            DeductionAccount = _txtAccount.Value?.Trim(),
                            PostAsDraft = _chkDraft.Checked
                        });
                        SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage("Settings saved.", BoMessageTime.bmt_Short, false);
                    }
                    catch (Exception ex)
                    {
                        SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage($"Save failed: {ex.Message}", BoMessageTime.bmt_Long, true);
                    }
                }
                else if (pVal.ItemUID == "btnCancel")
                {
                    _form.Close();
                }
            }
        }
    }
}
