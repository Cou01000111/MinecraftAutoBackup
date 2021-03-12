using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MABProcessAtWait {
    class Logger {
        public static string logPath = ".\\logs\\MABProcess.txt";
        private static int outputLevel = 2;

        private static void Base(string logLevelStr, string message) {
            string logMessage = $"{DateTime.Now.ToString($"yyyy/MM/dd-HH:mm:ss")} [{logLevelStr}]:{message}\n";
            Console.WriteLine(logMessage);
            File.AppendAllText(logPath, logMessage);
        }
        public static void Debug(string message) {
            if (outputLevel >= 3) {
                Base("DEBUG", message);
            }
        }
        public static void Info(string message) {
            if (outputLevel >= 2) {
                Base("INFO ", message);
            }
        }
        public static void Warn(string message) {
            if (outputLevel >= 1) {
                Base("WARN ", message);
            }
        }
        public static void Error(string message) {
            if (outputLevel >= 0) {
                Base("ERROR", message);
            }
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
