using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    interface ICLDisplayItem
    {
        string name { get; }
        string description { get; set; }
        string helpText { get; set; }
    }
}
