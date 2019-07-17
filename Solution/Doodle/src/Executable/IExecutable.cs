using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public interface IExecutable
    {
        string ExecuteOut(string arguments);
        string ExecuteErr(string arguments);
        void Execute(string arguments);
        void Execute(string arguments, out string stdout, out string stderr);

        int ExecuteNoThrow(string arguments);
        int ExecuteNoThrow(string arguments, out string stdout);
        int ExecuteNoThrow(string arguments, out string stdout, out string stderr);
    }
}
