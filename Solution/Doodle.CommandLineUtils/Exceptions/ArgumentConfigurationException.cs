using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class ArgumentConfigurationException : Exception
    {
        public ArgumentConfigurationException(string message) : base(message) { }
    }
}
