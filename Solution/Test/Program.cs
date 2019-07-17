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

            Console.WriteLine($"Parent process {Process.GetCurrentProcess().Id}");

            var childProcess = new Process();
            childProcess.StartInfo.FileName = "dotnet";
            childProcess.StartInfo.Arguments = "C:\\Work\\Doodle\\Solution\\Test2\\bin\\Debug\\netcoreapp2.0\\Test2.dll";
            childProcess.StartInfo.CreateNoWindow = true;

            Console.WriteLine($"start child process");
            childProcess.Start();
            childProcess.WaitForExit();
        }
    }
}
