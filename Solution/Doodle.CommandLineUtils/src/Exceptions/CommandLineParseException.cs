﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class CommandLineParseException : CommandLineException
    {
        public CommandLineParseException(string message) : base(message) { }
        public CommandLineParseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
