using System;
using System.Collections.Generic;

namespace Doodle.CommandLineUtils
{
    public class Command
    {
        private static readonly Dictionary<Type, Func<string, object>> s_type2Converter = new Dictionary<Type, Func<string, object>>();

        private readonly List<Option> m_options = new List<Option>();

        public int Execute(params string[] args)
        {
            List<string> lstArg = new List<string>(args);
            return ExecuteImpl(lstArg);
        }

        public void OnExecute(Func<int> onExecuteFunc)
        {
            throw new NotImplementedException();
        }

        public void AddSubCommand(Command command)
        {
            throw new NotImplementedException();
        }

        public Argument DefineArgument(string name, string description, bool mutipleValues = false)
        {
            throw new NotImplementedException();
        }

        public Option DefineOption(string template, string description, OptionType optionType)
        {
            throw new NotImplementedException();
        }

        protected int ExecuteImpl(List<string> lstArg)
        {
            foreach (var option in m_options)
            {
                var index = lstArg.FindIndex(arg => throw new NotImplementedException());
                var inputOptionTemplate = lstArg[index];
                if (index >= 0)
                {// 设置了option
                    option.isSet = true;

                    int consumeCount = 1;
                    if (option.optionType == OptionType.SingleValue)
                    {
                        if (index + 1 >= lstArg.Count)
                        {
                            throw new CommandLineParseException($"Option '{inputOptionTemplate}' has no value!");
                        }

                        consumeCount = 2;
                        string strValue = lstArg[index + 1];
                        if (!s_type2Converter.TryGetValue(option.valueType, out Func<string, object> converter))
                        {
                            throw new CommandLineParseException($"Option '{inputOptionTemplate}' has no Converter, type is '{option.valueType}'!");
                        }
                        option.value = converter(strValue);
                    }

                    // 消耗arg
                    lstArg.RemoveRange(index, consumeCount);
                }
                else
                {// 没有设置option
                    if (option.required)
                    {// 未设置必选option
                        throw new CommandLineParseException($"Lack of required option '{option.template}'!");
                    }

                    option.isSet = false;
                    option.value = option.defaultValue();
                }

                if (option.value.GetType() != option.valueType)
                {// 类型不匹配
                    throw new CommandLineParseException($"Option '{inputOptionTemplate}' 's value type is missmatched, need type is '{option.valueType}', receive type is '{option.value.GetType()}'!");
                }
            }
        }
    }
}
