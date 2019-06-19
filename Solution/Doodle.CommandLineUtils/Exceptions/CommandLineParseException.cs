using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class CommandLineParseException : Exception
    {
        public CommandLineParseException(string message) : base(message) { }
    }
}
