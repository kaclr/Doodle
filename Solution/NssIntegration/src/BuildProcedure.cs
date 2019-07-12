using System;
using System.Collections.Generic;
using System.Text;

namespace NssIntegration
{
    using Doodle;
    using Doodle.CommandLineUtils;
    using System.IO;

    static class BuildProcedure
    {
        public static void BuildApk(
            [ParameterConfiguration("Unity可执行程序的路径")] string unityExePath,
            [ParameterConfiguration("Nss客户端工程根目录")] string nssUnityProj,
            [ParameterConfiguration("构建模式")] BuildMode buildMode,
            [ParameterConfiguration("是否执行工程准备流程", optionTemplate = "-prepareProject")] bool prepareProject,
            [ParameterConfiguration("Version文件路径", optionTemplate = "-versionJsonPath")] string versionJsonPath = null,
            [ParameterConfiguration("版本线", optionTemplate = "-verLine")] VerLine verLine = VerLine.DB,
            [ParameterConfiguration("包类型", optionTemplate = "-packageType")] PackageType packageType = PackageType.Normal,
            [ParameterConfiguration("构建选项", optionTemplate = "-buildOption")] BuildOption buildOption = BuildOption.None,
            [ParameterConfiguration("客户端工程内的Tools目录", optionTemplate = "-toolsDir")] string toolsDir = "../Tools",
            [ParameterConfiguration("输出目录", optionTemplate = "-outputDir")] string outputDir = "Output",
            [ParameterConfiguration("当前svn revision", optionTemplate = "-svnRev")] string svnRev = null)
        {
            ModifyMacro(nssUnityProj, buildMode, packageType, buildOption);

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
        }

        public static void PrepareVersion(BuildTarget buildTarget, string svnUrl)
        {
            Logger.TryOffConsoleOutput();

            // 请求build version
            Logger.Log("从构建服务器请求build version...");
            var buildVer = IntegrationServer.RequestBuildVersion();
            Logger.Log($"Build version: {buildVer}");

            // 计算客户端版本号
            var versionDir = SpaceUtil.NewTempPath();
            SvnUtil.Checkout($"{svnUrl}/NssUnityProj/Version", versionDir);
            var version = ClientVersion.New(versionDir, buildTarget);
            version.BuildVersion = buildVer;
            Logger.Log($"客户端版本号为：{version}");

            version.Serialize();

            // 输出version文件路径
            Console.WriteLine(version.path);
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
    }
}
