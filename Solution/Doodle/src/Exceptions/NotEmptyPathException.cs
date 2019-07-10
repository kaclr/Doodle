using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public class NotEmptyPathException : Exception
    {
        public NotEmptyPathException(string path, string parameterName)
            : base($"'{path}' is not empty, parameterName is {parameterName}!")
        {
        }
    }
}
