using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Doodle
{
    internal abstract class CmdExecuter: ICmdExecuter
    {
        protected abstract string fileName
        {
            get;
        }

        protected StringBuilder m_stderr;
        protected StringBuilder m_stdout;

        private readonly string m_fileName;

        public CmdExecuter()
        {
            m_fileName = fileName;
        }

        public string ExecuteCmd(string cmd)
        {
            string stdout;
            string stderr;
            var exitCode = ExecuteCmdOE(cmd, out stdout, out stderr);
            if (exitCode != 0)
            {
                throw new CmdExecuteException(string.Format("执行命令错误:\n{0}\n退出码为{1}\n错误如下:\n{2}", cmd, exitCode, m_stderr.ToString()));
            }

            if (!string.IsNullOrEmpty(stdout) && stdout[stdout.Length - 1] == '\n')
            {// 删除最后的\n
                stdout = stdout.Remove(stdout.Length - 1);
            }

            return stdout;
        }

        public int ExecuteCmd(string cmd, out string stderr)
        {
            string stdout;
            stderr = null;
            return ExecuteCmdOE(cmd, out stdout, out stderr);
        }

        public int ExecuteCmdOE(string cmd, out string stdout, out string stderr)
        {
            stdout = null;
            stderr = null;
            int exitCode = 0;
            Process p = new Process();
            try
            {
                p.StartInfo.FileName = m_fileName;
                p.StartInfo.Arguments = GetArguments(cmd);
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


        public abstract bool ExistsExec(string execPath);

        protected abstract string GetArguments(string cmd);


        protected void OnStdout(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            m_stdout.AppendLine(e.Data);
        }

        protected void OnStderr(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            m_stderr.AppendLine(e.Data);
        }

    }
}
