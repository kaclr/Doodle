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

    public class Option : Param
    {
        public string template
        {
            get;
            set;
        }

        public string description
        {
            get;
            set;
        }

        public OptionType optionType
        {
            get;
            set;
        }

        public override bool required
        {
            get;
            set;
        }

        public override Func<object> defaultValue
        {
            get;
            set;
        }

        public override Type valueType
        {
            get { return m_valueType; }
            set
            {
                if (!Command.s_type2Converter.ContainsKey(value))
                {
                    throw new CommandLineParseException($"Option '{template}' with value type '{value}' has no Converter, you need register it first!");
                }
                m_valueType = value;
            }
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

        private Type m_valueType;
        private string[] m_templates;

        public Option(string template, string description, OptionType optionType)
        {
            this.template = template;
            this.description = description;
            this.optionType = optionType;

            required = false;
            valueType = typeof(string);

            m_templates = template.Split('|');
            // 检查template合法性
            foreach (var tmplt in m_templates)
            {
                if (!tmplt.StartsWith("-") && !tmplt.StartsWith("--"))
                {
                    throw new ArgumentException($"template '{tmplt}' is invalid, every template must start with '-' or '--'!");
                }
            }
        }

        internal bool IsMatchTemplate(string arg)
        {
            return Array.FindIndex<string>(m_templates, template => template == arg) >= 0;
        }
    }
}
