using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public abstract class Param : ICLDisplayItem
    {
        public Type valueType
        {
            get { return m_valueType; }
            set
            {
                if (!CLTypeRegistration.ContainsConverter(value))
                {
                    throw new CommandLineParseException($"{displayName} with value type '{value}' has no Converter, you need register it in {typeof(CLTypeRegistration).Name} first!");
                }
                m_valueType = value;
            }
        }

        public string displayName
        {
            get => $"{GetType().Name.ToLower()} '{name}'";
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

        public abstract string name
        {
            get;
        }

        public string description
        {
            get;
            set;
        }

        public string helpText
        {
            get;
            set;
        }

        public object value
        {
            get;
            internal set;
        }

        private Type m_valueType;
    }
}
