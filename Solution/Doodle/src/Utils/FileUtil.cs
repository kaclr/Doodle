using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Doodle
{
    public static class FileUtil
    {
        public static bool Exists(string path)
        {
            return PathUtil.GetPathState(path) == PathState.File;
        }

        public static bool TryCreateFile(string path, string content)
        {
            if (Exists(path))
            {
                return false;
            }

            var sw = File.CreateText(path);
            sw.Write(content);
            sw.Close();
            return true;
        }
    }
}
