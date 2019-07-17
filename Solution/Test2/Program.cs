using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Test2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Child process {Process.GetCurrentProcess().Id} {args[0]}");


            FileStream fs = new FileStream(new SafeFileHandle((IntPtr)int.Parse(args[0]), false), FileAccess.ReadWrite);
            using (StreamWriter f = new StreamWriter(fs, Encoding.UTF8))
            {
                f.WriteLine("child process writed!");
            }
        }
    }
}
