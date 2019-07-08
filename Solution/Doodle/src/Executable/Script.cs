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
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                m_interpreter = new Executable("cmd");
            }
            else
            {
                m_interpreter = new Executable("sh");
            }

            m_scriptPath = GetScriptPath(script);
        }

        public string Execute(string arguments)
        {
            return m_interpreter.Execute(GetFinalArguments(arguments));
        }

        private string GetFinalArguments(string arguments)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return $"/c \"{m_scriptPath}\" {arguments}";
            }
            else
            {
                return $"\"{m_scriptPath}\" {arguments}";
            }
        }

        private string GetScriptPath(string script)
        {
            if (File.Exists(script))
            {
                return script;
            }

            string scriptProcessed = null;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
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
