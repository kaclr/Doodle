using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class CLDoubleHandle : ICLTypeHandle
    {
        public string[] GetLimitValues(Type type)
        {
            return null;
        }

        public object GetValue(Type type, string rawValue)
        {
            return double.Parse(rawValue);
        }

        public bool IsValueQualified(Type type, string rawValue, ref string error)
        {
            if (!double.TryParse(rawValue, out _))
            {
                error = $"'{rawValue}' is not a double!";
                return false;
            }
            return true;
        }
    }
}
