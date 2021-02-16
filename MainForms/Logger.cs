using System;
using System.IO;

static class Logger {
    public static string logPath = ".\\logs\\MainProcess.txt";
    private static int outputLevel = 2;

    public static void Base(int level, string message) {
        string logLevelStr;
        switch (level) {
            case 3:
                logLevelStr = "DEBUG";
                break;
            case 2:
                logLevelStr = "INFO ";
                break;
            case 1:
                logLevelStr = "WARN ";
                break;
            case 0:
                logLevelStr = "ERROR";
                break;
            default:
                logLevelStr = "NONE";
                break;
        }
        if (level <= outputLevel) {
            string logMessage = $"{DateTime.Now.ToString($"yyyy/MM/dd-HH:mm:ss")} [{logLevelStr}]:{message}\n";
            Console.WriteLine(logMessage);
            File.AppendAllText(logPath, logMessage);
        }
    }
    public static void Debug(string message) {
        Base(3, message);
    }

    public static void Debug(int message){
        Base(3,$"(Int){message}");
    }

    public static void Debug(bool message){
        Base(3,$"(Boolean){message}");
    }

    public static void Info(string message) {
        Base(2, message);
    }

    public static void Info(int message){
        Base(2,$"(Int){message}");
    }

    public static void Info(bool message){
        Base(2,$"(Boolean){message}");
    }

    public static void Warn(string message) {
        Base(1, message);
    }

    public static void Warn(int message){
        Base(1,$"(Int){message}");
    }

    public static void Warn(bool message){
        Base(1,$"(Boolean){message}");
    }

    public static void Error(string message) {
        Base(0, message);
    }

    public static void Error(int message){
        Base(0,$"(Int){message}");
    }

    public static void Error(bool message){
        Base(0,$"(Boolean){message}");
    }
}
