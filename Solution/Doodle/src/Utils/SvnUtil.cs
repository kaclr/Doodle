using System;
using System.IO;

namespace Doodle
{
    public static class SvnUtil
    {
        private static string s_svnExePath;

        public static void Init(string svnExePath)
        {
            s_svnExePath = svnExePath;
        }

        public static void Sync(string localPath, string svnUrl = null, bool removeIgnore = true)
        {
            Check();

            if (File.Exists(localPath)) throw new ArgumentException($"'{nameof(localPath)}' can't be a file!", nameof(localPath));

            if (!Directory.Exists(localPath))
            {

            }
        }

        public static void Checkout(string svnUrl, string localPath)
        {
            Check();

            if (PathUtil.Exists(localPath)) throw new ArgumentException($"'{nameof(localPath)}' is already exists!", nameof(localPath));
            if (!IsSvnUrl(svnUrl)) throw new ArgumentException($"'{nameof(svnUrl)}' is not a svn url!", nameof(svnUrl));

            CmdUtil.ExecuteCmd($"\"{s_svnExePath}\" co \"{svnUrl}\" \"{localPath}\"");
        }

        public static bool IsSvnUrl(string svnUrl)
        {
            return svnUrl.StartsWith("http://");
        }

        private static void Check()
        {
            if (s_svnExePath == null)
            {
                throw new DoodleException($"'{nameof(SvnUtil)}' has not be Init!");
            }
        }
    }
}
