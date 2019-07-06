using Newtonsoft.Json;
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
        public static string appName
        {
            get => m_rootCommand.name;
            set => m_rootCommand.name = value;
        }

        private static string clAppInitLog
        {
            get;
            set;
        }

        private static readonly Command m_rootCommand = new Command("");
        private static bool s_inited = false;

        public static void Init(string appName, string envConfig)
        {
            if (s_inited)
                return;
            s_inited = true;

            SpaceUtil.SetTempSpace("temp");

            // 初始化log文件的路径
            clAppInitLog = SpaceUtil.GetTempPath($"CLAppInitLog_{Process.GetCurrentProcess().Id}_{DateTime.Now.ToString("yyyyMMddhhmmss")}.log");
            Logger.TurnOnLogFile(clAppInitLog, false);

            CLApp.appName = appName;

            if (!string.IsNullOrEmpty(envConfig))
            {
                Dictionary<string, string> dicEnvConfig = null;
                using (StreamReader f = new StreamReader(envConfig))
                using (JsonTextReader jr = new JsonTextReader(f))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    dicEnvConfig = (Dictionary<string, string>)jsonSerializer.Deserialize(jr, typeof(Dictionary<string, string>));
                }

                SvnUtil.Init(GetEnvConfigValue(envConfig, dicEnvConfig, "SvnBin"));
            }

            var logFileParam = m_rootCommand.AddOption(new Option("-l|--logFile", "日志文件", OptionType.SingleValue));
            var verboseParam = m_rootCommand.AddOption(new Option("-v|--verbose", "输出冗余", OptionType.NoValue));
 
            m_rootCommand.OnExecute(() =>
            {
                Logger.TurnOnLogFile(clAppInitLog);

                Logger.VerboseLog("CLApp initializing...");
                Logger.VerboseLog($"logFile: {logFileParam.value}");
                Logger.VerboseLog($"verbose: {verboseParam.isSet}");

                bool hasUserLogFile = false;
                string logFile = (string)logFileParam.value;
                if (!string.IsNullOrEmpty(logFile))
                {
                    Logger.TurnOnLogFile(logFile);
                    hasUserLogFile = true;
                }

                if (verboseParam.isSet)
                {
                    Logger.verbosity = Logger.Verbosity.Verbose;
                }

                if (!hasUserLogFile)
                    Logger.TurnOffLogFile();

                return 0;
            });

            Logger.TurnOffLogFile();
        }

        public static int Launch(string[] args)
        {
            if (!s_inited)
                throw new Exception($"{nameof(CLApp)} hasn't been inited!");

            return m_rootCommand.Execute(args);
        }

        public static Command AddSubCommand(Command command)
        {
            m_rootCommand.AddSubCommand(command);
            return command;
        }

        private static string GetEnvConfigValue(string envConfigFile, Dictionary<string, string> dicEnvConfig, string needKey)
        {
            if (!dicEnvConfig.TryGetValue(needKey, out string value))
            {
                throw new DoodleException($"Env config '{envConfigFile}' is missing key of '{needKey}'!");
            }
            return value;
        }
    }
}
