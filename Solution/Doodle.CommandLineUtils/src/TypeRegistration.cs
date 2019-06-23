using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public static class TypeRegistration
    {
        internal readonly static Dictionary<Type, Func<string, object>> s_type2Converter;

        static TypeRegistration()
        {
            s_type2Converter = new Dictionary<Type, Func<string, object>>
            {
                // 注册基本类型的converter
                { typeof(string), str => str },
                { typeof(int), str => int.Parse(str) },
                { typeof(double), str => double.Parse(str) },
                { typeof(float), str => float.Parse(str) },
            };
        }

        public static void RegisterConverter(Type type, Func<string, object> converter)
        {
            if (s_type2Converter.ContainsKey(type))
            {
                throw new ArgumentException($"Type '{type}' has already register a converter!");
            }

            s_type2Converter.Add(type, converter);
        }
    }
}
