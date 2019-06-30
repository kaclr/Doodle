using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doodle
{
    public class DoodleException : Exception
    {
        public DoodleException() { }
        public DoodleException(string message) : base(message) { }
    }
}
