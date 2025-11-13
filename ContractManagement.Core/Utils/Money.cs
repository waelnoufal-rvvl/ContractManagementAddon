using System;

namespace ContractManagement.Core.Utils
{
    public static class Money
    {
        public static decimal Round2(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}

