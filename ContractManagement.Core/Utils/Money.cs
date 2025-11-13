using System;

namespace ContractManagement.Core.Utils
{
    internal static class Money
    {
        public static decimal Round2(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}

