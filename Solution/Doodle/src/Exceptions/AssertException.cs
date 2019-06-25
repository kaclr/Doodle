using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public class AssertException : DoodleException
    {
        public AssertException(string message) : base(message) { }
    }
}
