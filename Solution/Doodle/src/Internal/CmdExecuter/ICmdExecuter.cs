using System;
using System.Collections.Generic;
using System.Text;

namespace Doodle
{
    internal interface ICmdExecuter
    {
        /// <summary>
        /// 执行命令，失败抛出异常
        /// </summary>
        /// <param name="cmd"> 命令 </param>
        /// <returns> 标准输出 </returns>
        string ExecuteCmd(string cmd);

        /// <summary>
        /// 执行命令，失败返回错误码
        /// </summary>
        /// <param name="cmd"> 命令 </param>
        /// <param name="stderr"> 标准错误输出 </param>
        /// <returns></returns>
        int ExecuteCmd(string cmd, out string stderr);
        
        /// <summary>
        /// 执行命令，失败返回错误码
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="stdout"> 标准输出 </param>
        /// <param name="stderr"> 标准错误输出 </param>
        /// <returns></returns>
        int ExecuteCmdOE(string cmd, out string stdout, out string stderr);

        /// <summary>
        /// 是否存在指定可执行文件
        /// </summary>
        /// <param name="execPath"> 文件 </param>
        /// <returns></returns>
        bool ExistsExec(string execPath);
    }
}
