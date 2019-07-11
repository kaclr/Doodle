using System;
using System.Collections.Generic;
using System.IO;
using Doodle;
using Doodle.CommandLineUtils;
using Newtonsoft.Json;
using NssIntegration;

namespace Test
{
    class TestClass<T1, T2>
        where T2: class, new()
    {
        [JsonProperty]
        private int A;

        [JsonProperty]
        public int B { get; set; }

        [JsonProperty]
        private readonly Dictionary<T1, T2> m_dic;

        public Dictionary<T1, T2> dic { get { return m_dic; }  }

        public TestClass()
        {
            m_dic = new Dictionary<T1, T2>();
        }
    }

    class TestClass2
    {
        [JsonProperty]
        public string name { get; set; }
    }

    class Program
    {
        private static string Test()
        {
            return null;
        }

        private static int BuildApk(
            [ParameterConfiguration("客户端工程中的Tools目录")] string toolsDir,
            [ParameterConfiguration("", optionTemplate = "-b")] bool toolsDir2,
            [ParameterConfiguration("产物输出目录", optionTemplate = "-o")] string outputDir = "1652136")
        {
            Logger.Log("BuildApk!");
            Logger.Log("toolsDir: " + toolsDir);
            Logger.Log("toolsDir2: " + toolsDir2);
            Logger.Log("outputDir: " + outputDir);

            return 0;
        }

        private static int BuildApk2(
            string toolsDir,
            string outputDir = null)
        {
            Logger.Log("BuildApk!");
            Logger.Log("toolsDir: " + toolsDir);
            Logger.Log("outputDir: " + outputDir);

            return 0;
        }

        static void Main(string[] args)
        {
            var v = ClientVersion.New("D:\\Jieji\\NssUnityProj\\Version\\Version_Android.json");
            Console.WriteLine(v);
        }
    }
}
