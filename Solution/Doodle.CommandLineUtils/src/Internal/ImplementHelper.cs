using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    internal static class ImplementHelper
    {
        public static string GetLimitValuesText(string[] limitValues)
        {
            StringBuilder sb = new StringBuilder();
            Array.ForEach<string>(limitValues, v => sb.Append($"{v}|"));
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }
}
