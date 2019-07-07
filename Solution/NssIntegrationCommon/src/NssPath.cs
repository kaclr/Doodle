using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NssIntegration
{
    public static class NssPath
    {
        public static bool IsToolsDir(string path, out string error)
        {
            error = null;
            if (!Directory.Exists(path))
            {
                error = string.Format("路径'{0}'不是一个存在的目录！", path);
                return false;
            }

            if (!Directory.Exists(Path.Combine(path, "BuildTools")))
            {
                error = string.Format("路径'{0}'不是一个合法的客户端Tools目录！", path);
                return false;
            }

            return true;
        }
    }
}
