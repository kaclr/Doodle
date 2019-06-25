using System;
using System.IO;
using System.Text;

namespace Doodle
{
    public enum PathState
    {
        None, // 路径不含文件和目录
        File, // 路径存在文件
        Dir, // 路径存在目录
    }

    public enum PathRelation
    {
        Same, // 路径一致
        Child, // 子关系，比如 A/B/C 是 A 的子  
        Parent, // 父关系，比如 A 是 A/B/C 的父
        Brother, // 兄弟关系，在同目录下
        Irrelevant, // 不相关，比如 A/B/C 和 A/D 
    }

    /// <summary>
    /// 路径相关工具函数，路径的概念包括文件和文件夹
    /// </summary>
    public static class PathUtil
    {
        public static string directorySeparator
        {
            get;
            private set;
        }

        static PathUtil()
        {
            directorySeparator = Path.DirectorySeparatorChar + "";
        }

        /// <summary>
        /// 转换路径为当前平台的标准路径格式
        /// </summary>
        public static string ToNormalizePlatformPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (DoodleEnv.curPlatform == Platform.OSX)
            {
                return ToNormalizeLinuxPath(path);
            }
            else
            {
                return ToNormalizeWindowsPath(path);
            }
        }

        /// <summary>
        /// 转换路径为Linux的标准路径格式
        /// </summary>
        public static string ToNormalizeLinuxPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            return path.Replace('\\', '/').TrimEnd('/');

        }

        /// <summary>
        /// 转换路径为Windows的标准路径格式
        /// </summary>
        public static string ToNormalizeWindowsPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            return path.Replace('/', '\\').TrimEnd('\\');
        }

        public static string Combine(params string[] paths)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var path in paths)
            {
                sb.Append(path);
            }

            return ToNormalizePlatformPath(sb.ToString());
        }

        /// <summary>
        /// 尝试删除指定路径
        /// </summary>
        public static bool TryRemovePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试删除指定路径
        /// </summary>
        public static void RemovePath(string path)
        {
            if (!TryRemovePath(path))
            {
                throw new DoodleException(string.Format("删除目录 {0} 失败", path));
            }
        }

        /// <summary>
        /// 路径存在性判断
        /// </summary>
        public static bool ExistsPath(string path)
        {
            if (File.Exists(path))
            {
                return true;
            }
            else if (Directory.Exists(path))
            {
                return true;
            }

            return false;
        }

        public static void CopyPathToDir(string sourPath, string destDir, bool force)
        {
            CopyPath(sourPath, Path.Combine(destDir, Path.GetFileName(sourPath)), force);
        }

        /// <summary>
        /// 拷贝源路径到目标路径
        /// </summary>
        /// <param name="sourPath"> 源路径 </param>
        /// <param name="destPath"> 目标路径 </param>
        /// <param name="force"> 是否强制覆盖目标路径 </param>
        public static void CopyPath(string sourPath, string destPath, bool force)
        {
            if (sourPath == destPath)
            {
                throw new DoodleException("sourPath and destPath can't be same!");
            }


            int wildcardIndex = sourPath.LastIndexOf("*", System.StringComparison.Ordinal);
            bool srcWildcard = wildcardIndex >= 0;
            string needCheckPath = sourPath;
            if (srcWildcard)
            {
                int lastSlashIndex = sourPath.LastIndexOf('/');
                if (lastSlashIndex > wildcardIndex)
                {
                    throw new DoodleException(string.Format("sourPath {0} 是不合法的路径！", sourPath));
                }

                needCheckPath = Path.GetDirectoryName(sourPath);
            }

            var sourState = GetPathState(needCheckPath);
            if (sourState == PathState.None)
            {
                throw new DoodleException(string.Format("{0} 路径不存在！", needCheckPath));
            }

            var destState = GetPathState(destPath);
            if (!force && destState != PathState.None)
            {
                throw new DoodleException("destPath is not empty!");
            }

            if (srcWildcard)
            {
                if (destState == PathState.File)
                {
                    if (force)
                    {
                        PathUtil.RemovePath(destPath);
                    }
                    else
                    {
                        throw new DoodleException(string.Format("destPath {0} 不能是一个文件，除非指明force！", destPath));
                    }
                }

                var files = Directory.GetFiles(Path.GetDirectoryName(sourPath), Path.GetFileName(sourPath));
                DirUtil.TryCreateDir(destPath);
                foreach (var f in files)
                {
                    PathUtil.CopyPath(f, Path.Combine(destPath, Path.GetFileName(f)), force);
                }
            }
            else
            {
                var relation = GetPathRelation(sourPath, destPath);
                if (sourState == PathState.Dir && relation == PathRelation.Parent)
                {
                    throw new DoodleException("can't copy a directory into itself!");
                }

                if (destState == PathState.Dir && relation == PathRelation.Child)
                {
                    throw new DoodleException("can't overwrite sourPath's parent dir!");
                }

                DirUtil.TryCreateParentDir(destPath, force);
                TryRemovePath(destPath);
                if (sourState == PathState.File)
                {
                    File.Copy(sourPath, destPath);
                }
                else if (sourState == PathState.Dir)
                {
                    DirUtil.CopyDir(sourPath, destPath, true);
                }
            }
        }

        /// <summary>
        /// 获取path1与path2的关系
        /// </summary>
        public static PathRelation GetPathRelation(string path1, string path2)
        {
            if (path1 == path2)
            {
                return PathRelation.Same;
            }

            if (Path.GetDirectoryName(path1) == Path.GetDirectoryName(path2))
            {
                return PathRelation.Brother;
            }

            if (path1.StartsWith(path2))
            {
                return PathRelation.Child;
            }

            if (path2.StartsWith(path1))
            {
                return PathRelation.Parent;
            }

            return PathRelation.Irrelevant;
        }

        /// <summary>
        /// 获取指定路径的状态
        /// </summary>
        public static PathState GetPathState(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return PathState.None;
            }

            if (File.Exists(path))
            {
                return PathState.File;
            }
            else if (Directory.Exists(path))
            {
                return PathState.Dir;
            }

            return PathState.None;
        }
    }
}
