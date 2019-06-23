using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public abstract class Param
    {
        public Type valueType
        {
            get
            {
                return typeConfiguration.type;
            }
            set
            {
                if (!CustomTypeFactory.s_dicValueType.TryGetValue(value, out ITypeConfiguration customType))
                {
                    throw new ParamConfigurationException($"Unknown type '{value.Name}', please register it in {typeof(CustomTypeFactory).Name}");
                }
                this.typeConfiguration = customType;
            }
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

        internal ITypeConfiguration typeConfiguration
        {
            get;
            private set;
        }

        protected Param()
        {
            typeConfiguration = new StringValueType();
        }
    }
}
