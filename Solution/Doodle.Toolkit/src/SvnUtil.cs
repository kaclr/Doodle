using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Doodle
{
    public class SvnInfo
    {
        public string url { get; set; }
        public int lastChangedRev { get; set; }
    }   

    /// <summary>
    /// 冲突接受方法枚举
    /// 下划线是为了直接转字符串参数
    /// </summary>
    public enum SvnConflictAccept
    {
        //不指定
        None,

        //使用前一版本作为最后提交的版本 
        BASE,

        //使用当前包含冲突内容的文件作为最后提交的版本 
        WORKING,

        //使用本地文件作为最后提交的版本 
        MINE_FULL,

        //使用本地修改内容作为最后提交的版本
        MINE_CONFLICT,

        //使用远端文件作为最后提交的版本
        THEIRS_FULL,

        //使用远端修改作为最后提交的版本
        THEIRS_CONFLICT,
    }

    public static class SvnUtil
    {

        public static Action<string> OutputDataReceived
        {
            get
            {
                return s_outputDataReceived;
            }
            set
            {
                //确保初始化前和初始化后赋值都能发生作用
                s_outputDataReceived = value;
                if (s_svn != null)
                {
                    s_svn.OutputDataReceived = value;
                }
            }
        }

        private static Action<string> s_outputDataReceived;

        private static Func<string> s_onGetSvnExe;
        private static Executable s_svn;

        //用户名
        private static string s_username = string.Empty;

        //密码
        private static string s_password = string.Empty;


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
            s_svn.OutputDataReceived = s_outputDataReceived;

            var version = GetSvnVersionInner();
            if (int.Parse(version.Split('.')[1]) <= 8)
            {
                throw new DoodleException($"SVN version must be greater than or equal to 1.9.x, input version is {version}");
            }
        }

        public static void SetUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException($"{nameof(username)} is empty!");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException($"{nameof(username)} is empty!");
            }

            s_username = username;
            s_password = password;     
        }

        public static void Sync(string localPath, string svnUrl = null, string svnRev = "head", bool removeIgnore = true)
        {
            InitInner();

            if (File.Exists(localPath)) throw new ArgumentException($"'{nameof(localPath)}' can't be a file!", nameof(localPath));

            if (!Directory.Exists(localPath))
            {// 直接checkout就完事了
                if (string.IsNullOrEmpty(svnUrl)) throw new ArgumentException($"'{nameof(svnUrl)}' is empty when svn checkout!", nameof(svnUrl));

                Checkout(svnUrl, localPath, svnRev);
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
            arguments = $"switch --force -r \"{svnRev}\" \"{svnUrl}\" \"{localPath}\"";
            Logger.VerboseLog($"3. svn {arguments}");
            DoWithCleanup(arguments, localPath);

            // 4. 检查状态
            string st = removeIgnore ? SvnExecuteOut($"st --no-ignore \"{localPath}\"") : SvnExecuteOut($"st \"{localPath}\"");
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

        public static void Checkout(string svnUrl, string localPath, string svnRev = "head")
        {
            InitInner();

            if (PathUtil.Exists(localPath)) throw new ArgumentException($"'{localPath}' is already exists!", nameof(localPath));
            if (!IsSvnUrl(svnUrl)) throw new ArgumentException($"'{nameof(svnUrl)}' is not a svn url!", nameof(svnUrl));

            var arguments = $"co -r \"{svnRev}\" \"{svnUrl}\" \"{localPath}\"";
            Logger.VerboseLog($"svn {arguments}");
            SvnExecuteOut(arguments);
        }

        /// <summary>
        /// SVN提交修改，包括增删的文件
        /// </summary>
        /// <param name="localPath">本地目录</param>
        public static void CommitModification(string localPath, string comment)
        {
            InitInner();

            bool addOrRemove = false;
            do
            {
                addOrRemove = false;
                string st = SvnExecuteOut($"st \"{localPath}\"");

                if (!string.IsNullOrEmpty(st))
                {
                    var lines = st.Split('\n');
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrEmpty(line))
                            continue;
                        //状态显示有新增的文件
                        if (CheckFileStatusFromStatusLine(line,"\\?"))
                        {
                            string fileName = GetFileNameFromStatusLine(line);
                            addOrRemove = true;
                            //svn增加文件
                            SvnExecuteOut($"add \"{fileName}\"");
                        }
                        //状态显示有删除的文件
                        else if (CheckFileStatusFromStatusLine(line, "\\!"))
                        {
                            string fileName = GetFileNameFromStatusLine(line);
                            //svn删除文件
                            SvnExecuteOut($"rm \"{fileName}\"");
                            addOrRemove = true;
                        }
                    }
                }
                //可能因为新增目录，所以重复搜索深层目录
            } while (addOrRemove);
            //最后提交
            Commit(localPath, comment);
        }
        /// <summary>
        /// SVN获取当前目录修改或者增加的文件
        /// </summary>
        /// <param name="localPath">本地目录</param>
        public static bool GetAddOrModify(string localPath,out List<string> filePaths)
        {
            InitInner();
            filePaths = new List<string>();

            bool addOrModyfy = false;    
            string st = SvnExecuteOut($"st \"{localPath}\"");

            if (!string.IsNullOrEmpty(st))
            {
                var lines = st.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;
                    //状态显示有新增的文件
                    if (CheckFileStatusFromStatusLine(line, "\\?"))
                    {
                        string fileName = GetFileNameFromStatusLine(line);
                        addOrModyfy = true;
                        filePaths.Add(fileName);
                    }
                    //状态显示有修改的文件
                    else if (CheckFileStatusFromStatusLine(line, "M"))
                    {
                        string fileName = GetFileNameFromStatusLine(line);
                        filePaths.Add(fileName);
                        addOrModyfy = true;
                    }
                }
            }
            return addOrModyfy;
        }

        /// <summary>
        /// SVN更新，无交互防止卡住
        /// </summary>
        /// <param name="localPath">本地目录</param>
        public static void Update(string localPath)
        {
            InitInner();
            SvnExecuteOut($"update \"{localPath}\"");
        }

        /// <summary>
        /// SVN检查是否存在冲突
        /// </summary>
        /// <param name="localPath">本地目录</param>
        /// <param name="conflictFiles">冲突文件列表</param>
        public static bool ExistConflict(string localPath, out List<string> conflictFiles)
        {
            InitInner();
            conflictFiles = new List<string>();
            //获取状态
            string st = SvnExecuteOut($"st \"{localPath}\"");
            if (!string.IsNullOrEmpty(st))
            {
                var lines = st.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;
                    //状态显示有冲突的文件
                    //C xxx.txt这种形式或者 A + C xxx.txt
                    if (CheckFileStatusFromStatusLine(line,"C"))
                    {
                        string fileName = GetFileNameFromStatusLine(line);
                        conflictFiles.Add(fileName);
                    }                      
                }
            }
            return conflictFiles.Count > 0;  
        }

        /// <summary>
        /// SVN直接解决文件冲突
        /// </summary>
        /// <param name="localFilePath">本地文件路径</param>
        /// <param name="conflictAccept">冲突接受方法枚举</param>
        public static void SolveConfilct(string localFilePath, SvnConflictAccept conflictAccept)
        {
            InitInner();
            SvnExecuteOut($"resolve {GetAcceptArg(conflictAccept)} \"{localFilePath}\"");
        }

        /// <summary>
        /// SVN解决目录下所有文件冲突，使用Theirs或者Mine
        /// </summary>
        /// <param name="localPath">本地目录</param>
        /// <param name="conflictAccept">冲突接受方法枚举</param>
        public static void SolveAllConfilct(string localPath, SvnConflictAccept conflictAccept)
        {
            InitInner();
            var list = new List<string>();
            if (ExistConflict(localPath, out list))
            {
                foreach (var filePath in list)
                {
                    SolveConfilct(filePath, conflictAccept);
                }
            }
        }

        /// <summary>
        /// 添加文件到版本控制下，已受控制的文件会抛异常
        /// </summary>
        /// <param name="localPath">本地目录</param>
        public static void Add(string localPath)
        {
            InitInner();
            SvnExecuteOut($"add \"{localPath}\"");
        }

        /// <summary>
        /// 添加文件到版本控制下，会无视已受控制的文件
        /// </summary>
        /// <param name="localPath">本地目录</param>
        public static void ForceAdd(string localPath)
        {
            InitInner();
            SvnExecuteOut($"add \"{localPath}\" --force");
        }

        /// <summary>
        /// 本地文件下删除
        /// </summary>
        /// <param name="localPath">本地目录</param>
        public static void Delete(string localPath)
        {
            InitInner();
            SvnExecuteOut($"delete \"{localPath}\"");
        }

        /// <summary>
        /// 清除
        /// </summary>
        /// <param name="localPath">本地目录</param>
        public static void Cleanup(string localPath, bool removeUnversioned = false)
        {
            InitInner();
            SvnExecuteOut($"cleanup \"{localPath}\"" + (removeUnversioned?" --remove-unversioned":""));
        }


        /// <summary>
        /// 带提交日志的SVN提交
        /// </summary>
        /// <param name="localPath">本地目录</param>
        /// <param name="comment">提交日志</param>
        public static void Commit(string localPath, string comment)
        {
            InitInner();
            SvnExecuteOut($"ci \"{localPath}\" -m \"{comment}\"");
        }

        public static SvnInfo GetSvnInfo(string pathOrUrl)
        {
            InitInner();

            if (string.IsNullOrEmpty(pathOrUrl)) throw new ArgumentException($"'{pathOrUrl}' is null or empty!");

            var infoStr = SvnExecuteOut($"info \"{pathOrUrl}\"");

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

        private static string GetSvnVersionInner()
        {
            var str = SvnExecuteOut($"--version --quiet");
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
                    return SvnExecuteOut(arguments);
                }
                catch (Exception e)
                {
                    if (!hasCleanup)
                    {// 试着cleanup一下
                        Logger.VerboseLog("Failed, try cleanup...");
                        SvnExecuteOut($"cleanup \"{localPath}\"");

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

        /// <summary>
        /// 判断当前状态行是否包含指定文件状态前缀
        /// </summary>
        /// <param name="outputLine">svn st 输出的单行字符串</param>
        /// <param name="prefix">前缀，比如C、A、！和？符号要转义</param>
        /// <returns></returns>
        private static bool CheckFileStatusFromStatusLine(string outputLine, string prefix)
        {
            return Regex.Match(outputLine, $"(^{prefix}|\\w\\s\\+\\s{prefix})").Success;
        }
        /// <summary>
        /// 当前文件状态中取到文件名
        /// </summary>
        /// <param name="outputLine">svn st 输出的单行字符串</param>
        /// <returns></returns>
        private static string GetFileNameFromStatusLine(string outputLine)
        {
            //匹配前缀中的A 或者A + B或者A + B + C
            var match = Regex.Match(outputLine, @"^\S\s(\+\s\S\s)*");
            //文件名从后面开始
            return outputLine.Substring(match.Index + 1).Trim();
        }

        /// <summary>
        /// 执行SVN操作
        /// </summary>
        /// <param name="arguments">参数</param>
        /// <returns>输出结果字符串</returns>
        private static string SvnExecuteOut(string arguments)
        {
            if (!string.IsNullOrEmpty(s_username))
            {
                return s_svn.ExecuteOut(arguments + " --non-interactive" 
                    + $" --username \"{s_username}\" --password \"{s_password}\" --no-auth-cache");
            }
            else
            {
                return s_svn.ExecuteOut(arguments + " --non-interactive");
            }
        }

        private static void InitInner()
        {
            if (s_onGetSvnExe == null) throw new DoodleException($"{nameof(SvnUtil)} hasn't been Inited!");
            Init(s_onGetSvnExe());
        }

        /// <summary>
        /// 将枚举转为可以附加的参数
        /// </summary>
        /// <param name="conflictAccept">枚举类型</param>
        /// <returns></returns>
        private static string GetAcceptArg(SvnConflictAccept conflictAccept)
        {
            if (conflictAccept == SvnConflictAccept.None)
            {
                return string.Empty;
            }
            else
            {
                return "–accept=" + conflictAccept.ToString().ToLower().Replace("_", "-");
            }
            
        }
    }
}
