//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;

//namespace Doodle
//{
//    public class Script : IExecutable
//    {
//        private static readonly Executable s_bin;
//        private static readonly string s_scriptDir;

//        static Script()
//        {
//            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
//            {
//                Logger.VerboseLog("PlatformScriptUtil use cmd");

//                s_bin = new Executable("cmd");
//                s_scriptDir = SpaceUtil.GetTempPath("TempBat");
//            }
//            else
//            {
//                Logger.VerboseLog("PlatformScriptUtil use sh");

//                s_bin = new Executable("sh");
//                s_scriptDir = SpaceUtil.GetTempPath("TempSh");
//            }

//            DirUtil.TryCreateDir(s_scriptDir);
//        }

//        public string Execute(string arguments)
//        {
//            throw new NotImplementedException();
//        }


//        public static void Execute(string script, string arguments)
//        {
//            var scriptPath = GetScriptPath(script);
//            s_bin.Execute(GetFinalArguments(script, arguments));
//        }

//        private static string GetFinalArguments(string scriptPath, string arguments)
//        {
//            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
//            {
//                return $"/c \"{scriptPath}\" {arguments}";
//            }
//            else
//            {
//                return $"\"{scriptPath}\" {arguments}";
//            }
//        }

//        private static string GetScriptPath(string script)
//        {
//            if (File.Exists(script))
//            {
//                return script;
//            }

//            string scriptProcessed = null;
//            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
//            {
//                scriptProcessed = script + ".bat";
//            }
//            else
//            {
//                scriptProcessed = script + ".sh";
//            }

//            if (File.Exists(script))
//            {
//                return script;
//            }

//            throw new DoodleException($"Can not find script '{script}' or '{scriptProcessed}'!");
//        }

//    }
//}
