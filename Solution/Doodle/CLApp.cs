using Microsoft.Extensions.CommandLineUtils;

namespace Doodle
{
    /// <summary>
    /// command line application
    /// </summary>
    public static class CLApp
    {
        private const string HELP_TEMPLATES = "-?|-h|--help";

        private static CommandLineApplication s_rootCommand;

        public static void Exec(string[] args)
        {
            s_rootCommand = new CommandLineApplication();
            s_rootCommand.HelpOption(HELP_TEMPLATES);

            //s_rootCommand.Option("--logFile")
        }
    }
}
