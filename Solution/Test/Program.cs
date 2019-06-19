using System;
using Doodle;
using Doodle.CommandLineUtils;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var globalCommand = new Command("Test");
            var logFileOpt = globalCommand.AddOption(new Option("-l", "日志文件", OptionType.SingleValue));
            globalCommand.OnExecute(() =>
            {
                Logger.Log("GlobalCommand:");
                Logger.Log($"logFileOpt: IsSet: {logFileOpt.isSet}, Value: {logFileOpt.value}");

                return 0;
            });

            var command1 = globalCommand.AddSubCommand(new Command("Command1"));
            var opt1 = command1.AddOption(new Option("-opt1", "参数1", OptionType.SingleValue));
            command1.OnExecute(() =>
            {
                Logger.Log("Command1:");
                Logger.Log($"opt1: IsSet: {opt1.isSet}, Value: {opt1.value}");

                return 1;
            });

            Logger.Log($"return value: {globalCommand.Execute(args)}");
        }
    }
}
