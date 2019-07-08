using System;
using System.IO;
using Doodle;
using Doodle.CommandLineUtils;
using NssIntegration;

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
            Logger.verbosity = Verbosity.Verbose;
            Logger.Log("111");

            Logger.SetLogFile("verbose", new LogFile("verbose.log") { verbosity = Verbosity.Verbose });

            Logger.VerboseLog("222");

            Logger.BeginMuteConsoleOutput();

            Logger.Log("333");

            Logger.EndMuteConsoleOutput();

            Logger.Log("444");
        }
    }
}
