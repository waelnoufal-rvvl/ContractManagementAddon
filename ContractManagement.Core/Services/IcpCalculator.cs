using System;
using System.Linq;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Services.Models;
using ContractManagement.Core.Utils;

namespace ContractManagement.Core.Services
{
    public class IcpCalculator
    {
        public IcpCalculationResult Calculate(ICP icp)
        {
            if (icp == null) throw new ArgumentNullException(nameof(icp));

            var grossValue = icp.Lines
                .Select(l => Money.Round2(l.ThisQty * l.UnitPrice))
                .DefaultIfEmpty(0m)
                .Sum();

            var retentionAmt = Money.Round2(grossValue * (icp.RetentionPct / 100m));

            var adv = icp.Deductions
                .Where(d => d.Type == DeductionType.Advance)
                .Select(d => d.Amount)
                .DefaultIfEmpty(0m)
                .Sum();

            var material = icp.Deductions
                .Where(d => d.Type == DeductionType.Material)
                .Select(d => d.Amount)
                .DefaultIfEmpty(0m)
                .Sum();

            var other = icp.Deductions
                .Where(d => d.Type == DeductionType.Other)
                .Select(d => d.Amount)
                .DefaultIfEmpty(0m)
                .Sum();

            var subtotal = Money.Round2(grossValue - adv - retentionAmt - material - other);
            if (subtotal < 0)
                throw new InvalidOperationException("Net amount cannot be negative. Total deductions exceed gross value.");

            var vatAmount = Money.Round2(subtotal * (icp.TaxRate / 100m));
            var netPayment = Money.Round2(subtotal + vatAmount);

            return new IcpCalculationResult
            {
                GrossValue = grossValue,
                RetentionAmount = retentionAmt,
                AdvanceDeduct = adv,
                MaterialDeduct = material,
                OtherDeduct = other,
                Subtotal = subtotal,
                VatAmount = vatAmount,
                NetPayment = netPayment
            };
        }
    }
}

