using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zipper {
    class Logger {
        public static string logPath = ".\\logs\\Zipper.txt";
        private static int outputLevel = 3;

        private static void Base(int level, string message) {
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
        public static void Info(string message) {
            Base(2, message);
        }
        public static void Warn(string message) {
            Base(1, message);
        }
        public static void Error(string message) {
            Base(0, message);
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
