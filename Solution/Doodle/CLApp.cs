using Microsoft.Extensions.CommandLineUtils;
using System;

namespace Doodle
{
    /// <summary>
    /// command line application
    /// </summary>
    public static class CLApp
    {
        private const string HELP_TEMPLATES = "-h|--help";

        public static CommandLineApplication rootCommand { get; set; }
        private static CommandLineApplication s_globalCommand;

        public static void Exec(string[] args)
        {
            Init();

            var remainArgs = ExecGlobalCommand(args);
            if (remainArgs != null)
            {
                rootCommand.Execute(args);
            }
        }

        private static void Init()
        {
            s_globalCommand = new CommandLineApplication(false);

            s_globalCommand.Argument(null, null, true);
            var helpOpt = s_globalCommand.Option("-h|--help", "帮助", CommandOptionType.NoValue);
            var logFileOpt = s_globalCommand.Option("-l|--logFile", "日志文件", CommandOptionType.SingleValue);

            s_globalCommand.OnExecute(() =>
            {
                if (helpOpt.HasValue())
                {
                    rootCommand.ShowRootCommandFullNameAndVersion();

                    var rootCommandHelpText = rootCommand.GetHelpText();
                    rootCommandHelpText = rootCommandHelpText.Insert(rootCommandHelpText.IndexOf($"Usage: {rootCommand.Name}", StringComparison.Ordinal) + $"Usage: {rootCommand.Name}".Length + 1, "[globalOptions] ");

                    Console.WriteLine("==============");
                    Console.WriteLine(rootCommandHelpText);
                    Console.WriteLine("==============");


                    Console.WriteLine($"Usage: {rootCommand.Name} [globalOptions] [command]{Environment.NewLine}");

                    Console.WriteLine("GlobalOptions:");
                    foreach(var opt in s_globalCommand.Options)
                    {
                        Console.WriteLine($"  {opt.Template}  {opt.Description}");
                    }

                    Console.WriteLine("");

                    rootCommand.ShowHelp();

                    return 1;
                }

                Console.WriteLine($"设置全局参数：");
                Console.WriteLine($"    logFile，hasValue: {logFileOpt.HasValue()}, value: {logFileOpt.Value()}");

                return 0;
            });

            // 配置help
            s_globalCommand.Command("help", c =>
            {
                c.ShowInHelpText = false;
                var commandNeedHelp = c.Argument("commandNeedHelp", "需要显示帮助的命令");

                c.OnExecute(() =>
                {
                    Console.WriteLine($"显示'{commandNeedHelp.Value}'的帮助");

                    return 1;
                });
            }, false);
        }

        private static string[] ExecGlobalCommand(string[] args)
        {
            if (s_globalCommand.Execute(args) == 1)
            {
                return null;
            }

            return args;
        }
    }
}
