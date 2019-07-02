using System;
using System.IO;

namespace Doodle
{
    public static class Logger
    {
        private static StreamWriter s_outLogFile;
        private static StreamWriter s_errLogFile;
        private static bool s_consoleOutputing;

        static Logger()
        {
            ToggleConsoleOutput(true);
        }

        public static void SetLogFile(string logFile)
        {
            if (!string.IsNullOrEmpty(logFile))
            {
                var f = File.Open(logFile, FileMode.Append, FileAccess.Write, FileShare.Read);
                s_outLogFile = new StreamWriter(f) { AutoFlush = true };
                s_errLogFile = new StreamWriter(f) { AutoFlush = true };
                s_consoleOutputing = false;
            }
            else
            {
                ToggleConsoleOutput(true);
            }
        }

        public static void ToggleConsoleOutput(bool on)
        {
            if (s_consoleOutputing && !on)
            {// 关闭
                if (s_outLogFile != null) s_outLogFile.Close();
                if (s_errLogFile != null) s_errLogFile.Close();

                s_outLogFile = null;
                s_errLogFile = null;
                s_consoleOutputing = false;
            }
            else if (!s_consoleOutputing && on)
            {// 打开
                s_outLogFile = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                s_errLogFile = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
                s_consoleOutputing = true;
            }
        }

        public static void Log(object message)
        {
            if (s_outLogFile == null) return;

            s_outLogFile.WriteLine(message);
        }

        public static void VerboseLog(object message)
        {
            if (s_outLogFile == null) return;

            s_outLogFile.WriteLine(message);
        }

        public static void ErrorLog(object message)
        {
            if (s_errLogFile == null) return;

            s_errLogFile.WriteLine(message);
        }
    }
}
