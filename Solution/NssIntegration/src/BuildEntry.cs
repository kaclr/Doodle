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
            [NssConceptConfiguration(NssConcept.NssUnityProj)] string nssUnityProj,
            [NssConceptConfiguration(NssConcept.BuildTarget)] BuildTarget buildTarget,
            [ParameterConfiguration("是否使用唯一的构建版本号", optionTemplate = "-uniqueVersion")] bool uniqueVersion,
            [NssConceptConfiguration(NssConcept.ToolsDir, optionTemplate = "-toolsDir")] string toolsDir = null,
            [NssConceptConfiguration(NssConcept.BuildConfig, optionTemplate = "-buildConfig")] string buildConfigPath = "Config/BuildConfig.json",
            [NssConceptConfiguration(NssConcept.IFS, optionTemplate = "-ifs")] string ifsPath = null,
            [NssConceptConfiguration(NssConcept.BuildMode, optionTemplate = "-buildMode")] BuildMode buildMode = BuildMode.Debug,
            [NssConceptConfiguration(NssConcept.VerLine, optionTemplate = "-verLine")] VerLine verLine = VerLine.DB,
            [NssConceptConfiguration(NssConcept.PackageType, optionTemplate = "-packageType")] PackageType packageType = PackageType.Normal,
            [NssConceptConfiguration(NssConcept.BuildOption, optionTemplate = "-buildOption")] BuildOption buildOption = BuildOption.None,
            [ParameterConfiguration("输出目录", optionTemplate = "-outputDir")] string outputDir = "Output"
            )
        {
            if (string.IsNullOrEmpty(toolsDir))
            {
                toolsDir = Path.Combine(nssUnityProj, "../Tools");
            }

            outputDir = Path.GetFullPath(outputDir);
            DirUtil.CreateEmptyDir(outputDir);

            var buildConfig = new BuildConfig(buildConfigPath);
            var unityExePath = NssConceptHelper.CheckConceptThrow<string>(NssConcept.UnityExe, buildConfig.Get<string>("UnityExe"));
            var svnRev = -1;
            try
            {
                svnRev = SvnUtil.GetSvnInfo(nssUnityProj).lastChangedRev;
            }
            catch { }

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
                var sdkRoot = buildConfig.Get<string>("AndroidSDKRoot");
                var ndkRoot = buildConfig.Get<string>("AndroidNDKRoot");
                var jdkRoot = buildConfig.Get<string>("JDKRoot");

                buildAppResult = BuildProcedure.BuildApk(unityExePath, nssUnityProj, buildMode, true, sdkRoot, ndkRoot, jdkRoot,
                    versionJsonPath, verLine, packageType, buildOption, toolsDir, outputDir, svnRev);

                //buildAppResult = new BuildAppResult()
                //{
                //    appPath = "H:\\branches\\H_trunk\\NssUnityProj\\Output\\NSS_Debug_Test_DB_1.14.0.22330_349712.apk",
                //    appVersion = "1.14.0.22330",
                //    defaultTDir = "Test",
                //};
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
                Path.Combine(outputDir, NssHelper.GetStandardAppName(buildTarget, buildMode, verLine, buildAppResult.defaultTDir, buildAppResult.appVersion, svnRev)), 
                ifsPath);
        }
    }
}
