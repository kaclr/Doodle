using System;
using System.Collections.Generic;
using Doodle;

namespace Doodle.CommandLineUtils
{
    public class Command
    {
        public string name { get; set; }

        private readonly List<Option> m_options = new List<Option>();
        private readonly List<Argument> m_arguments = new List<Argument>();
        private readonly Dictionary<string, Command> m_subCommands = new Dictionary<string, Command>();
        private Func<int> m_onExecute;
        private bool m_hasDefaultArgument;
        private bool m_hasMutiValue;

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

        public Param AddParam(Param param)
        {
            if (param is Argument)
            {
                return AddArgument((Argument)param);
            }
            else
            {
                return AddOption((Option)param);
            }
        }

        public Argument AddArgument(Argument argument)
        {
            if (m_hasMutiValue)
            {// 变长参数只能是最后一个参数
                throw new ParamConfigurationException($"Command '{name}' already exists a multiple value argument, can not have other argument!");
            }
            if (m_hasDefaultArgument && argument.defaultValue == null)
            {// 有默认值的参数后面只能是有默认值的参数
                throw new ParamConfigurationException($"{argument.displayName} is a default argument, must at the end of the arguments!");
            }

            m_hasDefaultArgument = argument.defaultValue != null;
            m_hasMutiValue = argument.mutiValue;

            m_arguments.Add(argument);

            return argument;
        }

        public Option AddOption(Option option)
        {
            m_options.Add(option);
            return option;
        }

        public IEnumerable<Argument> EnumArguments()
        {
            return m_arguments;
        }

        protected int ExecuteImpl(List<string> lstArg)
        {
            // 先处理option
            foreach (var option in m_options)
            {
                // 查找用户是否设置了option
                var optionIndex = lstArg.FindIndex(arg => option.IsMatchTemplate(arg));
                option.isSet = optionIndex >= 0;
                option.value = null;

                if (option.isSet)
                {// 设置了option

                    int consumeCount = 1;
                    if (option.optionType == OptionType.SingleValue)
                    {
                        var valueIndex = optionIndex + 1; // 值应该在的index
                        if (valueIndex >= lstArg.Count)
                        {// 没有值了
                            throw new CommandLineParseException($"Option '{lstArg[optionIndex]}' has no value!");
                        }

                        consumeCount = 2; // option + value = 2
                        string rawValue = lstArg[valueIndex];

                        // 处理原始值，获得最终值
                        option.value = HandleSettedValue(option, rawValue);
                    }

                    // 消耗arg
                    lstArg.RemoveRange(optionIndex, consumeCount);
                }
                else
                {// 没有设置option

                    if (option.required)
                    {// 未设置必选option
                        throw new CommandLineParseException($"Lack of required option '{option.template}'!");
                    }

                    if (option.optionType == OptionType.SingleValue && option.defaultValue != null)
                    {// 默认值
                        var value = option.defaultValue();
                        CheckValue(option, value);
                        option.value = value;
                    }
                }
            }

            // 再处理argument
            foreach (var argument in m_arguments)
            {
                if (argument.mutiValue)
                {// 变长参数

                    // 创建对应类型的数组，大小要吃光所有arg
                    var values = Array.CreateInstance(argument.valueType, lstArg.Count);
                    for (int i = 0; i < lstArg.Count; ++i)
                    {
                        values.SetValue(HandleSettedValue(argument, lstArg[i]), i);
                    }

                    // value和values都是这个数组
                    argument.value = values;
                    argument.values = values;

                    // 消耗光arg
                    lstArg.Clear();
                }
                else
                {
                    if (lstArg.Count >= 1)
                    {// 还有值

                        string rawValue = lstArg[0];
                        argument.value = HandleSettedValue(argument, rawValue);

                        // 消耗arg
                        lstArg.RemoveAt(0);
                    }
                    else
                    {// 没有值了

                        if (argument.defaultValue == null)
                            throw new CommandLineParseException($"{argument.displayName} is required!");

                        var value = argument.defaultValue();
                        CheckValue(argument, value);

                        argument.value = value;
                    }
                }
            }

            // 执行本命令逻辑
            var retValue = m_onExecute();

            // 如果还有剩余参数，用第一个参数匹配执行子命令
            if (lstArg.Count > 0 && m_subCommands.TryGetValue(lstArg[0], out var subCommand))
            {// 调用子命令

                // 消耗arg
                lstArg.RemoveAt(0);

                // 使用子命令的返回值
                retValue = subCommand.ExecuteImpl(lstArg);
            }
            return retValue;
        }

        private object HandleSettedValue(Param param, string rawValue)
        {
            var typeHandle = CLTypeRegistration.Get(param.valueType);
            Debug.Assert(typeHandle != null);

            string error = string.Empty;
            if (!typeHandle.IsValueQualified(param.valueType, rawValue, ref error))
            {
                throw new CommandLineParseException($"Parsing {param.displayName} failed, detail as follows:\n{error}");
            }
            var value = typeHandle.GetValue(param.valueType, rawValue);

            CheckValue(param, value);
            return value;
        }

        private void CheckValue(Param param, object value)
        {
            if (value == null)
            {
                if (param.valueType.IsValueType)
                    throw new CommandLineParseException($"The value type of {param.displayName} is {param.valueType.Name}, can't be null!");
            }
            else
            {
                if (value.GetType() != param.valueType)
                {// 类型不匹配
                    throw new CommandLineParseException($"The value type of {param.displayName} is mismatched, need type is '{param.valueType}', receive type is '{value.GetType()}'!");
                }
            }
            
            // 额外的值检查
            if (param.valueChecker != null)
            {
                var error = param.valueChecker(value);
                if (error != null)
                    throw new CommandLineParseException($"Checking {param.displayName} failed: {error}!");
            }
        }
    }
}
