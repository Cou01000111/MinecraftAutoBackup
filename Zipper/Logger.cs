using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zipper;

namespace Zipper {
    public class Logger {
        private const int DEFAULT_OUTPUT_LEVEL = 3;
        private const int MAX_LOG_COUNT = 500;
        private const string LOG_OUTPUT_PATH = ".\\logs";
        private string logPath;
        private int outputLevel;
        private string subProcessName;

        public Logger(string subProcessName, string logPath, int outputLevel) {
            this.subProcessName = subProcessName;
            this.logPath = logPath;
            this.outputLevel = outputLevel;
            LogRotate();
        }
        public Logger(string subProcessName, int outputLevel) {
            this.subProcessName = subProcessName;
            this.logPath = $".\\logs\\{subProcessName}.log";
            this.outputLevel = outputLevel;
            if (Directory.Exists(LOG_OUTPUT_PATH) == false) {
                Directory.CreateDirectory(LOG_OUTPUT_PATH);
            }
            LogRotate();
        }
        public Logger(string subProcessName, string logPath) {
            this.subProcessName = subProcessName;
            this.logPath = $".\\logs\\{subProcessName}.log";
            this.outputLevel = DEFAULT_OUTPUT_LEVEL;
            if (Directory.Exists(LOG_OUTPUT_PATH) == false) {
                Directory.CreateDirectory(LOG_OUTPUT_PATH);
            }
            LogRotate();
        }

        public Logger(string subProcessName) {
            this.subProcessName = subProcessName;
            this.logPath = $".\\logs\\{subProcessName}.log";
            this.outputLevel = DEFAULT_OUTPUT_LEVEL;
            if (Directory.Exists(LOG_OUTPUT_PATH) == false) {
                Directory.CreateDirectory(LOG_OUTPUT_PATH);
            }
            LogRotate();
        }

        private void Base(string logLevelStr, string message) {
            string logMessage = $"{DateTime.Now.ToString($"yyyy/MM/dd-HH:mm:ss")} [{logLevelStr}]:[{subProcessName}]{message}\n";
            Console.WriteLine(logMessage);
            Program.logs.Push(logMessage);
            File.AppendAllText(logPath, logMessage);
        }
        public void Debug(string message) {
            if (outputLevel >= 3) {
                Base("DEBUG", message);
            }
        }
        public void Info(string message) {
            if (outputLevel >= 2) {
                Base("INFO ", message);
            }
        }
        public void Warn(string message) {
            if (outputLevel >= 1) {
                Base("WARN ", message);
            }
        }
        public void Error(string message) {
            if (outputLevel >= 0) {
                Base("ERROR", message);
            }
        }
        public List<string> GetLogFromFile() {
            List<string> logs = new List<string>();
            try {
                using (StreamReader s = new StreamReader(this.logPath)) {
                    string _logs = s.ReadToEnd();
                    logs = _logs.Split('\n').ToList();
                }
            }
            catch (FileNotFoundException) {
                File.Create(this.logPath).Close();
            }
            //logs.Reverse();
            return logs;
        }

        private void LogRotate() {
            Debug("LogRotate");
            List<string> logs = GetLogFromFile();
            //log総量が1000行を超えている場合は100行削除
            while (logs.Count >= MAX_LOG_COUNT) {
                logs.RemoveAt(0);
            }
            //logs.Reverse();
            File.WriteAllText(logPath, string.Join("\n", logs.ToArray()));
        }
    }
}