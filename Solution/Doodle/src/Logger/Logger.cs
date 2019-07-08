using System;
using System.Collections.Generic;
using System.IO;

namespace Doodle
{
    public static class Logger
    {
        public static Verbosity verbosity
        {
            get => s_logFiles[0].verbosity;
            set => s_logFiles[0].verbosity = value;
        }

        private static readonly Dictionary<string, int> s_dicLogFileIndex = new Dictionary<string, int>();
        private static readonly List<LogFile> s_logFiles = new List<LogFile>();
        private static readonly List<bool> s_logFileOnOff = new List<bool>();

        private static readonly Stack<bool> s_consoleStateStack = new Stack<bool>();


        static Logger()
        {
            s_logFiles.Add(new LogFile(Console.OpenStandardOutput(), Console.OpenStandardError()));
            s_logFileOnOff.Add(true); // 默认开启

            verbosity = Verbosity.Log;
        }

        public static void SetLogFile(string name, LogFile logFile)
        {
            if (s_dicLogFileIndex.TryGetValue(name, out int index))
            {// 关闭旧的
                s_logFiles[index].Close();

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
        }

        public static void ToggleConsoleOutput(bool on)
        {
            if (s_logFileOnOff[0]  != on)
            {
                s_logFileOnOff[0] = on;
            }
        }

        public static void BeginMuteConsoleOutput()
        {
            s_consoleStateStack.Push(s_logFileOnOff[0]);
            ToggleConsoleOutput(false);
        }

        public static void EndMuteConsoleOutput()
        {
            ToggleConsoleOutput(s_consoleStateStack.Peek());
            s_consoleStateStack.Pop();
        }

        public static void VerboseLog(object message)
        {
            for (int i = 0; i < s_logFiles.Count; ++i)
            {
                if (s_logFileOnOff[i])
                {
                    s_logFiles[i].VerboseLog(message);
                }
            }
        }

        public static void Log(object message)
        {
            for (int i = 0; i < s_logFiles.Count; ++i)
            {
                if (s_logFileOnOff[i])
                {
                    s_logFiles[i].Log(message);
                }
            }
        }

        public static void WarningLog(object message)
        {
            for (int i = 0; i < s_logFiles.Count; ++i)
            {
                if (s_logFileOnOff[i])
                {
                    s_logFiles[i].WarningLog(message);
                }
            }
        }

        public static void ErrorLog(object message)
        {
            for (int i = 0; i < s_logFiles.Count; ++i)
            {
                if (s_logFileOnOff[i])
                {
                    s_logFiles[i].ErrorLog(message);
                }
            }
        }
    }
}
