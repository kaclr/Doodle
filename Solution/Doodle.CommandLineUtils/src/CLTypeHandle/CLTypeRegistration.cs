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
                // 注册基本类型
                { typeof(string), new CLStringHandle() },
                { typeof(int), new CLIntHandle() },
                { typeof(double), new CLDoubleHandle() },
                { typeof(float), new CLFloatHandle() },
                { typeof(Enum), new CLEnumHandle() },
                { typeof(bool), new CLBoolHandle() },
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
