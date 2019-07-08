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

        static SpaceUtil()
        {
            s_tempSpaceRoot = Path.GetTempPath();
        }

        public static void SetTempSpace(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(path);
            if (File.Exists(path)) throw new ArgumentException($"Temp space can not be a file!", nameof(path));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            s_tempSpaceRoot = path;
        }

        public static string GetTempPath(string path = "")
        {
            return Path.Combine(s_tempSpaceRoot, path);
        }

        public static string GetBasePath(string path = "")
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        public static string NewTempDir()
        {
            var dir = Path.Combine(s_tempSpaceRoot, Guid.NewGuid().ToString().Replace("-", ""));
            Directory.CreateDirectory(Path.Combine(s_tempSpaceRoot, dir));
            return dir;
        }
    }
}
