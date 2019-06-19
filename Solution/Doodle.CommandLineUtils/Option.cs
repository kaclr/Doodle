using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public enum OptionType
    {
        SingleValue,
        NoValue,
    }

    public class Option
    {
        public string template
        {
            get;
            set;
        }

        public OptionType optionType
        {
            get;
            set;
        }

        public bool required
        {
            get;
            set;
        }

        public Func<object> defaultValue
        {
            get;
            set;
        }

        public Type valueType
        {
            get;
            set;
        }

        public bool isSet
        {
            get;
            internal set;
        }

        public object value
        {
            get;
            internal set;
        }
    }
}
