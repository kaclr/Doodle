using System;
using System.Collections.Generic;
using System.Text;

namespace NssIntegration
{
    using Doodle;
    using System.IO;

    static class BuildProcedure
    {
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
