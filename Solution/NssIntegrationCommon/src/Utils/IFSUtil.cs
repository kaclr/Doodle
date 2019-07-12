using System;
using System.Collections.Generic;
using System.Text;

namespace NssIntegration
{
    using Doodle;
    using System.IO;

    public static class IFSUtil
    {
        private static Func<string> s_onGetIIPSDir;

        [ThreadStatic]
        private static bool s_inited;
        [ThreadStatic]
        private static Executable s_packager;
        [ThreadStatic]
        private static Executable s_nifs;

        public static void Init(Func<string> onGetIIPSDir)
        {
            s_onGetIIPSDir = onGetIIPSDir;
        }

        public static void UnpackIFS(string ifsPath, string outDir)
        {
            InitInner();

            if (!File.Exists(ifsPath)) throw new ArgumentException($"'{outDir}' is not a file!", nameof(ifsPath));
            if (File.Exists(outDir)) throw new ArgumentException($"'{outDir}' can't be a file!", nameof(outDir));

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            Logger.VerboseLog($"Unpacking ifs '{ifsPath}' to directory '{outDir}'...");
            if (DoodleEnv.curPlatform == Platform.Windows)
            {
                s_packager.Execute($"get \"{ifsPath}\" * \"{outDir}\"");
            }
            else
            {
                throw new NotImplementedException();
            }

            // 删除(listfile)
            PathUtil.RemovePath(Path.Combine(outDir, "(listfile)"));
        }

        public static void PackIFS(string inputDir, string outIFSPath)
        {
            InitInner();

            if (!Directory.Exists(inputDir)) throw new ArgumentException($"'{inputDir}' is not a directory!", nameof(inputDir));

            if (PathUtil.Exists(outIFSPath))
            {
                PathUtil.RemovePath(outIFSPath);
            }

            Logger.VerboseLog($"Packing ifs '{outIFSPath}' from directory '{inputDir}'...");
            s_nifs.Execute($"create \"{inputDir}\" \"{outIFSPath}\"");
        }

        private static void InitInner()
        {
            if (s_inited) return;
            if (s_onGetIIPSDir == null) throw new NssIntegrationException($"{nameof(IFSUtil)} hasn't been Inited!");

            s_inited = true;

            var iipsDir = s_onGetIIPSDir();
            if (DoodleEnv.curPlatform == Platform.Windows)
            {
                if (!File.Exists(Path.Combine(iipsDir, "Packager.exe")))
                {
                    throw new NssIntegrationException($"Can't find 'Packager.exe' in directory '{iipsDir}'!");
                }
                if (!File.Exists(Path.Combine(iipsDir, "WinPackager.exe")))
                {
                    throw new NssIntegrationException($"Can't find 'WinPackager.exe' in directory '{iipsDir}'!");
                }

                s_packager = new Executable(Path.Combine(iipsDir, "Packager.exe"));
                s_nifs = new Executable(Path.Combine(iipsDir, "WinPackager.exe"));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
