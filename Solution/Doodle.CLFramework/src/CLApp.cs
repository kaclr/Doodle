using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Doodle.CommandLineUtils
{
    /// <summary>
    /// Command Line Application
    /// </summary>
    public static class CLApp
    {
        private static string appName
        {
            get => s_rootCommand.name;
            set => s_rootCommand.name = value;
        }

        private static readonly Command s_rootCommand = new Command("");
        private static Action s_onRootExecute;
        private static bool s_inited = false;
        private static bool s_hasRootCommandExecuted = false;

        public static void Init(string appName)
        {
            if (s_inited)
                return;
            s_inited = true;

            Logger.BeginMuteConsoleOutput();

            var logFileOpt = s_rootCommand.AddOption(new Option("-l|--logFile", "日志文件", OptionType.SingleValue));
            var verboseOpt = s_rootCommand.AddOption(new Option("-v|--verbose", "输出冗余", OptionType.NoValue));
            var forceConsoleOutputOpt = s_rootCommand.AddOption(new Option("-f|--forceConsoleOutput", "强制把日志输出到Console", OptionType.NoValue));
            var persistentSpaceOpt = s_rootCommand.AddOption(new Option("-p|--persistentSpace", "持久化目录", OptionType.SingleValue));

            s_rootCommand.OnExecute(() =>
            {
                if (s_hasRootCommandExecuted)
                    return 0;
                s_hasRootCommandExecuted = true;

                Logger.BeginMuteConsoleOutput();

                if (verboseOpt.isSet)
                {
                    Logger.verbosity = Verbosity.Verbose;
                }

                Logger.VerboseLog("CLApp initializing...");
                Logger.VerboseLog($"logFile: {logFileOpt.value}");
                Logger.VerboseLog($"verbose: {verboseOpt.isSet}");
                Logger.VerboseLog($"forceConsoleOutput: {forceConsoleOutputOpt.isSet}");
                Logger.VerboseLog($"persistentSpaceOpt: {persistentSpaceOpt.value}");

                if (forceConsoleOutputOpt.isSet)
                {
                    Logger.forceConsoleOutput = true;
                }

                if (persistentSpaceOpt.isSet && !string.IsNullOrEmpty((string)persistentSpaceOpt.value))
                {
                    SpaceUtil.SetPersistentSpace((string)persistentSpaceOpt.value);
                }

                string logFile = (string)logFileOpt.value;
                if (!string.IsNullOrEmpty(logFile))
                {
                    Logger.SetLogFile("UserLog", new LogFile(logFile) { verbosity = Verbosity.Verbose });
                }

                Logger.EndMuteConsoleOutput();

                s_onRootExecute?.Invoke();

                return 0;
            });

            Logger.EndMuteConsoleOutput();
        }

        public static Option AddRootCommandOption(Option option)
        {
            return s_rootCommand.AddOption(option);
        }

        public static void OnRootCommandExecute(Action onExecute)
        {
            s_onRootExecute = onExecute;
        }

        public static int Launch(string[] args)
        {
            if (!s_inited)
                throw new Exception($"{nameof(CLApp)} hasn't been inited!");

            return s_rootCommand.Execute(args);
        }

        public static int LaunchMutiCommand(string[] args)
        {
            if (!s_inited)
                throw new Exception($"{nameof(CLApp)} hasn't been inited!");

            var ret = s_rootCommand.Execute(args);
            while (ret == 0 && s_rootCommand.remainArgs.Length > 0 && s_rootCommand.remainArgs.Length != args.Length)
            {
                args = s_rootCommand.remainArgs;
                ret = s_rootCommand.Execute(args);
            }
            return ret;
        }

        public static Command AddCommand(Command command)
        {
            s_rootCommand.AddSubCommand(command);
            return command;
        }

    }
}
