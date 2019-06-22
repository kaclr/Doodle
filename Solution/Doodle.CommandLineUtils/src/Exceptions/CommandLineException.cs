using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class CommandLineException : Exception
    {
        public CommandLineException(string message) : base(message) { }
    }
}
