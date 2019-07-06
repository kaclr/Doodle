using System;
using System.IO;

namespace Doodle
{
    public static class Logger
    {
        public enum Verbosity
        {
            Exception = 0,
            Error,
            Warning,
            Log,
            Verbose
        }

        public static Verbosity verbosity
        {
            get;
            set;
        }

        private static StreamWriter s_outLogFile;
        private static StreamWriter s_errLogFile;
        private static bool s_consoleOutputing;
        private static bool s_hasLogFile;

        static Logger()
        {
            s_outLogFile = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            s_errLogFile = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
            s_consoleOutputing = true;
            s_hasLogFile = false;

            verbosity = Verbosity.Log;
        }

        public static void TurnOnLogFile(string logFile, bool append = true)
        {
            if (string.IsNullOrEmpty(logFile)) throw new ArgumentException($"{nameof(logFile)} is empty!", nameof(logFile));

            if (s_hasLogFile)
            {
                s_outLogFile.Flush();
                s_errLogFile.Flush();

                // 因为s_outLogFile和s_errLogFile指向同一个文件，所以只close一次。
                s_outLogFile.Close();
            }

            var f = File.Open(logFile, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read);

            s_outLogFile = new StreamWriter(f) { AutoFlush = true };
            s_errLogFile = new StreamWriter(f) { AutoFlush = true };
            s_hasLogFile = true;
            s_consoleOutputing = false;
        }

        public static void TurnOffLogFile()
        {
            if (s_hasLogFile)
            {
                s_outLogFile.Flush();
                s_errLogFile.Flush();

                // 因为s_outLogFile和s_errLogFile指向同一个文件，所以只close一次。
                s_outLogFile.Close();

                s_outLogFile = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                s_errLogFile = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
                s_hasLogFile = false;
                s_consoleOutputing = true;
            }
        }

        public static void ToggleConsoleOutput(bool on)
        {
            if (on && !s_consoleOutputing)
            {// 打开
                if (s_hasLogFile)
                {
                    s_outLogFile.Flush();
                    s_errLogFile.Flush();

                    // 因为s_outLogFile和s_errLogFile指向同一个文件，所以只close一次。
                    s_outLogFile.Close();
                }

                s_outLogFile = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                s_errLogFile = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
                s_hasLogFile = false;
                s_consoleOutputing = true;
            }
            else if (!on && s_consoleOutputing)
            {// 关闭
                s_outLogFile = null;
                s_errLogFile = null;
                s_hasLogFile = false;
                s_consoleOutputing = false;
            }
        }

        public static void VerboseLog(object message)
        {
            if (verbosity < Verbosity.Verbose) return;
            if (s_outLogFile == null) return;

            s_outLogFile.WriteLine(message);
        }

        public static void Log(object message)
        {
            if (verbosity < Verbosity.Log) return;
            if (s_outLogFile == null) return;

            s_outLogFile.WriteLine(message);
        }

        public static void WarningLog(object message)
        {
            if (verbosity < Verbosity.Warning) return;
            if (s_errLogFile == null) return;

            s_errLogFile.WriteLine(message);
        }

        public static void ErrorLog(object message)
        {
            if (verbosity < Verbosity.Error) return;
            if (s_errLogFile == null) return;

            s_errLogFile.WriteLine(message);
        }
    }
}
