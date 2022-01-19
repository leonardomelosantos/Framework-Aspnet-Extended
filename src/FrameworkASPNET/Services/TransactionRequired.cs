using System;
using System.Data;

namespace FrameworkAspNetExtended.Services
{
    public class TransactionRequired : Attribute
    {
        public IsolationLevel? IsolationLevel { get; set; }

        public TransactionRequired() { }

        public TransactionRequired(IsolationLevel isolationLevel)
        {
            IsolationLevel = isolationLevel;
        }
    }
}
