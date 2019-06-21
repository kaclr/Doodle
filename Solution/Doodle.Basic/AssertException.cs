using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public class AssertException : Exception
    {
        public AssertException(string message) : base(message) { }
    }
}
