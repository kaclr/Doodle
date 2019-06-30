using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NssIntegrationCommon
{
    public static class DatabinUtil
    {
        public static void RequestDatabinBase(string toolsDir, string outputPath)
        {
            if (!NssPath.IsToolsDir(toolsDir, out string error)) throw new ArgumentException(error, nameof(toolsDir));
            if (string.IsNullOrEmpty(outputPath)) throw new ArgumentException($"'{nameof(outputPath)}' is empty!", nameof(outputPath));
        }
    }
}
