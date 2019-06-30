using System;
using System.IO;

namespace Doodle
{
    public static class SvnUtil
    {
        private static Executable s_svn;

        public static void Init(string svnExePath)
        {
            s_svn = new Executable(svnExePath);
        }

        public static void Sync(string localPath, string svnUrl = null, bool removeIgnore = true)
        {
            Check();

            if (File.Exists(localPath)) throw new ArgumentException($"'{nameof(localPath)}' can't be a file!", nameof(localPath));

            if (!Directory.Exists(localPath))
            {// 直接checkout就完事了
                if (string.IsNullOrEmpty(svnUrl)) throw new ArgumentException($"'{nameof(svnUrl)}' is empty when svn checkout!", nameof(svnUrl));

                Checkout(svnUrl, localPath);
                return;
            }

            if (string.IsNullOrEmpty(svnUrl))
            {// 从本地目录获取svnUrl

            }
        }

        public static void Checkout(string svnUrl, string localPath)
        {
            Check();

            if (PathUtil.Exists(localPath)) throw new ArgumentException($"'{nameof(localPath)}' is already exists!", nameof(localPath));
            if (!IsSvnUrl(svnUrl)) throw new ArgumentException($"'{nameof(svnUrl)}' is not a svn url!", nameof(svnUrl));

            s_svn.Execute($"co \"{svnUrl}\" \"{localPath}\"");
        }

        public static bool IsSvnUrl(string svnUrl)
        {
            return svnUrl.StartsWith("http://");
        }

        private static void Check()
        {
            if (s_svn == null)
            {
                throw new DoodleException($"'{nameof(SvnUtil)}' has not be Init!");
            }
        }
    }
}
