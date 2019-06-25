using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public abstract class Param
    {
        public Type valueType
        {
            get { return m_valueType; }
            set
            {
                if (!TypeRegistration.ContainsConverter(value))
                {
                    throw new CommandLineParseException($"{displayName} with value type '{value}' has no Converter, you need register it in {typeof(TypeRegistration).Name} first!");
                }
                m_valueType = value;
            }
        }

        public Func<object, string> valueChecker
        {
            get;
            set;
        }

        public virtual Func<object> defaultValue
        {
            get;
            set;
        }

        public string description
        {
            get;
            set;
        }

        public object value
        {
            get;
            internal set;
        }

        public abstract string displayName { get; }

        private Type m_valueType;
    }
}
