using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public static class MethodCommand
    {
        public static Command New(MethodInfo method, Func<object> getInstance = null)
        {
            var methodParameters = method.GetParameters();
            var command = new Command(method.Name);
            var commandParams = new Param[methodParameters.Length];
            bool setOption = false;
            
            for (int i = 0; i < methodParameters.Length; ++i)
            {
                var parameterInfo = methodParameters[i];
                var parameterConfiguration = parameterInfo.GetCustomAttribute<ParameterConfigurationAttribute>();
                Param commandParam = null;

                if (parameterConfiguration != null && parameterConfiguration.optionTemplate != null)
                {// option
                    setOption = true;

                    var option = new Option(parameterConfiguration.optionTemplate, parameterConfiguration.description, OptionType.SingleValue)
                    {
                        required = parameterConfiguration.required
                    };

                    if (parameterInfo.ParameterType == typeof(bool))
                    {
                        if (parameterInfo.DefaultValue != DBNull.Value)
                        {// bool值不能有默认值，因为要用开关的2个状态来表示
                            throw new CommandLineException($"New MethodCommand '{method.Name}' failed, parameter '{parameterInfo.Name}' is a bool option, can not have any default value!");
                        }

                        option.optionType = OptionType.NoValue;
                    }
                    else
                    {
                        if (parameterInfo.DefaultValue == DBNull.Value)
                        {// 无参数默认值

                            if (!parameterConfiguration.required)
                            {// 非required option必须要有一个默认值
                                throw new CommandLineException($"New MethodCommand '{method.Name}' failed, parameter '{parameterInfo.Name}' is a none required option, must has default value!");
                            }
                        }
                        else
                        {
                            // 使用函数定义中的默认值
                            option.defaultValue = () => parameterInfo.DefaultValue;
                        }
                    }

                    commandParam = option;

                }
                else
                {// argument
                    if (setOption)
                    {
                        throw new CommandLineException($"New MethodCommand '{method.Name}' failed, option must at the end of the parameters!");
                    }

                    commandParam = new Argument(parameterInfo.Name, "", false);
                    if (parameterConfiguration != null)
                    {
                        commandParam.description = parameterConfiguration.description;
                    }
 
                    if (commandParam.defaultValue == null && parameterInfo.DefaultValue != DBNull.Value)
                    {// 用函数定义上的默认值
                        commandParam.defaultValue = () => parameterInfo.DefaultValue;
                    }
                }

                commandParam.valueType = parameterInfo.ParameterType;
                command.AddParam(commandParam);

                commandParams[i] = commandParam;
            }

            command.OnExecute(() =>
            {
                object[] parameters = new object[method.GetParameters().Length];
                for (int i = 0; i < commandParams.Length; ++i)
                {
                    var commandParam = commandParams[i];
                    object value = commandParam.value;
                    if (commandParam.valueType == typeof(bool) && commandParam is Option)
                    {// 用set状态表示bool的值
                        value = (commandParam as Option).isSet;
                    }

                    parameters[i] = value;
                }

                object obj = null;
                if (getInstance != null)
                    obj = getInstance();

                var ret = method.Invoke(obj, parameters);
                if (method.ReturnType == typeof(int))
                    return (int)ret;
                else
                // 其他返回值类型都返回0
                    return 0;
            });

            return command;
        }
    }
}
