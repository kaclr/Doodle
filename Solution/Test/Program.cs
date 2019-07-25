using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Doodle;
using Doodle.CommandLineUtils;
using Newtonsoft.Json;
using NssIntegration;

namespace Test
{
    class TestData
    {
        public int V;
    }

    class Program
    {

        
        static void Main(string[] args)
        {
            Logger.verbosity = Verbosity.Verbose;

            ObjDictionary<string, TestData> dic = new ObjDictionary<string, TestData>
            {
                { "a", new TestData() { V = 100 } },
                { "b", new TestData() { V = 100 } }
            };

            //Dictionary<string, int> dic = new Dictionary<string, int>
            //{
            //    { "a", 100 },
            //    { "b", 100 }
            //};

            JsonSerializer jsonSerializer = new JsonSerializer();
            jsonSerializer.Formatting = Formatting.Indented;

            using (var f = new StreamWriter("test2.json"))
            {
                jsonSerializer.Serialize(f, dic);
            }


        }
    }
}
