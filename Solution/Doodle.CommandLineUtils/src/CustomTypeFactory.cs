using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public static class CustomTypeFactory
    {
        internal static readonly Dictionary<Type, ITypeConfiguration> s_dicValueType = new Dictionary<Type, ITypeConfiguration>();

        public static void Register<T>(Type type)
            where T : ITypeConfiguration, new()
        {
            if (s_dicValueType.TryGetValue(type, out ITypeConfiguration cutomType))
            {
                throw new ArgumentException($"Type '{type.Name}' has already registered!");
            }

            s_dicValueType.Add(type, new T());
        }
    }
}
