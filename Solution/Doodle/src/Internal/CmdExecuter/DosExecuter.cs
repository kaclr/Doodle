using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Doodle
{
    internal class DosExecuter : CmdExecuter, ICmdExecuter
    {
        protected override string fileName
        {
            get
            {
                return "cmd.exe";
            }
        }

        public override bool ExistsExec(string execPath)
        {
            return ExecuteCmd(string.Format("where \"{0}\"", execPath), out _) == 0;
        }

        protected override string GetArguments(string cmd)
        {
            return string.Format("/c \"{0}\"", cmd);
        }
    }
}
