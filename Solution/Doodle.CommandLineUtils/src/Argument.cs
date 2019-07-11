using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public sealed class Argument : Param
    {
        public bool mutiValue
        {
            get;
            private set;
        }

        public override string name
        {
            get;
        }

        public Array values
        {
            get;
            internal set;
        }

        public override Func<object> defaultValue
        {
            get => m_defaultValue;
            set
            {
                if (mutiValue)
                {
                    throw new ParamConfigurationException($"{displayName} with mutiple value can not has default value!");
                }
                m_defaultValue = value;
            }
        }

        private Func<object> m_defaultValue;

        public Argument(string name, string description, bool mutiValue)
        {
            this.name = name;
            this.description = description;
            this.mutiValue = mutiValue;

            valueType = typeof(string);
        }
    }
}
