using System;
using System.Collections.Generic;
using System.Text;

namespace NssIntegration
{
    public class NssIntegrationException : Exception
    {
        public NssIntegrationException(string message) : base(message)
        {
        }

        public NssIntegrationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
