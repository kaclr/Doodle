using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Doodle
{
    public static class CustomAttributeExtensions
    {
        public static T GetCustomAttribute<T>(this ParameterInfo element) 
            where T : Attribute
        {
            foreach (var attr in element.GetCustomAttributes(typeof(T), true))
            {
                if (typeof(T).IsAssignableFrom(attr.GetType()))
                {
                    return (T)attr;
                }
            }

            return null;
        }
    }
}
