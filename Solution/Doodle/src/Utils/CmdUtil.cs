using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Doodle
{
    public static class CmdUtil
    {
        public static string ExecuteOut(string cmd)
        {
            return GenScript(cmd).ExecuteOut(null);
        }

        public static string ExecuteErr(string cmd)
        {
            return GenScript(cmd).ExecuteErr(null);
        }

        public static void Execute(string cmd)
        {
            GenScript(cmd).Execute(null);
        }

        public static void Execute(string cmd, out string stdout, out string stderr)
        {
            GenScript(cmd).Execute(null, out stdout, out stderr);
        }

        public static int ExecuteNoThrow(string cmd)
        {
            return GenScript(cmd).ExecuteNoThrow(null);
        }

        public static int ExecuteNoThrow(string cmd, out string stdout)
        {
            return GenScript(cmd).ExecuteNoThrow(null, out stdout);
        }

        public static int ExecuteNoThrow(string cmd, out string stdout, out string stderr)
        {
            return GenScript(cmd).ExecuteNoThrow(null, out stdout, out stderr);
        }

        private static Script GenScript(string cmd)
        {
            var f = SpaceUtil.NewTempPath() + ".bat";
            File.WriteAllText(f, cmd);

           return new Script(f);
        }
    }
}
