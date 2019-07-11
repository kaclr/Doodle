using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Doodle
{
    public static class SpaceUtil
    {
        private static string s_tempSpaceRoot;
        private static string s_persistentSpaceRoot;

        static SpaceUtil()
        {
            s_tempSpaceRoot = Path.GetTempPath();
        }

        public static void SetTempSpace(string path)
        {
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
            if (string.IsNullOrEmpty(path)) throw new ArgumentException($"'{nameof(path)}' is empty!");
            if (File.Exists(path)) throw new ArgumentException($"Temp space can not be a file!", nameof(path));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            s_persistentSpaceRoot = path;
        }

        public static string GetPathInPersistent(string path = "")
        {
            if (s_persistentSpaceRoot == null)
                throw new DoodleException($"Persistent space hasn't been set, you must set it by 'SetPersistentSpace' first!");

            return Path.Combine(s_persistentSpaceRoot, path);
        }

        public static string GetPathInTemp(string path = "")
        {
            return Path.Combine(s_tempSpaceRoot, path);
        }

        public static string GetPathInBase(string path = "")
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        public static string NewTempPath()
        {
            return Path.Combine(s_tempSpaceRoot, Guid.NewGuid().ToString().Replace("-", ""));
        }

        public static string NewTempDir()
        {
            var path = NewTempPath();
            Directory.CreateDirectory(NewTempPath());
            return path;
        }
    }
}
