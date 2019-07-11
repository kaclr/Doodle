using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class CLFloatHandle : ICLTypeHandle
    {
        public string[] GetLimitValues(Type type)
        {
            return null;
        }

        public object GetValue(Type type, string rawValue)
        {
            return float.Parse(rawValue);
        }

        public bool IsValueQualified(Type type, string rawValue, ref string error)
        {
            if (!float.TryParse(rawValue, out _))
            {
                error = $"'{rawValue}' is not a double!";
                return false;
            }
            return true;
        }
    }
}
