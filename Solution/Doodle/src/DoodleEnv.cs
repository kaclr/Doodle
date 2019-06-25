using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public enum Platform
    {
        Windows,
        OSX,
    }

    public static class DoodleEnv
    {
        public static Platform curPlatform
        {
            get;
            private set;
        }

        static DoodleEnv()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                curPlatform = Platform.Windows;
            }
            else
            {
                curPlatform = Platform.OSX;
            }
        }
    }
}
