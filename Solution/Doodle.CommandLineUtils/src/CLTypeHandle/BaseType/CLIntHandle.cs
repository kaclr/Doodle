using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class CLIntHandle : ICLTypeHandle
    {
        public string[] GetLimitValues(Type type)
        {
            return null;
        }

        public object GetValue(Type type, string rawValue)
        {
            return int.Parse(rawValue);
        }

        public bool IsValueQualified(Type type, string rawValue, ref string error)
        {
            if (!int.TryParse(rawValue, out _))
            {
                error = $"'{rawValue}' is not a int!";
                return false;
            }
            return true;
        }
    }
}
