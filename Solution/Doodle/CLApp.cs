using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;

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
                {// 输出帮助信息

                    // 获取帮助数组
                    var rootCommandHelpText = rootCommand.GetHelpText();
                    List<string> rootCommandHelpTextLines = new List<string>(rootCommandHelpText.Split(Environment.NewLine));

                    // 添加[globalOptions]
                    var usageLineIndex = rootCommandHelpTextLines.FindIndex(line => line.StartsWith("Usage:"));
                    rootCommandHelpTextLines[usageLineIndex] = rootCommandHelpTextLines[usageLineIndex].Insert(
                        rootCommandHelpTextLines[usageLineIndex].IndexOf($"Usage: {rootCommand.Name}", StringComparison.Ordinal) + $"Usage: {rootCommand.Name}".Length + 1,
                        "[globalOptions] ");

                    // 输出帮助
                    for (int i = 0; i < rootCommandHelpTextLines.Count; ++i)
                    {
                        Console.WriteLine(rootCommandHelpTextLines[i]);
                        if (i == usageLineIndex + 1)
                        {// 输出globalOptions的参数
                            Console.WriteLine("GlobalOptions:");
                            foreach (var opt in s_globalCommand.Options)
                            {
                                Console.WriteLine($"  {opt.Template}  {opt.Description}");
                            }
                            Console.WriteLine("");
                        }
                    }

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
