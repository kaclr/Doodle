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
        private static CommandArgument s_fakeGlobalArgument;
        private static List<string> s_originArgs;
        private static readonly List<CommandOption> s_globalOptions = new List<CommandOption>();

        public static void Exec(string[] args)
        {
            s_originArgs = new List<string>(args);

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

            // 这个假的argument是为了吃掉所有输入的argument，不然无法匹配到option。
            s_fakeGlobalArgument = s_globalCommand.Argument(null, null, true);

            var helpOpt = s_globalCommand.Option("-h|--help", "帮助", CommandOptionType.NoValue);
            s_globalOptions.Add(helpOpt);

            var logFileOpt = s_globalCommand.Option("-l|--logFile", "日志文件", CommandOptionType.SingleValue);
            s_globalOptions.Add(logFileOpt);


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
            // 返回1代表执行了某些全局命令，就不要在执行用户命令了。
            if (s_globalCommand.Execute(args) == 1)
            {
                return null;
            }

            foreach (var opt in s_globalOptions)
            {
                ConsumeOriginArgs(opt);
            }

            Logger.Log("Remaining:");
            s_originArgs.ForEach(arg => Logger.Log(arg));

            return s_originArgs.ToArray();
        }

        private static void ConsumeOriginArgs(CommandOption option)
        {
            if (option.OptionType == CommandOptionType.MultipleValue)
            {
                throw new NotImplementedException("MultipleValue hasn't implemented for globalOptions!");
            }

            var templates = option.Template.Split('|');

            var templateIndex = s_originArgs.FindIndex(arg => Array.FindIndex<string>(templates, template => template == arg) >= 0);
            if (templateIndex >= 0)
            {// 有option
                var consumeCount = option.OptionType == CommandOptionType.SingleValue ? 1 : 2;
                if (templateIndex + consumeCount >= s_originArgs.Count)
                {
                    // 不该出现，因为如果参数不够的话，CommandLineUtils解析时内部就会报错。
                    throw new ImplException();
                }

                s_originArgs.RemoveRange(templateIndex, consumeCount);
            }
        }
    }
}
