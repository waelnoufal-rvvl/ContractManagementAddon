using ContractManagement.Core.Domain.Enums;

namespace ContractManagement.Core.Domain.Entities
{
    public class Deduction
    {
        public string ICPCode { get; set; }
        public DeductionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}

