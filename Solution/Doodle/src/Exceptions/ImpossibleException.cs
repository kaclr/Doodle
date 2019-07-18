using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public class ImpossibleException : DoodleException
    {
        public ImpossibleException()
        {
        }

        public ImpossibleException(string message) : base(message)
        {
        }
    }
}
