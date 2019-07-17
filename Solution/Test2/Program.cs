using System;
using System.Diagnostics;

namespace Test2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Child process {Process.GetCurrentProcess().Id}");
        }
    }
}
