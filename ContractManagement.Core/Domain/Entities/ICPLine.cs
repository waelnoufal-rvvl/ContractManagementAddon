namespace ContractManagement.Core.Domain.Entities
{
    public class ICPLine
    {
        public string ICPCode { get; set; }
        public int LineNo { get; set; }
        public decimal ThisQty { get; set; }
        public decimal UnitPrice { get; set; }
    }
}

