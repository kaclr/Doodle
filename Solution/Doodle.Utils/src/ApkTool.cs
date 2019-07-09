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
