using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public class NotFileException : DoodleException
    {
        public NotFileException(string path, string parameterName)
            : base($"'{path}' is not a file, parameterName is {parameterName}!")
        {
        }
    }
}
