namespace ContractManagement.Core.Services.Models
{
    public class IcpCalculationResult
    {
        public decimal GrossValue { get; set; }
        public decimal RetentionAmount { get; set; }
        public decimal AdvanceDeduct { get; set; }
        public decimal MaterialDeduct { get; set; }
        public decimal OtherDeduct { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal NetPayment { get; set; }
    }
}

