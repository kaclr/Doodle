using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Doodle;
using Doodle.CommandLineUtils;
using Newtonsoft.Json;
using NssIntegration;

namespace Test
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Logger.verbosity = Verbosity.Verbose;

            FLockUtil.Init(() => "/Users/harveyjin/Work/nssclient/Tools/BuildTools/ThirdParty/flock");

            Console.WriteLine($"main process {Process.GetCurrentProcess().Id}");

            // 使用flock给文件加锁
            var flock = FLockUtil.NewFLock("lock");
            bool success = false;
            if (args[0] == "s")
            {
                Console.WriteLine($"flock share");
                flock.AcquireShareLock();
            }
            else
            {
                Console.WriteLine($"flock exclusive");
                flock.AcquireExclusiveLock();
            }

            Console.WriteLine(success ? "success" : "failed");

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
