using System;

namespace Doodle.CommandLineUtils
{
    /// <summary>
    /// Command Line Application
    /// </summary>
    public static class CLApp
    {
        public static string appName
        {
            get => m_rootCommand.name;
            set => m_rootCommand.name = value;
        }

        private static readonly Command m_rootCommand = new Command("");
        private static bool s_inited = false;

        public static int Launch(string[] args)
        {
            Init();

            return m_rootCommand.Execute(args);
        }

        public static Command AddSubCommand(Command command)
        {
            m_rootCommand.AddSubCommand(command);
            return command;
        }

        private static void Init()
        {
            if (s_inited)
                return;
            s_inited = true;

            SpaceUtil.SetTempSpace("temp");

            var verbose = m_rootCommand.AddOption(new Option("-v|--verbose", "输出冗余", OptionType.NoValue));
            m_rootCommand.OnExecute(() =>
            {
                Logger.Log($"verbose: {verbose.isSet}");

                return 0;
            });
        }
    }
}
