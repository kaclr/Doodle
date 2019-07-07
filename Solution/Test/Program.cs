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
            //Logger.SetLogFile("init.log");
            //SvnUtil.Init("E:\\E_trunk\\Tools\\Sorcery\\ThirdParty\\svn_bin\\svn.exe");

            //CLApp.appName = "Test";

            //CLApp.AddSubCommand(getSvnLastChangedRev);



            //CLApp.Launch(args);

            //CLApp.appName = "Test";
            var envConfigOpt = CLApp.AddRootCommandOption(new Option("-envConfig", "环境配置文件", OptionType.SingleValue));
            CLApp.OnRootCommandExecute(() =>
            {
                Console.WriteLine($"OnRootCommandExecute, envConfig: {envConfigOpt.value}");
            });
            

            var getSvnLastChangedRev = new Command("GetSvnLastChangedRev");
            var pathOrUrl = getSvnLastChangedRev.AddArgument(new Argument("pathOrUrl", "路径或者URL", false));
            getSvnLastChangedRev.OnExecute(() =>
            {

                Console.WriteLine($"GetSvnLastChangedRev!");

                return 0;
            });
            CLApp.AddCommand(getSvnLastChangedRev);

            CLApp.Launch(args);
        }
    }
}
