using System;
using System.Collections.Generic;
using System.IO;

namespace Doodle
{
    public static class Logger
    {
        public static Verbosity verbosity
        {
            get => s_logInfos[0].LogItem.verbosity;
            set => s_logInfos[0].LogItem.verbosity = value;
        }

        class LogInfo
        {

            public ILog LogItem;
            public bool Enable;
            public string Name;
            public LogInfo(ILog log, bool enable, string name = "")
            { 
                LogItem = log;
                Enable = enable;
                Name = name;
            }
        }

        public static bool forceConsoleOutput { get; set; } = false;

        //private static readonly Dictionary<string, int> s_dicLogFileIndex = new Dictionary<string, int>();
        //private static readonly List<ILog> s_logFiles = new List<ILog>();
        //private static readonly List<bool> s_logFileOnOff = new List<bool>();

        private static readonly Stack<bool> s_consoleStateStack = new Stack<bool>();

        private static readonly List<LogInfo> s_logInfos = new List<LogInfo>();

        static Logger()
        {
            //s_logFiles.Add(new LogFile(Console.Out, Console.Error));
            //s_logFileOnOff.Add(true); // 默认开启
            s_logInfos.Add(new LogInfo(new LogFile(Console.Out, Console.Error), true));

            verbosity = Verbosity.Log;
        }

        public static void SetLogFile(string name, LogFile logFile)
        {
            /*
            if (s_dicLogFileIndex.TryGetValue(name, out int index))
            {// 关闭旧的

                ((LogFile)s_logFiles[index]).Close();

                s_dicLogFileIndex.Remove(name);
                s_logFiles.RemoveAt(index);
                s_logFileOnOff.RemoveAt(index);
            }
              if (logFile != null)
            {// 创建新的
                s_dicLogFileIndex.Add(name, s_logFiles.Count);
                s_logFiles.Add(logFile);
                s_logFileOnOff.Add(true);
            }
             */
             //名字不能为空
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{nameof(name)} is empty!", nameof(name));
            }
            //寻找重名的
            var findLogInfo = s_logInfos.Find((log) => { return log.Name == name; });
            if (findLogInfo != null)
            {
                if (findLogInfo.LogItem != logFile)
                {
                    ((LogFile)findLogInfo.LogItem).Close();
                }
                s_logInfos.Remove(findLogInfo);
            }
            s_logInfos.Add(new LogInfo(logFile, true, name));

        }

        public static void TryOffConsoleOutput()
        {
            if (!forceConsoleOutput)
            {
                ToggleConsoleOutput(false);
            }
        }

        public static void ToggleConsoleOutput(bool on)
        {
            s_logInfos[0].Enable = on;
        }

        public static void BeginMuteConsoleOutput()
        {
            s_consoleStateStack.Push(s_logInfos[0].Enable);
            ToggleConsoleOutput(false);
        }

        public static void EndMuteConsoleOutput()
        {
            ToggleConsoleOutput(s_consoleStateStack.Peek());
            s_consoleStateStack.Pop();
        }

        public static void VerboseLog(object message)
        {
            for (int i = 0; i < s_logInfos.Count; ++i)
            {
                if (s_logInfos[i].Enable)
                {
                    s_logInfos[i].LogItem.VerboseLog(message);
                }
            }
            
        }

        public static void Log(object message)
        {
            for (int i = 0; i < s_logInfos.Count; ++i)
            {
                if (s_logInfos[i].Enable)
                {
                    s_logInfos[i].LogItem.Log(message);
                }
            }
        }

        public static void WarningLog(object message)
        {
            for (int i = 0; i < s_logInfos.Count; ++i)
            {
                if (s_logInfos[i].Enable)
                {
                    s_logInfos[i].LogItem.WarningLog(message);
                }
            }
        }

        public static void ErrorLog(object message)
        {
            for (int i = 0; i < s_logInfos.Count; ++i)
            {
                if (s_logInfos[i].Enable)
                {
                    s_logInfos[i].LogItem.ErrorLog(message);
                }
            }
        }
    }
}
