using System;
using Doodle;
using Doodle.CommandLineUtils;
using NssIntegrationCommon;

namespace Test
{
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
            SvnUtil.Init("E:\\E_trunk\\Tools\\Sorcery\\ThirdParty\\svn_bin\\svn.exe");

            SvnUtil.Checkout("http://tc-svn.tencent.com/ied/ied_nssclient_rep/nssclient_proj/trunk/Tools/BuildTools/Config",
                "E:\\test");
        }
    }
}
