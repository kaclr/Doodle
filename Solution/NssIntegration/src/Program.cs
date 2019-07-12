using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

            var logFile = SpaceUtil.GetPathInTemp($"NssIntegrationStart_{DateTime.Now.ToString("yyyyMMddhhmmss")}.log");
            Logger.SetLogFile("NssIntegrationStart", new LogFile(logFile));
            Logger.BeginMuteConsoleOutput();

            CLApp.Init("NssIntegration");

            var envConfigPath = SpaceUtil.GetPathInBase($"Config{Path.DirectorySeparatorChar}EnvConfig.json");
            Dictionary<string, string> dicEnvConfig = null;
            if (File.Exists(envConfigPath))
            {
                JsonSerializer jsonSerializer = new JsonSerializer();
                dicEnvConfig = (Dictionary<string, string>)jsonSerializer.Deserialize(new StreamReader(envConfigPath), typeof(Dictionary<string, string>));
            }

            // 初始化各种工具类
            SvnUtil.Init(() => GetEnv(envConfigPath, dicEnvConfig, "SvnBin"));
            ApkTool.Init(() => GetEnv(envConfigPath, dicEnvConfig, "ApkTool"));
            IntegrationServer.Init(() => GetEnv(envConfigPath, dicEnvConfig, "IntegrationServerAddress"));

            CLApp.AddCommand(NewMethodCommand(typeof(BuildProcedure), "AssemblyApk"));
            CLApp.AddCommand(NewMethodCommand(typeof(BuildProcedure), "PrepareVersion"));
            CLApp.AddCommand(NewMethodCommand(typeof(BuildProcedure), "ModifyMacro"));
            CLApp.AddCommand(NewMethodCommand(typeof(BuildProcedure), "BuildApk"));

            Logger.SetLogFile("NssIntegrationStart", null);
            Logger.EndMuteConsoleOutput(); 

            CLApp.Launch(args);
        }

        private static Command NewMethodCommand(Type classType, string methodName)
        {
            return MethodCommand.New(classType.GetMethod(methodName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
        }

        private static string GetEnv(string envConfigPath, Dictionary<string, string> dicEnvConfig, string key)
        {
            if (dicEnvConfig == null)
                throw new Exception($"找不到'{envConfigPath}'文件！");

            if (!dicEnvConfig.TryGetValue(key, out var value))
            {
                throw new Exception($"'{envConfigPath}'中缺少'{key}'的配置！");
            }
            return value;
        }

        private static void StatAddAssets(
            string pathAssetInfoData1, 
            string pathBundleNodeData1, 
            string nssUnityProjDir1,
            string nssUnityProjDir2,
            string pathABList,
            string outfile)
        {
            var abAssetRelation1 = new ABAssetRelation();
            abAssetRelation1.Parse(pathAssetInfoData1, pathBundleNodeData1, nssUnityProjDir1);

            Logger.Log("计算资源差异...");
            List<ABDiffInfo> abDiffInfos = new List<ABDiffInfo>();
            Dictionary<string, bool> dicAB = new Dictionary<string, bool>();
            foreach (var line in File.ReadAllLines(pathABList))
            {
                var abName = line.Split(' ', '\t')[0].Replace("AssetBundles/", "");

                var ab = abAssetRelation1.GetAB(abName);
                if (abDiffInfos.Find(item => item.name == abName) != null)
                {
                    throw new Exception($"重复的ab: {ab.name}");
                }

                var abDiffInfo = new ABDiffInfo() { name = abName };
                foreach (var assetInfo in ab.EnumTotalAssets())
                {
                    var assetPath2 = Path.Combine(nssUnityProjDir2, assetInfo.path);
                    if (!File.Exists(assetPath2))
                    {
                        abDiffInfo.AddDiffAsset(ABDiffInfo.DiffType.Add, assetInfo.path);
                    }
                    else
                    {
                        if (FileUtil.ComputeSHA1(Path.Combine(nssUnityProjDir1, assetInfo.path)) != FileUtil.ComputeSHA1(assetPath2))
                        {
                            abDiffInfo.AddDiffAsset(ABDiffInfo.DiffType.Mod, assetInfo.path);
                        }
                    }
                }


                abDiffInfos.Add(abDiffInfo);
            }

            Logger.Log("输出...");
            using (var f = new StreamWriter(outfile))
            {
                foreach (var abDiffInfo in abDiffInfos)
                {
                    f.WriteLine(abDiffInfo.name);
                    foreach (var asset in abDiffInfo.EnumDiffAssets())
                    {
                        f.WriteLine($"    {asset.Value}|{asset.Key}");
                    }
                }
            }

            //using (var f = new FileStream(outfile, FileMode.Create))
            //{
            //    BinaryFormatter binaryFormatter = new BinaryFormatter();
            //    binaryFormatter.Serialize(f, abAssetRelation);
            //}
        }
    }
}
