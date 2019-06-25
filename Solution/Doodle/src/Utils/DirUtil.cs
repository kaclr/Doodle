using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Doodle
{
    public static class DirUtil
    {
        public static bool Exists(string path)
        {
            return PathUtil.GetPathState(path) == PathState.Dir;
        }

        public static void ClearDir(string dir)
        {
            //删除文件夹
            string[] dirs = Directory.GetDirectories(dir);
            for (int i = 0; i < dirs.Length; i++)
            {
                Directory.Delete(dirs[i], true);
            }

            //删除文件
            string[] files = Directory.GetFiles(dir);
            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i]);
            }
        }

        /// <summary>
        /// 创建空目录
        /// </summary>
        /// <param name="dirPath"> 路径 </param>
        public static void CreateEmptyDir(string dirPath)
        {
            if (PathUtil.GetPathState(dirPath) != PathState.None)
            {
                PathUtil.RemovePath(dirPath);
            }

            TryCreateDir(dirPath);
        }

        /// <summary>
        /// 创建目录，路径不为空则报错
        /// </summary>
        public static void CreateDir(string dirPath)
        {
            if (PathUtil.GetPathState(dirPath) != PathState.None)
            {
                throw new DoodleException(string.Format("dirPath '{0}' already exists!", dirPath));
            }

            TryCreateDir(dirPath);
        }

        /// <summary>
        /// 尝试创建目录，会创建所有需要的父级目录
        /// </summary>
        /// <param name="path"> 路径 </param>
        /// <param name="force"> 是否覆盖，如果覆盖，则当路径中存在文件时会删除它再创建需要的文件夹。 </param>
        /// <returns></returns>
        public static bool TryCreateDir(string dirPath, bool force = false)
        {
            var create = false;
            var dirs = dirPath.Split('/', '\\');
            var rootPath = Path.GetPathRoot(dirPath);
            for (int i = string.IsNullOrEmpty(rootPath) ? 0 : 1; i < dirs.Length; ++i)
            {
                var dir = dirs[i];
                var fullPathDir = rootPath + dir;
                var state = PathUtil.GetPathState(fullPathDir);
                if (state == PathState.File)
                {
                    if (force)
                    {
                        PathUtil.TryRemovePath(fullPathDir);
                    }
                    else
                    {
                        throw new DoodleException("a file in the dirPath!");
                    }
                }

                if (state == PathState.None)
                {
                    Directory.CreateDirectory(fullPathDir);
                    create = true;
                }

                rootPath += dir + Path.DirectorySeparatorChar;
            }

            return create;
        }

        /// <summary>
        /// 尝试创建路径需要的所有父级目录
        /// </summary>
        /// <param name="path"> 路径 </param>
        /// <param name="force"> 是否覆盖，如果覆盖，则当路径中存在文件时会删除它再创建需要的文件夹。 </param>
        /// <returns></returns>
        public static bool TryCreateParentDir(string path, bool force = false)
        {
            var dirName = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dirName) && dirName != PathUtil.directorySeparator)
            {
                return TryCreateDir(dirName, force);
            }
            return false;
        }

        public static void CopyDir(string sourDir, string destDir, params string[] limitExts)
        {
            if (!Directory.Exists(sourDir))
            {
                throw new DoodleException(string.Format("Source dir '{0}' isn't exist!", sourDir));
            }

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            CopyDirImpl(new DirectoryInfo(sourDir), destDir, false, limitExts);
        }

        /// <summary>
        /// 拷贝目录
        /// </summary>
        /// <param name="limitExts"> 约束文件后缀，如果有约束则只拷贝约束限定的文件，没有任何约束即为拷贝目录内所有文件 </param>
        public static void CopyDir(string sourDir, string destDir, bool recursive, params string[] limitExts)
        {
            if (!Directory.Exists(sourDir))
            {
                throw new DoodleException(string.Format("Source dir '{0}' isn't exist!", sourDir));
            }

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            CopyDirImpl(new DirectoryInfo(sourDir), destDir, recursive, limitExts);
        }

        private static void CopyDirImpl(DirectoryInfo curSourDir, string destDir, bool recursive, string[] limitExts)
        {
            foreach (var fi in curSourDir.GetFiles())
            {
                bool canCopy = true;
                if (limitExts != null && limitExts.Length > 0)
                {
                    canCopy = false;
                    for (int i = 0; i < limitExts.Length; ++i)
                    {
                        if (fi.Extension == limitExts[i])
                        {
                            canCopy = true;
                            break;
                        }
                    }
                }

                if (canCopy)
                {
                    File.Copy(fi.FullName, Path.Combine(destDir, fi.Name));
                }
            }

            if (recursive)
            {
                foreach (var di in curSourDir.GetDirectories())
                {
                    var childDirName = Path.Combine(destDir, di.Name);
                    Directory.CreateDirectory(childDirName);
                    CopyDirImpl(di, childDirName, recursive, limitExts);
                }
            }
        }
    }
}
