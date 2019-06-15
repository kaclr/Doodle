using System;
using Doodle;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            CLApp.rootCommand = new Microsoft.Extensions.CommandLineUtils.CommandLineApplication(false)
            {
                Name = "Sample",
                FullName = "Sample of Doodle",
                ShortVersionGetter = () => "1.0.0.0"
            };

            CLApp.rootCommand.Argument("argument1", "参数1");
            CLApp.rootCommand.Option("-option1", "可选参数1", Microsoft.Extensions.CommandLineUtils.CommandOptionType.SingleValue);

            CLApp.Exec(args);
        }
    }
}
