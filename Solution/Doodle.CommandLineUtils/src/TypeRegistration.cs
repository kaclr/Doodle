using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public static class TypeRegistration
    {
        private readonly static Dictionary<Type, Func<Type, string, object>> s_type2Converter;

        static TypeRegistration()
        {
            s_type2Converter = new Dictionary<Type, Func<Type, string, object>>
            {
                // 注册基本类型的converter
                { typeof(string), (type, str) => str },
                { typeof(int), (type, str) => int.Parse(str) },
                { typeof(double), (type, str) => double.Parse(str) },
                { typeof(float), (type, str) => float.Parse(str) },
                { typeof(bool), (type, str) =>
                    {
                        if (str == "true")
                        {
                            return true;
                        }
                        else if (str == "false")
                        {
                            return false;
                        }
                        else
                        {
                            throw new CommandLineParseException($"Bool value can only be 'true' of 'false', input is '{str}'");
                        }
                    }
                },
                { typeof(Enum), (type, str) => Enum.Parse(type, str) },
            };
        }

        public static void RegisterConverter(Type type, Func<Type, string, object> converter)
        {
            if (s_type2Converter.ContainsKey(type))
            {
                throw new ArgumentException($"Type '{type}' has already register a converter!");
            }

            s_type2Converter.Add(type, converter);
        }

        internal static bool ContainsConverter(Type type)
        {
            if (type.BaseType == typeof(Enum))
            {
                type = typeof(Enum);
            }

            return s_type2Converter.ContainsKey(type);
        }

        internal static Func<Type, string, object> GetConverter(Type type)
        {
            if (type.BaseType == typeof(Enum))
            {
                type = typeof(Enum);
            }

            return s_type2Converter[type];
        }
    }
}
