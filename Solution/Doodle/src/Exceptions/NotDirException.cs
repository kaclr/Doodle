using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public class NotDirException : Exception
    {
        public NotDirException(string path, string parameterName)
            : base($"'{path}' is not a directory, parameterName is {parameterName}!")
        {
        }
    }
}
