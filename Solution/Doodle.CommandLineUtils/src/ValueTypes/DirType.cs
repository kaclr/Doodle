using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class DirValueType : ITypeConfiguration
    {
        public Type type => typeof(string);

        public string Check(object value)
        {
            if (!Directory.Exists((string)value))
            {
                return $"{value} is not a existing directory";
            }

            return null;
        }

        public object Convert(string rawValue)
        {
            return rawValue;
        }
    }
}
