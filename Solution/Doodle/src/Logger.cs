using System;

namespace Doodle
{
    public static class Logger
    {
        public static void Log(object message)
        {
            Console.WriteLine(message);
        }

        public static void VerboseLog(object message)
        {
            Console.WriteLine(message);
        }
    }
}
