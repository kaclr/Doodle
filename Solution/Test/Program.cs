using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Test
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.HelpOption("-h|--help");

            var gloO1 = app.Option("-go1", "global o1", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                Console.WriteLine($"gloO1: {gloO1.Value()}");
                return 0;
            });
            




            app.Command("TestCommand", c =>
            {
                c.Description = "这是一个测试命令。";
                c.HelpOption("-h|--help");

                var arg1 = c.Argument("arg1", "固定参数1");
                var opt1 = c.Option("-o1|--opt1", "可选参数1", CommandOptionType.SingleValue);

                c.OnExecute(() =>
                {
                    Console.WriteLine("TestCommand执行！");
                    Console.WriteLine($"固定参数1: {arg1.Value}");
                    Console.WriteLine($"可选参数1，HasValue：{opt1.HasValue()}, Value: {opt1.Value()}");

                    return int.Parse(arg1.Value);
                });
            }, false);

            return new CommandLineApplication()
        }
    }
}
