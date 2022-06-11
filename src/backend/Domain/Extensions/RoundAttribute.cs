using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Extensions
{
    public class RoundAttribute: Attribute
    {
        public int Decimals { get; private set; }

        public RoundAttribute(int decimals)
        {
            Decimals = decimals;
        }
    }
}
