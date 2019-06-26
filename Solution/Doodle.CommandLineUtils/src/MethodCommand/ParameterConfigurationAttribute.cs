using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParameterConfigurationAttribute : Attribute
    {
        public string optionTemplate
        {
            get;
            set;
        }

        public string description
        {
            get;
            private set;
        }

        public bool required
        {
            get;
            set;
        }

        public ParameterConfigurationAttribute(string description)
        {
            this.description = description;

            this.required = false;
        }
    }
}
