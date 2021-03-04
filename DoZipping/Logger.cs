using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zipper {
    class Logger {
        public static string logPath = ".\\logs\\Zipper.txt";
        private static int outputLevel = 3;

        private static void Base(string logLevelStr, string message, int level) {
            if (level <= outputLevel) {
                string logMessage = $"{DateTime.Now.ToString($"yyyy/MM/dd-HH:mm:ss")} [{logLevelStr}]:{message}\n";
                Console.WriteLine(logMessage);
                File.AppendAllText(logPath, logMessage);
            }
        }
        public static void Debug(string message) {
            Base("DEBUG", message, 3);
        }
        public static void Info(string message) {
            Base("INFO ", message, 2);
        }
        public static void Warn(string message) {
            Base("WARN ", message, 1);
        }
        public static void Error(string message) {
            Base("ERROR", message, 0);
        }

        public static List<string> GetLogFromFile() {
            List<string> logs = new List<string>();

            using (StreamReader s = new StreamReader(Logger.logPath)) {
                string _logs = s.ReadToEnd();
                logs = _logs.Split('\n').ToList();
            }
            logs.Reverse();
            return logs;
        }

        public static string GetNearestLogFromFile() {
            return GetLogFromFile()[GetLogFromFile().Count - 2];
        }
    }
}
