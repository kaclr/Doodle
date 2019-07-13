using System;
using System.Collections.Generic;
using System.IO;
using Doodle;
using Doodle.CommandLineUtils;
using Newtonsoft.Json;
using NssIntegration;

namespace Test
{
    public class TestClass
    {
        public static implicit operator TestClass(string str) => new TestClass(str);

        private string m_str;

        public TestClass(string str)
        {
            m_str = str;
        }
    }

    public class DerivedClass : TestClass
    {
        public DerivedClass(string str) : base(str)
        {
        }
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            Logger.verbosity = Verbosity.Verbose;

            var type = typeof(NssUnityProj);
            var obj = Activator.CreateInstance(type);
            var perp = type.GetProperty("value", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            perp.SetValue(obj, "123");
        }
    }
}
