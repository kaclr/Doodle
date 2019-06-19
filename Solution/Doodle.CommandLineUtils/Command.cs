using System;
using System.Collections.Generic;

namespace Doodle.CommandLineUtils
{
    public class Command
    {
        internal static readonly Dictionary<Type, Func<string, object>> s_type2Converter;

        static Command()
        {
            s_type2Converter = new Dictionary<Type, Func<string, object>>();

            // 注册基本类型的converter
            s_type2Converter.Add(typeof(string), str => str);
        }

        public string name { get; set; }

        private readonly List<Option> m_options = new List<Option>();
        private readonly Dictionary<string, Command> m_subCommands = new Dictionary<string, Command>();
        private Func<int> m_onExecute;

        public Command(string name)
        {
            this.name = name;
        }

        public int Execute(params string[] args)
        {
            List<string> lstArg = new List<string>(args);
            return ExecuteImpl(lstArg);
        }

        public void OnExecute(Func<int> onExecute)
        {
            m_onExecute = onExecute;
        }

        public Command AddSubCommand(Command command)
        {
            m_subCommands.Add(command.name, command);
            return command;
        }

        public Option AddOption(Option option)
        {
            m_options.Add(option);
            return option;
        }

        protected int ExecuteImpl(List<string> lstArg)
        {
            foreach (var option in m_options)
            {
                var index = lstArg.FindIndex(arg => option.IsMatchTemplate(arg));
                if (index >= 0)
                {// 设置了option
                    option.isSet = true;

                    int consumeCount = 1;
                    if (option.optionType == OptionType.SingleValue)
                    {
                        if (index + 1 >= lstArg.Count)
                        {
                            throw new CommandLineParseException($"Option '{lstArg[index]}' has no value!");
                        }

                        consumeCount = 2;
                        string strValue = lstArg[index + 1];
                        s_type2Converter.TryGetValue(option.valueType, out Func<string, object> converter);
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
                    throw new CommandLineParseException($"Option '{option.template}' 's value type is mismatched, need type is '{option.valueType}', receive type is '{option.value.GetType()}'!");
                }
            }

            // todo argument


            var retValue = m_onExecute();

            if (lstArg.Count > 0 && m_subCommands.TryGetValue(lstArg[0], out var subCommand))
            {// 调用子命令

                // 使用子命令的返回值
                retValue = subCommand.ExecuteImpl(lstArg);
            }
            return retValue;
        }
    }
}
