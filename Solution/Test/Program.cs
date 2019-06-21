using System;
using Doodle;
using Doodle.CommandLineUtils;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var command1 = new Command("Command1");
            var arg1 = command1.AddArgument(new Argument("arg1", "固定参数1", false) { defaultValue = () => "aaa" });
            //var arg2 = command1.AddArgument(new Argument("arg2", "固定参数2", false));
            var opt1 = command1.AddOption(new Option("-opt1", "参数1", OptionType.SingleValue));
            command1.OnExecute(() =>
            {
                Logger.Log("Command1:");
                Logger.Log($"arg1: Value: {arg1.value}");
               
                Logger.Log($"opt1: IsSet: {opt1.isSet}, Value: {opt1.value}");

                return 1;
            });

            CLApp.appName = "Test";
            CLApp.AddSubCommand(command1);
            CLApp.Launch(args);
        }
    }
}
