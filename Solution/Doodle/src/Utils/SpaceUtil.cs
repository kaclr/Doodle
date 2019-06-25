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
            if (s_tempSpaceRoot == null)
                throw new DoodleException($"Don't have temp space, you must set it by 'SetTempSpace'!");

            return Path.Combine(s_tempSpaceRoot, path);
        }

        public static string NewTempDir()
        {
            if (s_tempSpaceRoot == null)
                throw new DoodleException($"Don't have temp space, you must set it by 'SetTempSpace'!");

            var dir = Path.Combine(s_tempSpaceRoot, Guid.NewGuid().ToString().Replace("-", ""));
            Directory.CreateDirectory(Path.Combine(s_tempSpaceRoot, dir));
            return dir;
        }
    }
}
