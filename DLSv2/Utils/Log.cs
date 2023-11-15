using Rage;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace DLSv2.Utils
{
    class Log
    {
        public Log()
        {
            string message = "DLS - Dynamic Siren System v" + Assembly.GetExecutingAssembly().GetName().Version;
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            string path = @"Plugins/DLS.log";
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }
    }

    internal static class LogExtensions
    {
        internal static void ToLog(this string log, bool toConsole = false)
        {
            if (toConsole) Game.Console.Print(log);
            string path = @"Plugins/DLS.log";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine("[" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + "] " + log);
                writer.Close();
            }
        }
    }
}
