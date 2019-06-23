using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class StringValueType : ITypeConfiguration
    {
        public Type type => typeof(string);

        public string Check(object value)
        {
            return null;
        }

        public object Convert(string rawValue)
        {
            return rawValue;
        }
    }
}
