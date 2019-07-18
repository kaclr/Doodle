using System;
using System.Collections.Generic;
using System.Text;

namespace NssIntegration
{
    using Doodle;
    using Doodle.CommandLineUtils;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    public class BuildAppResult
    {
        public string appPath;
        public string appVersion;
        public string defaultTDir;
    }

    static class BuildProcedure
    {


        public static BuildAppResult BuildApk(
            string unityExePath,
            string nssUnityProj,
            BuildMode buildMode,
            bool prepareProject,
            string sdkRoot,
            string ndkRoot,
            string jdkRoot,
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
            result.defaultTDir = ModifyDefaultLaunch(nssUnityProj, verLine, buildMode);

            var unityExe = new Executable(unityExePath);
            var unityArguments = $"-batchmode -quit -logFile \"{SpaceUtil.GetPathInTemp("log.log", true)}\" -projectPath \"{nssUnityProj}\" -executeMethod \"NssIntegration.BuildProcedure.Entry\" -l \"{SpaceUtil.GetPathInTemp("log2.log", true)}\"";
            if (prepareProject)
            {
                unityArguments += $" PrepareProject {buildMode} {BuildTarget.Android} \"{versionJsonPath}\" {packageType} {buildOption} \"{toolsDir}\"";
            }
            unityArguments += $" BuildApk {buildMode} \"{sdkRoot}\" \"{ndkRoot}\" \"{jdkRoot}\" {verLine} {packageType} {buildOption} \"{toolsDir}\" \"{outputDir}\" {svnRev}";
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

        public static string ModifyDefaultLaunch(string nssUnityProj, VerLine verLine, BuildMode buildMode)
        {
            var path = $"{nssUnityProj}/Assets/StreamingAssets/DefaultLaunchConfig.txt";
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

        public static void AcquireUnity(string unityPoolDir, string unitySvnUrl, string unitySvnRev, out string unityExePath, out IFLock unityLock)
        {
            if (DoodleEnv.curPlatform != Platform.OSX) throw new NssIntegrationException($"'{nameof(AcquireUnity)}'只能在OS X系统使用！");
            if (!Directory.Exists(unityPoolDir)) throw new NotDirException(unityPoolDir, nameof(unityPoolDir));

            unityExePath = null;
            unityLock = null;

            // 遍历目录下所有Unity，返回可以得到写锁的那个
            string unityAppPath = null;
            var rootDir = new DirectoryInfo(unityPoolDir);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            IFLock flock = null;
            RepeateDo repeateDo = new RepeateDo(600);
            repeateDo.Do(() =>
            {
                foreach (var dir in rootDir.GetDirectories("*.app"))
                {
                    flock = FLockUtil.NewFLock(Path.Combine(unityPoolDir, dir.Name, "lock"));
                    if (!flock.TryAcquireExclusiveLock())
                    {
                        continue;
                    }

                    unityAppPath = Path.Combine(unityPoolDir, dir.Name, "Contents/MacOS/Unity");
                    break;
                }

                if (unityAppPath == null)
                {
                    Logger.Log($"没有可用的Unity引擎，已等待{stopwatch.ElapsedMilliseconds / 1000 / 60}分钟...");
                    Thread.Sleep(60 * 1000);
                    return false;
                }
                
                return true;
            });

            repeateDo = new RepeateDo(5);
            repeateDo.IgnoreException<ExecutableException>();
            repeateDo.Do(() => SvnUtil.Sync(unityAppPath, unitySvnUrl, unitySvnRev));

            if (!flock.TryAcquireShareLock())
            {
                throw new ImpossibleException($"之前已经获取了写锁了，获取读锁不可能失败！");
            }

            // 赋执行权限
            CmdUtil.Execute($"chmod -R +x {unityAppPath}");

            unityExePath = Path.Combine(unityAppPath, "Contents/MacOS/Unity");
            unityLock = flock;
            return;
        }
    }
}
