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

        public static IFLock NewFLock(string filePath)
        {
            InitInner();
            return new FLock(s_flock, filePath);
        }

        private static void InitInner()
        {
            if (DoodleEnv.curPlatform != Platform.OSX) throw new DoodleException($"'{nameof(FLockUtil)}' only valid in OSX for now!");
            if (s_flock != null) return;
            if (s_onGetFLock == null) throw new DoodleException($"{nameof(FLockUtil)} hasn't been Inited!");

            s_flock = new Executable(s_onGetFLock());
        }
    }

    public interface IFLock
    {
        void AcquireExclusiveLock();
        void AcquireShareLock();
        void ReleaseLock();
    }

    internal class FLock : IFLock
    {
        private readonly IExecutable m_flock;
        private readonly string m_filePath;
        private FileStream m_fileStream;

        internal FLock(IExecutable flock, string filePath)
        {
            m_flock = flock;
            m_filePath = filePath;
        }

        public void AcquireExclusiveLock()
        {
            InitFileStream();
            m_flock.Execute($"{m_fileStream.SafeFileHandle.DangerousGetHandle().ToInt32()}");
        }

        public void AcquireShareLock()
        {
            InitFileStream();
            m_flock.Execute($"-s {m_fileStream.SafeFileHandle.DangerousGetHandle().ToInt32()}");
        }

        public void ReleaseLock()
        {
            if (m_fileStream == null)
            {
                throw new DoodleException($"Hasn't lock yet!");
            }

            m_fileStream.Dispose();
            m_fileStream = null;
        }

        private void InitFileStream()
        {
            if (m_fileStream == null)
                m_fileStream = new FileStream(m_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Inheritable);
        }
    }
}
