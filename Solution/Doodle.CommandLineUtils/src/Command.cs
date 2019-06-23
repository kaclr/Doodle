﻿using System;
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
                    // 未设置必选option
                        throw new CommandLineParseException($"Lack of required option '{option.template}'!");

                    if (option.optionType == OptionType.SingleValue && option.defaultValue != null)
                    {// 设置默认值
                        var value = option.defaultValue;
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

                    // 创建对应类型的数组，大小要吃光所有参数
                    var values = Array.CreateInstance(argument.typeConfiguration.type, lstArg.Count);
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

                        if (argument.defaultValue == null)
                            throw new CommandLineParseException($"Unset {argument.displayName} without defaultValue!");

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
            object value;
            try
            {
                value = param.typeConfiguration.Convert(rawValue);
            }
            catch (Exception e)
            {
                throw new CommandLineParseException($"{param.displayName} convert failed, value type is '{param.typeConfiguration.GetType().Name}', raw value is '{rawValue}', error detail:\n {e.Message}");
            }
            
            CheckValue(param, value);

            return value;
        }

        private void CheckValue(Param param, object value)
        {
            if (value.GetType() != param.typeConfiguration.type)
            {// 类型不匹配
                throw new CommandLineParseException($"The value type of {param.displayName} is mismatched, need type is '{param.typeConfiguration}', receive type is '{value.GetType()}'!");
            }

            var errMsg = param.typeConfiguration.Check(value);
            if (errMsg != null)
                throw new CommandLineParseException($"Checking {param.displayName} failed: {errMsg}!");
        }
    }
}
