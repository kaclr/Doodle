using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public interface ITypeConfiguration
    {
        Type type { get;  }

        object Convert(string rawValue);
        string Check(object value);
    }
}
