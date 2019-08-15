using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Doodle
{
    interface ILog
    {
        Verbosity verbosity { get; set; }

        void VerboseLog(object message);

        void Log(object message);

        void WarningLog(object message);

        void ErrorLog(object message);
    }
}
