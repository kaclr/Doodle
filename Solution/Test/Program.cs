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

            app.Description = "asdgasdhasdh";
            app.Name = "Test";
            app.FullName = "FullTest";
            app.LongVersionGetter = () => "1.2.3.4";
            app.ShortVersionGetter = () => "5.6";

            app.ExtendedHelpText = "123123";

            app.Syntax = "123123123";


            app.Command("UserCommand1", c =>
            {
                c.OnExecute(() =>
                {
                    Console.WriteLine("UserCommand1!");
                    return 0;
                });
            });


            app.Execute(args);


            app.ShowRootCommandFullNameAndVersion();
            app.ShowHint();
            app.ShowVersion();



            return 0;
        }
    }
}
