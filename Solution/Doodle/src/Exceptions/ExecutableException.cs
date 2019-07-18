using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public class ExecutableException : DoodleException
    {
        public ExecutableException()
        {
        }

        public ExecutableException(string message) : base(message)
        {
        }
    }
}
