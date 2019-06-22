using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class ParamConfigurationException : CommandLineException
    {
        public ParamConfigurationException(string message) : base(message) { }
    }
}
