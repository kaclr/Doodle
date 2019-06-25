using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public class CmdExecuteException : DoodleException
    {
        public CmdExecuteException(string message) : base(message) { }
    }
}
