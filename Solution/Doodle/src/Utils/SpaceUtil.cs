using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Doodle
{
    public static class SpaceUtil
    {
        private static string s_tempSpaceRoot;
        private static string s_persistentSpaceRoot;
        private static StringBuilder s_longPathBuffer;
        private static bool s_inited;

        public static void SetTempSpace(string path)
        {
            Init();

            if (string.IsNullOrEmpty(path)) throw new ArgumentException($"'{nameof(path)}' is empty!");
            if (File.Exists(path)) throw new ArgumentException($"Temp space can not be a file!", nameof(path));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            s_tempSpaceRoot = path;
        }

        public static void SetPersistentSpace(string path)
        {
            Init();

            if (string.IsNullOrEmpty(path)) throw new ArgumentException($"'{nameof(path)}' is empty!");
            if (File.Exists(path)) throw new ArgumentException($"Temp space can not be a file!", nameof(path));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            s_persistentSpaceRoot = path;
        }

        public static string GetPathInPersistent(string path = "", bool absolute = false)
        {
            Init();

            if (s_persistentSpaceRoot == null)
                throw new DoodleException($"Persistent space hasn't been set, you must set it by 'SetPersistentSpace' first!");

            path = Path.Combine(s_persistentSpaceRoot, path);
            if (absolute)
            {
                path = Path.GetFullPath(path);
            }
            return path;
        }

        public static string GetPathInTemp(string path = "", bool absolute = false)
        {
            Init();

            path = string.IsNullOrEmpty(path) ? s_tempSpaceRoot : Path.Combine(s_tempSpaceRoot, path);

            if (absolute)
            {
                path = Path.GetFullPath(path);
            }
            return path;
        }

        public static string GetPathInBase(string path = "")
        {
            Init();

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        public static string NewTempPath()
        {
            Init();

            Logger.VerboseLog($"[SpaceUtil->NewTempPath] s_tempSpaceRoot: {s_tempSpaceRoot}");

            var result = Path.Combine(s_tempSpaceRoot, Guid.NewGuid().ToString().Replace("-", ""));

            Logger.VerboseLog($"[SpaceUtil->NewTempPath] result: {result}");
            return result;
        }

        public static string NewTempDir()
        {
            Init();

            var path = NewTempPath();
            Directory.CreateDirectory(path);
            return path;
        }

        private static void Init()
        {
            if (s_inited) return;
            s_inited = true;

            s_tempSpaceRoot = Path.GetTempPath();
            Logger.VerboseLog($"[SpaceUtil->Init] raw s_tempSpaceRoot: {s_tempSpaceRoot}");
            if (DoodleEnv.curPlatform == Platform.Windows)
            {
                //s_longPathBuffer = new StringBuilder(256);
                //GetLongPathName(s_tempSpaceRoot, s_longPathBuffer, s_longPathBuffer.Capacity);
                //s_tempSpaceRoot = s_longPathBuffer.ToString();
                s_tempSpaceRoot = Path.GetFullPath(s_tempSpaceRoot);
                Logger.VerboseLog($"[SpaceUtil->Init] long s_tempSpaceRoot: {s_tempSpaceRoot}");
            }
        }

        //[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        //private static extern int GetLongPathName(
        //[MarshalAs(UnmanagedType.LPTStr)]
        //string path,
        //[MarshalAs(UnmanagedType.LPTStr)]
        //StringBuilder longPath,
        //int longPathLength
        //);
    }
}
