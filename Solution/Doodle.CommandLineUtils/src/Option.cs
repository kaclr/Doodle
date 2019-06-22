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

        public bool isSet
        {
            get;
            internal set;
        }

        public override string displayName => $"option '{template}'";

        private readonly string[] m_templates;

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
                    throw new ParamConfigurationException($"template '{tmplt}' is invalid, every template must start with '-' or '--'!");
                }
            }
        }

        internal bool IsMatchTemplate(string arg)
        {
            return Array.FindIndex<string>(m_templates, template => template == arg) >= 0;
        }
    }
}
