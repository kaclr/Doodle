using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NssIntegration
{
    public static class NssHelper
    {
        public static string GetStandardIFSName(BuildTarget buildTarget, string version)
        {
            return $"{buildTarget}_{version}.ifs";
        }

        public static string GetStandardAppName(
            BuildTarget buildTarget,
            BuildMode buildMode,
            VerLine verLine,
            string defaultTDir,
            string version,
            int svnRev)
        {
            var name = $"NSS_{buildMode}_{defaultTDir}_{verLine}_{version}_{svnRev}";
            if (buildTarget == BuildTarget.Android)
            {
                return $"{name}.apk";
            }
            else if (buildTarget == BuildTarget.iOS)
            {
                return $"{name}.ipa";
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
