using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    /// <summary>
    /// 实现异常，一般是实现上的bug导致的异常。
    /// </summary>
    public class ImplException : Exception
    {
        public ImplException()
        {
        }

        public ImplException(string message)
            : base(message)
        {
        }
    }
}
