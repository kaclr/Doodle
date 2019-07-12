using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class CLEnumHandle : ICLTypeHandle
    {
        public string[] GetLimitValues(Type type)
        {
            return Enum.GetNames(type);
        }

        public object GetValue(Type type, string rawValue)
        {
            return Enum.Parse(type, rawValue);
        }

        public bool IsValueQualified(Type type, string rawValue, ref string error)
        {
            var limitValues = GetLimitValues(type);

            if (Array.FindIndex<string>(limitValues, v => v == rawValue) < 0)
            {// 找不到任何一个匹配的值
                error = $"'{rawValue}' is not qualifed value in '{ImplementHelper.GetLimitValuesText(limitValues)}'!";
                return false;
            }
            return true;
        }
    }
}
