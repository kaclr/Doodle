using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public class IntValueType : ITypeConfiguration
    {
        public Type type => typeof(int);

        public string Check(object value)
        {
            return null;
        }

        public object Convert(string rawValue)
        {
            return int.Parse(rawValue);
        }
    }
}
