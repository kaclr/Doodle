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
            SpaceUtil.SetTempSpace("temp");

            Logger.verbosity = Verbosity.Verbose;

            //var bin = new Executable("cmd") { printToVerbose = true };
            //File.WriteAllText(SpaceUtil.GetTempPath("tmp"), "echo 123");
            //bin.Execute($"\"{SpaceUtil.GetTempPath("tmp")}\"");

            File.WriteAllText(SpaceUtil.GetTempPath("tmp.bat"), $"@echo off{Environment.NewLine}echo 123");
            var bin = new Executable("cmd") { printToVerbose = true };
            bin.Execute($"/c \"{SpaceUtil.GetTempPath("tmp.bat")}\"");
        }
    }
}
