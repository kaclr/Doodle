using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Doodle
{
    public static class ApkTool
    {
        private static Func<string> s_onGetApkToolDir;
        [ThreadStatic]
        private static Script s_apkToolScript;

        public static void Init(Func<string> onGetApkToolDir)
        {
            s_onGetApkToolDir = onGetApkToolDir;
        }

        /// <summary>
        /// 把pathToPut的路径放入到Apk中的pathInApk路径，如果已经存在则覆盖。
        /// </summary>
        /// <param name="inApkPath">输入的apk</param>
        /// <param name="outApkPath">输出的apk</param>
        /// <param name="pathInApk">apk中的路径</param>
        /// <param name="pathToPut">需要被放置的路径</param>
        public static void PutPathInAPK(string inApkPath, string outApkPath, string pathInApk, string pathToPut)
        {
            InitInner();

            var apkDir = SpaceUtil.NewTempPath();
            Logger.Log($"Unpack apk to '{apkDir}'...");
            UnpackAPK(inApkPath, apkDir);

            // 放
            Logger.Log($"Copy '{pathToPut}' to '{Path.Combine(apkDir, pathInApk)}'...");
            PathUtil.CopyPath(pathToPut, Path.Combine(apkDir, pathInApk), true);

            Logger.Log($"Pack apk to '{outApkPath}'...");
            PackAPK(apkDir, outApkPath);
        }

        public static void PackAPK(string apkDir, string outputApk)
        {
            InitInner();

            if (PathUtil.GetPathState(apkDir) != PathState.Dir)
            {
                throw new NotDirException(apkDir, nameof(apkDir));
            }
            if (PathUtil.GetPathState(outputApk) != PathState.None)
            {
                throw new NotEmptyPathException(outputApk, nameof(outputApk));
            }

            s_apkToolScript.Execute($"empty-framework-dir");
            s_apkToolScript.Execute($"b -o \"{outputApk}\" \"{apkDir}\"");
        }

        public static void UnpackAPK(string apkPath, string outputDir)
        {
            InitInner();

            if (!File.Exists(apkPath)) throw new DoodleException($"'{nameof(apkPath)}' with value '{apkPath}' is invalid, can't find any file!");

            if (PathUtil.GetPathState(outputDir) != PathState.None)
            {
                throw new DoodleException(string.Format("outputDir '{0}' already exists, please use a empty path!", outputDir));
            }

            s_apkToolScript.Execute($"d -o \"{outputDir}\" \"{apkPath}\"");
        }

        private static void InitInner()
        {
            if (s_apkToolScript != null) return;
            if (s_onGetApkToolDir == null) throw new DoodleException($"{nameof(ApkTool)} hasn't been Inited!");

            s_apkToolScript = new Script(Path.Combine(s_onGetApkToolDir(), "apktool"));
        }
    }
}
