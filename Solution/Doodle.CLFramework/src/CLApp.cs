using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        private static readonly Command m_rootCommand = new Command("");
        private static bool s_inited = false;

        public static void Init(string envConfigPath)
        {
            if (s_inited)
                return;
            s_inited = true;

            Dictionary<string, string> dicEnvConfig = null;
            using (StreamReader f = new StreamReader(envConfigPath))
            using (JsonTextReader jr = new JsonTextReader(f))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                dicEnvConfig = (Dictionary<string, string>)jsonSerializer.Deserialize(jr, typeof(Dictionary<string, string>));
            }

            SvnUtil.Init(dicEnvConfig["SvnBin"]);
            


            SpaceUtil.SetTempSpace("temp");

            var verbose = m_rootCommand.AddOption(new Option("-v|--verbose", "输出冗余", OptionType.NoValue));
            m_rootCommand.OnExecute(() =>
            {
                Logger.Log($"verbose: {verbose.isSet}");

                return 0;
            });
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

    }
}
