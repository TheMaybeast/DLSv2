using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Rage;

namespace DLSv2.Utils;

internal enum LogLevel
{
    DEBUG, // Only prints if DEBUG flag is enabled
    INFO, // Prints to log file only
    DEVMODE, // Prints to console aswell
    ERROR, // Prints to console aswell
    FATAL // Prints to console aswell
}

internal static class Log
{
    private const string Path = "Plugins\\DLS.log";

    static Log()
    {
        var message = $"DLS - Dynamic Lighting System v{Assembly.GetExecutingAssembly().GetName().Version}";
        message += Environment.NewLine;
        message += "-----------------------------------------------------------";
        message += Environment.NewLine;

        using var writer = new StreamWriter(Path, false);
        writer.WriteLine(message);
        writer.Close();
    }

    public static void Write(string text, LogLevel logLevel = LogLevel.INFO)
    {
#if !DEBUG
        if (logLevel == LogLevel.DEBUG) return;
#endif
        if (logLevel > LogLevel.INFO) Game.LogTrivial($"[{logLevel}] {text}");

        using var writer = new StreamWriter(Path, true);
        writer.WriteLine($"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] [{logLevel}] {text}");
        writer.Close();
    }

    internal static void Terminate()
    {
        if (!Directory.Exists($"Logs"))
            Directory.CreateDirectory($"Logs");
        if (!Directory.Exists($"Logs\\DLS"))
            Directory.CreateDirectory($"Logs\\DLS");

        File.Copy(Path, $"Logs\\DLS\\DLS_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.log");
    }
}

internal static class LogExtensions
{
    public static void ToLog(this string message, LogLevel logLevel = LogLevel.INFO) =>
        Log.Write(message, logLevel);
}