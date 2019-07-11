using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class CLStringHandle : ICLTypeHandle
    {
        public string[] GetLimitValues(Type type)
        {
            return null;
        }

        public object GetValue(Type type, string rawValue)
        {
            return rawValue;
        }

        public bool IsValueQualified(Type type, string rawValue, ref string error)
        {
            return true;
        }
    }
}
