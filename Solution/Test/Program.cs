using System;
using System.IO;
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
            Logger.SetLogFile("init.log");
            SvnUtil.Init("E:\\E_trunk\\Tools\\Sorcery\\ThirdParty\\svn_bin\\svn.exe");

            CLApp.appName = "Test";

            var getSvnLastChangedRev = new Command("GetSvnLastChangedRev");
            var pathOrUrl = getSvnLastChangedRev.AddArgument(new Argument("pathOrUrl", "路径或者URL", false));
            getSvnLastChangedRev.OnExecute(() =>
            {
                Logger.VerboseLog("123");

                Logger.ToggleConsoleOutput(false);

                Logger.VerboseLog("bbb");

                var info = SvnUtil.GetSvnInfo((string)pathOrUrl.value);
                Console.WriteLine(info.lastChangedRev);
                return 0;
            });
            CLApp.AddSubCommand(getSvnLastChangedRev);

            

            CLApp.Launch(args);
        }

        private static void InitCLApp()
        {
            Logger.SetLogFile("init.log");

            SvnUtil.Init("E:\\E_trunk\\Tools\\Sorcery\\ThirdParty\\svn_bin\\svn.exe");

            CLApp.appName = "Test";
            CLApp.Init();

            Logger.SetLogFile(null);
        }
    }
}
