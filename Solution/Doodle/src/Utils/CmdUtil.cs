using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public static class CmdUtil
    {
        private static readonly ICmdExecuter m_cmdExecuter;

        static CmdUtil()
        {
            if (DoodleEnv.curPlatform == Platform.OSX)
            {
                m_cmdExecuter = new ShellExecuter();
            }
            else
            {
                m_cmdExecuter = new DosExecuter();
            }
        }

        public static string ExecuteCmd(string cmd)
        {
            return m_cmdExecuter.ExecuteCmd(cmd);
        }

        public static int ExecuteCmd(string cmd, out string stderr)
        {
            return m_cmdExecuter.ExecuteCmd(cmd, out stderr);
        }

        public static int ExecuteCmdOE(string cmd, out string stdout, out string stderr)
        {
            return m_cmdExecuter.ExecuteCmdOE(cmd, out stdout, out stderr);
        }

        public static bool ExistsExec(string execPath)
        {
            return m_cmdExecuter.ExistsExec(execPath);
        }
    }
}
