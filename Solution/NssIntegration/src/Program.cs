using System;
using System.Collections.Generic;
using System.IO;
using Doodle;
using Doodle.CommandLineUtils;
using Newtonsoft.Json;

namespace NssIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            SpaceUtil.SetTempSpace("temp");

            var logFile = SpaceUtil.GetTempPath($"NssIntegrationStart_{DateTime.Now.ToString("yyyyMMddhhmmss")}.log");
            Logger.SetLogFile("NssIntegrationStart", new LogFile(logFile));
            Logger.BeginMuteConsoleOutput();

            CLApp.Init("NssIntegration");

            var envConfigPath = SpaceUtil.GetBasePath($"Config{Path.DirectorySeparatorChar}EnvConfig.json");
            if (!File.Exists(envConfigPath))
            {
                throw new Exception($"找不到'{envConfigPath}'文件！");
            }

            JsonSerializer jsonSerializer = new JsonSerializer();
            Dictionary<string, string> dicEnvConfig = (Dictionary<string, string>)jsonSerializer.Deserialize(new StreamReader(envConfigPath), typeof(Dictionary<string, string>));

            SvnUtil.Init(() => GetEnv(envConfigPath, dicEnvConfig, "SvnBin"));

            CLApp.AddCommand(new Command("Test")).OnExecute(() =>
            {
                Console.WriteLine("Test!");
                return 0;
            });

            Logger.SetLogFile("NssIntegrationStart", null);
            Logger.EndMuteConsoleOutput();

            CLApp.Launch(args);
        }

        private static string GetEnv(string envConfigPath, Dictionary<string, string> dicEnvConfig, string key)
        {
            if (!dicEnvConfig.TryGetValue(key, out var value))
            {
                throw new Exception($"'{envConfigPath}'中缺少'{key}'的配置！");
            }
            return value;
        }
    }
}
