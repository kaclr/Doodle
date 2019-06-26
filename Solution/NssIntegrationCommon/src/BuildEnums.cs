using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NssIntegrationCommon
{
    public enum BuildMode
    {
        Debug,
        Publish,
    }

    public enum PackageType
    {
        Normal,
        Experience
    }

    public enum VerLine
    {
        DB,
        PlayerGroup,
        Experience,
        OB,
        Predownload,
    }

    public enum BuildTarget
    {
        Android,
        iOS,
        Windows,
    }

    [Flags]
    public enum BuildOption
    {
        None = 0,
        AI = 1,
    }
}
