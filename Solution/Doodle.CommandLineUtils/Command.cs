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
        private readonly List<Argument> m_arguments = new List<Argument>();
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
            // 先解析option
            foreach (var option in m_options)
            {
                string rawValue = null;
                int consumeCount = 0;

                var index = lstArg.FindIndex(arg => option.IsMatchTemplate(arg));
                option.isSet = index >= 0;
                option.value = null;

                if (option.isSet)
                {// 设置了option

                    consumeCount = 1;
                    if (option.optionType == OptionType.SingleValue)
                    {
                        if (index + 1 >= lstArg.Count)
                        {
                            throw new CommandLineParseException($"Option '{lstArg[index]}' has no value!");
                        }

                        consumeCount = 2;
                        rawValue = lstArg[index + 1];
                    }
                }

                option.value = ObtainValue(rawValue, option,
                    $"Option '{option.template}' 's value type is mismatched, need type is '{option.valueType}', receive type is '{option.value.GetType()}'!",
                    $"Lack of required option '{option.template}'!");

                // 消耗arg
                lstArg.RemoveRange(index, consumeCount);
            }

            // 再解析argument
            foreach (var argument in m_arguments)
            {

            }


            var retValue = m_onExecute();

            if (lstArg.Count > 0 && m_subCommands.TryGetValue(lstArg[0], out var subCommand))
            {// 调用子命令

                // 使用子命令的返回值
                retValue = subCommand.ExecuteImpl(lstArg);
            }
            return retValue;
        }

        private object ObtainValue(string rawValue, Param param, string errOnType, string errOnRequired)
        {
            object value = null;
            if (rawValue != null)
            {
                s_type2Converter.TryGetValue(param.valueType, out Func<string, object> converter);
                value = converter(rawValue);
            }
            else
            {
                if (param.required)
                {// 未设置必选param
                    throw new CommandLineParseException(errOnRequired);
                }

                value = param.defaultValue();
            }

            if (value.GetType() != param.valueType)
            {// 类型不匹配
                throw new CommandLineParseException(errOnType);
            }

            return value;
        }
    }
}
