using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Doodle
{
    internal class ShellExecuter : CmdExecuter, ICmdExecuter
    {
        protected override string fileName
        {
            get
            {
                return "sh";
            }
        }

        public override bool ExistsExec(string execPath)
        {
            return ExecuteCmd(string.Format("whereis \"{0}\"", execPath), out _) == 0;
        }

        protected override string GetArguments(string cmd)
        {
            var temp = Path.GetTempFileName();
            File.WriteAllText(temp, cmd);

            return temp;
        }
    }
}
