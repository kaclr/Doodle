using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Test
{
    class TestProgram
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication(false);
            app.HelpOption("-h|--help");

            app.Argument("argument1", "参数1", true);
            var opt1 = app.Option("--opt1|-o1", "可选参数1", CommandOptionType.SingleValue);

            app.AllowArgumentSeparator = true;




            app.Execute(args);

            Console.WriteLine($"opt1: hasValue: {opt1.HasValue()}");
            opt1.Values.ForEach(str => Console.WriteLine(str));

            Console.WriteLine("Remain:");
            app.RemainingArguments.ForEach(str => Console.WriteLine(str));

            return 0;
        }
    }
}
