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

            //app.Argument("argument1", "参数1", true);
            app.Option("-opt1", "可选参数1", CommandOptionType.SingleValue);

            

            app.Execute(args);

            Console.WriteLine("Remain:");
            app.RemainingArguments.ForEach(str => Console.WriteLine(str));

            return 0;
        }
    }
}
