using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Doodle
{
    public class Script : IExecutable
    {
        private readonly string m_scriptPath;
        private readonly IExecutable m_interpreter;

        public Script(string script)
        {
            if (DoodleEnv.curPlatform == Platform.Windows)
            {
                m_interpreter = new Executable("cmd");
            }
            else
            {
                m_interpreter = new Executable("sh");
            }

            m_scriptPath = GetScriptPath(script);
        }

        public string ExecuteOut(string arguments)
        {
            return m_interpreter.ExecuteOut(GetFinalArguments(arguments));
        }

        public string ExecuteErr(string arguments)
        {
            return m_interpreter.ExecuteErr(GetFinalArguments(arguments));
        }

        public void Execute(string arguments)
        {
            ExecuteOut(arguments);
        }

        public void Execute(string arguments, out string stdout, out string stderr)
        {
            m_interpreter.Execute(GetFinalArguments(arguments), out stdout, out stderr);
        }

        public int ExecuteNoThrow(string arguments)
        {
            return m_interpreter.ExecuteNoThrow(GetFinalArguments(arguments));
        }

        public int ExecuteNoThrow(string arguments, out string stdout)
        {
            return m_interpreter.ExecuteNoThrow(GetFinalArguments(arguments), out stdout);
        }

        public int ExecuteNoThrow(string arguments, out string stdout, out string stderr)
        {
            return m_interpreter.ExecuteNoThrow(GetFinalArguments(arguments), out stdout, out stderr);
        }

        private string GetFinalArguments(string arguments)
        {
            if (DoodleEnv.curPlatform == Platform.Windows)
            {
                var finalArguments = $"/c \"{m_scriptPath}";
                if (!string.IsNullOrEmpty(arguments))
                {
                    finalArguments += $" {arguments}";
                }
                finalArguments += "\"";
                return finalArguments;
            }
            else
            {
                var finalArguments = $"\"{m_scriptPath}";
                if (!string.IsNullOrEmpty(arguments))
                {
                    finalArguments += $" {arguments}";
                }
                finalArguments += "\"";
                return finalArguments;
            }
        }

        private string GetScriptPath(string script)
        {
            if (File.Exists(script))
            {
                return script;
            }

            string scriptProcessed = null;
            if (DoodleEnv.curPlatform == Platform.Windows)
            {
                scriptProcessed = script + ".bat";
            }
            else
            {
                scriptProcessed = script + ".sh";
            }

            if (File.Exists(scriptProcessed))
            {
                return scriptProcessed;
            }

            throw new DoodleException($"Can not find script '{script}' or '{scriptProcessed}'!");
        }

    }
}
