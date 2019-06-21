using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public abstract class Param
    {
        public abstract Type valueType { get; set; }
        public abstract bool required { get; set; }
        public abstract Func<object> defaultValue { get; set; }
    }
}
