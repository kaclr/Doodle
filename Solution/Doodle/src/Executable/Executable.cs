using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Doodle
{
    public class Executable : IExecutable
    {
        public bool printToVerbose { get; set; } = true;

        private string m_exePath;
        protected StringBuilder m_stderr;
        protected StringBuilder m_stdout;

        public Executable(string exePath)
        {
            this.m_exePath = exePath;
        }

        public string Execute(string arguments)
        {
            if (ExecuteImpl(m_exePath, arguments, out string stdout, out string stderr) != 0)
            {
                throw new DoodleException($"Execute '{m_exePath} {arguments}' failed, detail as follows:\n{stderr}");
            }
            return stdout;
        }

        private int ExecuteImpl(string exePath, string arguments, out string stdout, out string stderr)
        {
            stdout = null;
            stderr = null;
            int exitCode = 0;
            Process p = new Process();
            try
            {
                p.StartInfo.FileName = exePath;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                p.OutputDataReceived += OnStdout;
                p.ErrorDataReceived += OnStderr;

                p.Start();
                m_stdout = new StringBuilder();
                m_stderr = new StringBuilder();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                p.WaitForExit();
                exitCode = p.ExitCode;
            }
            finally
            {
                p.Close();
            }

            stdout = m_stdout.ToString();
            stderr = m_stderr.ToString();

            return exitCode;
        }

        private void OnStdout(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            m_stdout.AppendLine(e.Data);

            if (printToVerbose)
                Logger.VerboseLog(e.Data);
        }

        private void OnStderr(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            m_stderr.AppendLine(e.Data);

            if (printToVerbose)
                Logger.VerboseLog(e.Data);
        }
    }
}
