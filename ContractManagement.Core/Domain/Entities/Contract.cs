using ContractManagement.Core.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ContractManagement.Core.Domain.Entities
{
    public class Contract
    {
        public string Code { get; set; }
        public string ContractName { get; set; }
        public string ContractNameAR { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Currency { get; set; }
        public decimal TotalValue { get; set; }
        public string TaxCode { get; set; }
        public decimal RetentionPct { get; set; }
        public ContractStatus Status { get; set; }
        public List<ContractLine> Lines { get; set; } = new List<ContractLine>();
    }
}

