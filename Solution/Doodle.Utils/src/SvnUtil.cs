using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Doodle
{
    public class SvnInfo
    {
        public string url { get; set; }
        public int lastChangedRev { get; set; }
    }

    public static class SvnUtil
    {
        private static Func<string> s_onGetSvnExe;
        private static Executable s_svn;

        public static void Init(Func<string> onGetSvnExe)
        {
            if (onGetSvnExe == null) throw new ArgumentNullException(nameof(onGetSvnExe));
            s_onGetSvnExe = onGetSvnExe;
        }

        public static void Init(string svnExe)
        {
            if (s_svn != null) return;

            if (string.IsNullOrEmpty(svnExe)) throw new ArgumentException($"{nameof(svnExe)} is empty!", nameof(svnExe));

            s_svn = new Executable(svnExe);

            var version = GetSvnVersionInner();
            if (int.Parse(version.Split('.')[1]) <= 8)
            {
                throw new DoodleException($"SVN version must be greater than or equal to 1.9.x, input version is {version}");
            }
        }

        public static void Sync(string localPath, string svnUrl = null, bool removeIgnore = true)
        {
            InitInner();

            if (File.Exists(localPath)) throw new ArgumentException($"'{nameof(localPath)}' can't be a file!", nameof(localPath));

            if (!Directory.Exists(localPath))
            {// 直接checkout就完事了
                if (string.IsNullOrEmpty(svnUrl)) throw new ArgumentException($"'{nameof(svnUrl)}' is empty when svn checkout!", nameof(svnUrl));

                Checkout(svnUrl, localPath);
                return;
            }

            if (string.IsNullOrEmpty(svnUrl))
            {// 从本地目录获取svnUrl
                svnUrl = GetSvnInfo(localPath).url;
                Logger.VerboseLog($"Get svn url from localPath: {svnUrl}");
            }

            Logger.VerboseLog($"svn sync '{localPath}' to '{svnUrl}'\nremoveIgnore:{removeIgnore}");

            // 1. cleanup
            var arguments = "cleanup --remove-unversioned";
            if (removeIgnore)
            {
                arguments += " --remove-ignored";
            }
            arguments += $" \"{localPath}\"";
            Logger.VerboseLog($"1. svn {arguments}");
            DoWithCleanup(arguments, localPath);

            // 2. revert
            arguments = $"revert -R \"{localPath}\"";
            Logger.VerboseLog($"2. svn {arguments}");
            DoWithCleanup(arguments, localPath);

            // 3. switch
            arguments = $"switch --force \"{svnUrl}\" \"{localPath}\"";
            Logger.VerboseLog($"3. svn {arguments}");
            DoWithCleanup(arguments, localPath);

            // 4. 检查状态
            string st = removeIgnore ? s_svn.Execute($"st --no-ignore \"{localPath}\"") : s_svn.Execute($"st \"{localPath}\"");
            if (!string.IsNullOrEmpty(st))
            {
                var lines = st.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (!line.StartsWith("X", StringComparison.Ordinal) &&
                        !line.StartsWith("Performing status on external item at", StringComparison.Ordinal))
                    {
                        throw new DoodleException($"Svn sync failed, svn status is wrong:\n{line}");
                    }
                }
            }
        }

        public static void Checkout(string svnUrl, string localPath)
        {
            InitInner();

            if (PathUtil.Exists(localPath)) throw new ArgumentException($"'{localPath}' is already exists!", nameof(localPath));
            if (!IsSvnUrl(svnUrl)) throw new ArgumentException($"'{nameof(svnUrl)}' is not a svn url!", nameof(svnUrl));

            var arguments = $"co \"{svnUrl}\" \"{localPath}\"";
            Logger.VerboseLog($"svn {arguments}");
            s_svn.Execute(arguments);
        }

        public static SvnInfo GetSvnInfo(string pathOrUrl)
        {
            InitInner();

            if (string.IsNullOrEmpty(pathOrUrl)) throw new ArgumentException($"'{pathOrUrl}' is null or empty!");

            var infoStr = s_svn.Execute($"info \"{pathOrUrl}\"");

            return new SvnInfo()
            {
                url = Regex.Match(infoStr, "^URL: (.*)$", RegexOptions.Multiline).Groups[1].Value.Trim(),
                lastChangedRev = int.Parse(Regex.Match(infoStr, "^Last Changed Rev: (.*)$", RegexOptions.Multiline).Groups[1].Value),
            };
        }

        public static bool IsSvnUrl(string svnUrl)
        {
            return svnUrl.StartsWith("http://");
        }

        public static string GetSvnVersion()
        {
            InitInner();

            return GetSvnVersionInner();
        }

        public static string GetSvnVersionInner()
        {
            var str = s_svn.Execute($"--version --quiet");
            var m = Regex.Match(str, "(\\d+\\.\\d+\\.\\d+)");
            if (!m.Success)
            {
                throw new DoodleException($"Get svn version failed, contents:\n{str}");
            }

            return m.Groups[1].Value;
        }

        private static string DoWithCleanup(string arguments, string localPath)
        {
            bool hasCleanup = false;
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    return s_svn.Execute(arguments);
                }
                catch (Exception e)
                {
                    if (!hasCleanup)
                    {// 试着cleanup一下
                        Logger.VerboseLog("Failed, try cleanup...");
                        s_svn.Execute($"cleanup \"{localPath}\"");

                        hasCleanup = true;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            throw new DoodleException("Can not be here!");
        }

        private static void InitInner()
        {
            if (s_onGetSvnExe == null) throw new DoodleException($"{nameof(SvnUtil)} hasn't been Inited!");
            Init(s_onGetSvnExe());
        }
    }
}
