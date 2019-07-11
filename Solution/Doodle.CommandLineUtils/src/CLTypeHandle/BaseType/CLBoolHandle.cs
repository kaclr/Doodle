using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class CLBoolHandle : ICLTypeHandle
    {
        public string[] GetLimitValues(Type type)
        {
            return new string[] { "true", "false" };
        }

        public object GetValue(Type type, string rawValue)
        {
            if (rawValue == "true")
            {
                return true;
            }
            else if (rawValue == "false")
            {
                return false;
            }
            else
            {
                Debug.Assert(false);
                return false;
            }
        }

        public bool IsValueQualified(Type type, string rawValue, ref string error)
        {
            if (rawValue != "true" && rawValue == "false")
            {
                error = $"Bool value can only be 'true' of 'false', input value is '{rawValue}'!";
                return true;
            }

            return false;
        }
    }
}
