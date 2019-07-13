using Doodle.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NssIntegration
{
    public class CLNssConceptHandle : ICLTypeHandle
    {
        public string[] GetLimitValues(Type type)
        {
            throw new NotImplementedException();
        }

        public object GetValue(Type type, string rawValue)
        {
            throw new NotImplementedException();
        }

        public bool IsValueQualified(Type type, string rawValue, ref string error)
        {
            throw new NotImplementedException();
        }
    }
}
