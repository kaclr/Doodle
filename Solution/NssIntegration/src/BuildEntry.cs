using Doodle;
using Doodle.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NssIntegration
{
    internal static class BuildEntry
    {
        private static void Build(
            [ParameterConfiguration("Nss客户端工程根目录")] string nssUnityProj,
            [ParameterConfiguration("平台")] BuildTarget buildTarget,
            [ParameterConfiguration("是否使用唯一的构建版本号", optionTemplate = "-uniqueVersion")] bool uniqueVersion,
            [ParameterConfiguration("客户端Tools目录路径", optionTemplate = "-toolsDir")] string toolsDir = null,
            [ParameterConfiguration("构建配置文件路径", optionTemplate = "-buildConfig")] string buildConfigPath = "Config/BuildConfig.json",
            [ParameterConfiguration("输入的IFS文件路径", optionTemplate = "-ifs")] string ifsPath = null,
            [ParameterConfiguration("构建模式", optionTemplate = "-buildMode")] BuildMode buildMode = BuildMode.Debug,
            [ParameterConfiguration("版本线", optionTemplate = "-verLine")] VerLine verLine = VerLine.DB,
            [ParameterConfiguration("包类型", optionTemplate = "-packageType")] PackageType packageType = PackageType.Normal,
            [ParameterConfiguration("构建选项", optionTemplate = "-buildOption")] BuildOption buildOption = BuildOption.None,
            [ParameterConfiguration("输出目录", optionTemplate = "-outputDir")] string outputDir = "Output"
            )
        {
            if (string.IsNullOrEmpty(toolsDir))
            {
                toolsDir = Path.Combine(nssUnityProj, "../Tools");
            }

            var buildConfig = new BuildConfig(buildConfigPath);
            var unityExePath = buildConfig.Get<string>("UnityExe");
            var svnRev = SvnUtil.GetSvnInfo(nssUnityProj).lastChangedRev;

            string versionJsonPath = null;
            if (uniqueVersion)
            {
                versionJsonPath = BuildProcedure.PrepareUniqueVersionJson(buildTarget, Path.Combine(nssUnityProj, "Version"));
            }
            else
            {
                versionJsonPath = Path.Combine(nssUnityProj, "Version", ClientVersion.GetVersionFileName(buildTarget));
            }

            BuildAppResult buildAppResult = null;
            if (buildTarget == BuildTarget.Android)
            {
                buildAppResult = BuildProcedure.BuildApk(unityExePath, nssUnityProj, buildMode, true,
                    versionJsonPath, verLine, packageType, buildOption, toolsDir, outputDir, svnRev);
            }
            else
            {
                throw new NotImplementedException();
            }
            
            if (File.Exists(ifsPath))
            {
                Logger.Log($"使用输入的IFS文件：{ifsPath}...");

                ifsPath = IFSUtil.ModifyIFSVersion(ifsPath, buildTarget, buildAppResult.appVersion);
            }
            else
            {
                throw new NotImplementedException();
            }

            BuildProcedure.AssemblyApk(buildAppResult.appPath, 
                NssHelper.GetStandardAppName(buildTarget, buildMode, verLine, buildAppResult.defaultTDir, buildAppResult.appVersion, svnRev), 
                ifsPath);
        }
    }
}
