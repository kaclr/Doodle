﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class CommandLineException : DoodleException
    {
        public CommandLineException(string message) : base(message) { }
    }
}
