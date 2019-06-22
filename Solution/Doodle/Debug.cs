using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    public static class Debug
    {
        public static void Assert(bool condition, string message = null)
        {
            if (!condition)
            {
                throw new AssertException(message);
            }
        }
    }
}
