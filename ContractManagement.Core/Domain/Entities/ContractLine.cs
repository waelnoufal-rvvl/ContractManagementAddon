namespace ContractManagement.Core.Domain.Entities
{
    public class ContractLine
    {
        public string ContractCode { get; set; }
        public int LineNo { get; set; }
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public decimal ContractQty { get; set; }
        public decimal UnitPrice { get; set; }
        public string UoM { get; set; }
    }
}

