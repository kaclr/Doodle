using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public static class CLTypeRegistration
    {
        private readonly static Dictionary<Type, ICLTypeHandle> s_type2TypeHandle;

        static CLTypeRegistration()
        {
            s_type2TypeHandle = new Dictionary<Type, ICLTypeHandle>
            {
                // 注册基本类型的converter
                { typeof(string), new CLStringHandle() },
            //    { typeof(string), (type, str) => str },
            //    { typeof(int), (type, str) => int.Parse(str) },
            //    { typeof(double), (type, str) => double.Parse(str) },
            //    { typeof(float), (type, str) => float.Parse(str) },
            //    { typeof(bool), (type, str) =>
            //        {
            //            if (str == "true")
            //            {
            //                return true;
            //            }
            //            else if (str == "false")
            //            {
            //                return false;
            //            }
            //            else
            //            {
            //                throw new CommandLineParseException($"Bool value can only be 'true' of 'false', input is '{str}'");
            //            }
            //        }
            //    },
            //    { typeof(Enum), (type, str) => Enum.Parse(type, str) },
            };
        }

        public static void Register(Type type, ICLTypeHandle typeHandle)
        {
            if (s_type2TypeHandle.ContainsKey(type))
            {
                throw new ArgumentException($"Type '{type}' has already registered!");
            }

            s_type2TypeHandle.Add(type, typeHandle);
        }

        internal static bool ContainsConverter(Type type)
        {
            return s_type2TypeHandle.ContainsKey(GetTypeResolved(type));
        }

        internal static ICLTypeHandle Get(Type type)
        {
            return s_type2TypeHandle[GetTypeResolved(type)];
        }

        private static Type GetTypeResolved(Type type)
        {
            if (type.BaseType == typeof(Enum))
            {// 枚举值的特写
                type = typeof(Enum);
            }
            return type;
        }
    }
}
