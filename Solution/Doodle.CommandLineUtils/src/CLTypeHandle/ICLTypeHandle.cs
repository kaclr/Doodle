using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle.CommandLineUtils
{
    public interface ICLTypeHandle
    {
        object GetValue(Type type, string rawValue);

        // 取得受限的值数组，返回null代表没有受限
        string[] GetLimitValues(Type type);

        // 判读值是否合法，不合法要返回错误
        bool IsValueQualified(Type type, string rawValue, ref string error);
    }
}
