using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Doodle
{
    /// <summary>
    /// Unix flock wrapper
    /// </summary>
    public static class FLockUtil
    {
        private static Func<string> s_onGetFLock;
        [ThreadStatic]
        private static IExecutable s_flock;

        public static void Init(Func<string> onGetFLock)
        {
            s_onGetFLock = onGetFLock;
        }

        public static FileStream Lock(string filePath, bool exclusive = false)
        {
            var f = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Inheritable);
            string arguments = "";
            if (!exclusive)
            {
                arguments = "-s ";
            }
            arguments += $"{f.SafeFileHandle.DangerousGetHandle().ToInt32()}";
            s_flock.Execute(arguments);
            return f;
        }

        private static void InitInner()
        {
            if (s_flock != null) return;
            if (s_onGetFLock == null) throw new DoodleException($"{nameof(FLockUtil)} hasn't been Inited!");

            s_flock = new Executable(s_onGetFLock());
        }
    }
}
