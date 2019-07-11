using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Doodle
{
    public class LogFile
    {
        public Verbosity verbosity { get; set; } = Verbosity.Verbose;

        private readonly TextWriter m_outFile;
        private readonly TextWriter m_errFile;
        private readonly bool m_isOutErrSame;

        public LogFile(string file)
        {
            var f = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            m_outFile = new StreamWriter(f) { AutoFlush = true };
            m_errFile = new StreamWriter(f) { AutoFlush = true };
            m_isOutErrSame = true;
        }

        public LogFile(string file, bool append)
        {
            var f = File.Open(file, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read);
            m_outFile = new StreamWriter(f) { AutoFlush = true };
            m_errFile = new StreamWriter(f) { AutoFlush = true };
            m_isOutErrSame = true;
        }

        public LogFile(Stream outStream, Stream errStream)
        {
            m_outFile = new StreamWriter(outStream) { AutoFlush = true };
            m_errFile = new StreamWriter(errStream) { AutoFlush = true };
            m_isOutErrSame = false;
        }

        public LogFile(TextWriter outWriter, TextWriter errWriter)
        {
            m_outFile = outWriter;
            m_errFile = errWriter;
            m_isOutErrSame = false;
        }

        public void Close()
        {
            m_outFile.Close();
            if (!m_isOutErrSame)
                m_errFile.Close();
        }

        public void VerboseLog(object message)
        {
            if (verbosity < Verbosity.Verbose) return;

            m_outFile.WriteLine(message);
        }

        public void Log(object message)
        {
            if (verbosity < Verbosity.Log) return;

            m_outFile.WriteLine(message);
        }

        public void WarningLog(object message)
        {
            if (verbosity < Verbosity.Warning) return;

            m_errFile.WriteLine(message);
        }

        public void ErrorLog(object message)
        {
            if (verbosity < Verbosity.Error) return;

            m_errFile.WriteLine(message);
        }
    }
}
