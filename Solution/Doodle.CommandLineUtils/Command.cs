using System;
using System.Collections.Generic;
using Doodle;

namespace Doodle.CommandLineUtils
{
    public class Command
    {
        internal static readonly Dictionary<Type, Func<string, object>> s_type2Converter;

        static Command()
        {
            s_type2Converter = new Dictionary<Type, Func<string, object>>
            {
                // 注册基本类型的converter
                { typeof(string), str => str }
            };
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
            // 先处理option
            foreach (var option in m_options)
            {
                var index = lstArg.FindIndex(arg => option.IsMatchTemplate(arg));
                option.isSet = index >= 0;
                option.value = null;

                if (option.isSet)
                {// 设置了option

                    int consumeCount = 1;
                    if (option.optionType == OptionType.SingleValue)
                    {
                        if (index + 1 >= lstArg.Count)
                        {
                            throw new CommandLineParseException($"Option '{lstArg[index]}' has no value!");
                        }

                        consumeCount = 2;
                        string rawValue = lstArg[index + 1];
                        option.value = HandleSettedValue(option, rawValue);
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

                    option.value = HandleUnsetValue(option);
                }
            }

            // 再处理argument
            foreach (var argument in m_arguments)
            {
                if (argument.mutiValue)
                {// 变长参数

                    // 创建对应类型的数组，大小要吃光所有参数
                    var values = Array.CreateInstance(argument.valueType, lstArg.Count);
                    for (int i = 0; i < lstArg.Count; ++i)
                    {
                        values.SetValue(HandleSettedValue(argument, lstArg[i]), i);
                    }

                    argument.value = values;
                    argument.values = values;

                    // 消耗arg
                    lstArg.Clear();
                }
                else
                {
                    if (lstArg.Count >= 1)
                    {// 有值
                        string rawValue = lstArg[0];
                        argument.value = HandleSettedValue(argument, rawValue);

                        // 消耗arg
                        lstArg.RemoveAt(0);
                    }
                    else
                    {// 无值
                        argument.value = HandleUnsetValue(argument);
                    }
                }
            }


            var retValue = m_onExecute();

            if (lstArg.Count > 0 && m_subCommands.TryGetValue(lstArg[0], out var subCommand))
            {// 调用子命令

                // 使用子命令的返回值
                retValue = subCommand.ExecuteImpl(lstArg);
            }
            return retValue;
        }

        private object HandleSettedValue(Param param, string rawValue)
        {
            Debug.Assert(s_type2Converter.TryGetValue(param.valueType, out Func<string, object> converter));
            var value = converter(rawValue);

            if (value.GetType() != param.valueType)
            {// 类型不匹配
                throw new CommandLineParseException($"The value type of {param.displayName} is mismatched, need type is '{param.valueType}', receive type is '{value.GetType()}'!");
            }

            return value;
        }

        private object HandleUnsetValue(Param param)
        {
            if (param.defaultValue == null)
            {
                throw new CommandLineParseException($"Unset {param.displayName} without defaultValue!");
            }

            var value = param.defaultValue();
            if (value.GetType() != param.valueType)
            {// 类型不匹配
                throw new CommandLineParseException($"The value type of {param.displayName} is mismatched, need type is '{param.valueType}', receive type is '{value.GetType()}'!");
            }

            return value;
        }
    }
}
