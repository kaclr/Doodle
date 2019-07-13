using System;
using System.Collections.Generic;
using System.Text;

namespace NssIntegration
{
    using Doodle;
    using Doodle.CommandLineUtils;
    using System.IO;

    public class BuildAppResult
    {
        public string appPath;
        public string appVersion;
        public string defaultTDir;
    }

    static class BuildProcedure
    {
        public static void Build(
            [ParameterConfiguration("Nss客户端工程根目录")] string nssUnityProj,
            [ParameterConfiguration("平台")] BuildTarget buildTarget,
            [ParameterConfiguration("版本号", optionTemplate = "-version")] string version,
            [ParameterConfiguration("输入的IFS文件路径", optionTemplate = "-ifs")] string ifsPath,
            [ParameterConfiguration("构建配置文件路径", optionTemplate = "-buildConfig")] string buildConfig = "Config/BuildConfig.json",
            [ParameterConfiguration("构建模式", optionTemplate = "-buildMode")] BuildMode buildMode = BuildMode.Debug,
            [ParameterConfiguration("版本线", optionTemplate = "-verLine")] VerLine verLine = VerLine.DB,
            [ParameterConfiguration("包类型", optionTemplate = "-packageType")] PackageType packageType = PackageType.Normal,
            [ParameterConfiguration("构建选项", optionTemplate = "-buildOption")] BuildOption buildOption = BuildOption.None,
            [ParameterConfiguration("输出目录", optionTemplate = "-outputDir")] string outputDir = "Output",
            [ParameterConfiguration("当前svn revision", optionTemplate = "-svnRev")] string svnRev = null
            )
        {

        }

        public static BuildAppResult BuildApk(
            string unityExePath,
            string nssUnityProj,
            BuildMode buildMode,
            bool prepareProject,
            string versionJsonPath = null,
            VerLine verLine = VerLine.DB,
            PackageType packageType = PackageType.Normal,
            BuildOption buildOption = BuildOption.None,
            string toolsDir = "../Tools",
            string outputDir = "Output",
            int svnRev = -1)
        {
            ModifyMacro(nssUnityProj, buildMode, packageType, buildOption);

            var result = new BuildAppResult();
            result.defaultTDir = ModifyDefaultLaunch(verLine, buildMode);

            var unityExe = new Executable(unityExePath);
            var unityArguments = $"-batchmode -quit -logFile \"G:\\temp\\log.log\" -projectPath \"{nssUnityProj}\" -executeMethod \"NssIntegration.BuildProcedure.Entry\" -l \"G:\\temp\\log2.log\"";
            if (prepareProject)
            {
                unityArguments += $" PrepareProject {buildMode} {BuildTarget.Android} \"{versionJsonPath}\" {packageType} {buildOption} \"{toolsDir}\"";
            }
            var sdkRoot = "D:\\android_sdk";
            var ndkRoot = "D:\\android-ndk-r13b";
            unityArguments += $" BuildApk {buildMode} \"{sdkRoot}\" \"{ndkRoot}\" {verLine} {packageType} {buildOption} \"{toolsDir}\" \"{outputDir}\" {svnRev}";
            unityExe.Execute(unityArguments);

            var apks = Directory.GetFiles(outputDir, "*.apk");
            if (apks.Length != 1)
            {
                throw new NssIntegrationException($"'{outputDir}'中有{apks.Length}个apk文件，不合理！");
            }
            result.appPath = apks[0];
            result.appVersion = ClientVersion.New(versionJsonPath).ToString();

            return result;
        }

        public static string PrepareUniqueVersionJson(BuildTarget buildTarget, string versionDir)
        {
            // 请求build version
            Logger.Log("从构建服务器请求build version...");
            var buildVer = IntegrationServer.RequestBuildVersion();
            Logger.Log($"Build version: {buildVer}");

            // 计算客户端版本号
            var version = ClientVersion.New(versionDir, buildTarget);
            version.BuildVersion = buildVer;
            Logger.Log($"客户端版本号为：{version}");

            // 保存
            version.Serialize();

            // 提交svn
            SvnUtil.Commit(version.path);

            return version.path;
        }

        public static void ModifyMacro(string nssUnityProj, BuildMode buildMode, PackageType packageType, BuildOption buildOption)
        {
            Logger.Log("开始修改客户端宏文件...");

            var mcsPath = Path.Combine(nssUnityProj, "Assets/mcs.rsp");
            if (!File.Exists(mcsPath)) throw new ArgumentException($"Can't find mcs.rsp in '{nssUnityProj}'!", nssUnityProj);

            MCS mcs = new MCS(mcsPath);
            mcs.ModifyItemByBuildMode(buildMode);

            if (packageType == PackageType.Experience)
            {
                mcs.TryAddItem("-define:NSS_EXPERIENCE");
            }
            if (buildOption == BuildOption.AI)
            {
                mcs.TryAddItem("-define:AVATAR_AI_ENABLE");
            }

            mcs.Serialize(mcsPath);

            Logger.Log("修改后的宏文件：");
            Logger.Log(File.ReadAllText(mcsPath));

            var sha1FilePath = SpaceUtil.GetPathInPersistent("MCSSha1");
            var newSha1 = FileUtil.ComputeSHA1(mcsPath);
            if (!File.Exists(sha1FilePath) || File.ReadAllText(sha1FilePath) != newSha1)
            {// 宏变了，要删除dll，强制重编
                Logger.Log("宏变了，删除旧的DLL...");
                DirUtil.ClearDir(Path.Combine(nssUnityProj, "Library/ScriptAssemblies"));
            }
            File.WriteAllText(sha1FilePath, newSha1);
        }

        public static void AssemblyApk(string inApkPath, string outApkPath, string ifsPath)
        {
            Logger.Log($"开始组装APK...\napkPath：{inApkPath}，ifsPath：{ifsPath}");

            ApkTool.PutPathInAPK(inApkPath, outApkPath, "assets/AssetBundles.png", ifsPath);

            Logger.Log($"APK组装完成");
        }

        public static string ModifyDefaultLaunch(VerLine verLine, BuildMode buildMode)
        {
            var path = "Assets/StreamingAssets/DefaultLaunchConfig.txt";
            var defaultLaunchConfig = new DefaultLaunchConfig(path);

            defaultLaunchConfig["DefaultVerLine"] = verLine.ToString();
            defaultLaunchConfig["MiniPackage"] = "false";

            if (buildMode == BuildMode.Publish)
            {
                defaultLaunchConfig["DefaultTDir"] = "Official";
                defaultLaunchConfig["DefaultTVersion"] = "Official";
                defaultLaunchConfig["DefaultTVersionLog"] = "false";
                defaultLaunchConfig["DefaultTVEnv"] = "Pub";
            }
            else if (buildMode == BuildMode.Debug)
            {
                defaultLaunchConfig["DefaultTDir"] = "Test";
                defaultLaunchConfig["DefaultTVersion"] = "Test";
                defaultLaunchConfig["DefaultTVersionLog"] = "false";
                defaultLaunchConfig["DefaultTVEnv"] = "Pre";
            }
            else
            {
                throw new Exception(string.Format("BuildMode '{0}' 是不支持的类型！", buildMode));
            }

            if (verLine == VerLine.PlayerGroup)
            {
                defaultLaunchConfig["DefaultTDir"] = "Test";
                defaultLaunchConfig["DefaultTVersion"] = "Test";
                defaultLaunchConfig["DefaultTVersionLog"] = "true";
                defaultLaunchConfig["DefaultTVEnv"] = "Pre";
            }
            else if (verLine == VerLine.Experience)
            {
                defaultLaunchConfig["DefaultTDir"] = "Test";
                defaultLaunchConfig["DefaultTVersion"] = "Official";
                defaultLaunchConfig["DefaultTVersionLog"] = "false";
                defaultLaunchConfig["DefaultTVEnv"] = "Pub";
            }
            else if (verLine == VerLine.OB)
            {
                defaultLaunchConfig["DefaultTDir"] = "Test";
                defaultLaunchConfig["DefaultTVersion"] = "Official";
                defaultLaunchConfig["DefaultTVersionLog"] = "false";
                defaultLaunchConfig["DefaultTVEnv"] = "Pre";
            }
            else if (verLine == VerLine.Predownload)
            {
                defaultLaunchConfig["DefaultTDir"] = "Test";
                defaultLaunchConfig["DefaultTVersion"] = "TestB";
                defaultLaunchConfig["DefaultTVersionLog"] = "true";
                defaultLaunchConfig["DefaultTVEnv"] = "Pre";
            }
            else if (verLine == VerLine.DB) { }
            else
            {
                throw new Exception(string.Format("VerLine '{0}' 是不支持的类型！", verLine));
            }

            defaultLaunchConfig.Serialize(path);
            return defaultLaunchConfig["DefaultTDir"];
        }
    }
}
