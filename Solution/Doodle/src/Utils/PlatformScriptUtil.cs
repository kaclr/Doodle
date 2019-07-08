using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public static class PlatformScriptUtil
    {
        private static readonly Executable s_bin;

        static PlatformScriptUtil()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Logger.VerboseLog("PlatformScriptUtil use cmd");
                s_bin = new Executable("cmd");
            }
            else
            {
                Logger.VerboseLog("PlatformScriptUtil use sh");
                s_bin = new Executable("sh");
            }
        }
    }
}
