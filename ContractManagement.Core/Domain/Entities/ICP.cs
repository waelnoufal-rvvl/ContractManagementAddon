using ContractManagement.Core.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ContractManagement.Core.Domain.Entities
{
    public class ICP
    {
        public string Code { get; set; }
        public string ContractCode { get; set; }
        public DateTime? CertDate { get; set; }
        public string Currency { get; set; }
        public decimal RetentionPct { get; set; }
        public decimal TaxRate { get; set; }
        public ICPStatus Status { get; set; }
        public List<ICPLine> Lines { get; set; } = new List<ICPLine>();
        public List<Deduction> Deductions { get; set; } = new List<Deduction>();
    }
}

