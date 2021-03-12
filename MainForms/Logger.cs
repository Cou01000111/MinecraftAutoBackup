using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Logger {
    //Logger logger = new Logger("Logger",".\\logs\\Logger.txt",3);
    private string logPath;
    private int outputLevel;
    private string subProcessName;

    public Logger(string subProcessName,string logPath,int outputLevel) {
        this.subProcessName = subProcessName;
        this.logPath = logPath;
        this.outputLevel = outputLevel;
    }

    public Logger(string subProcessName,int outputLevel) {
        this.subProcessName = subProcessName;
        this.logPath = $".\\logs\\{subProcessName}.txt";
        this.outputLevel = outputLevel;
    }

    public Logger(string subProcessName) {
        this.subProcessName = subProcessName;
        this.logPath = $".\\logs\\{subProcessName}.txt";
        this.outputLevel = 3;
    }

    private void Base(string logLevelStr, string message) {
        LogRotate();
        string logMessage = $"{DateTime.Now.ToString($"yyyy/MM/dd-HH:mm:ss")} [{logLevelStr}]:[{subProcessName}]{message}\n";
        Console.WriteLine(logMessage);
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
        logs.Reverse();
        return logs;
    }

    public string GetNearestLogFromFile() {
        return GetLogFromFile()[GetLogFromFile().Count - 2];
    }

    private void LogRotate() {
        List<string> logs = GetLogFromFile();
        if (logs.Count >= 1000) {
            Console.WriteLine($"LogRotate : { logs[logs.Count - 1]} , {logs[logs.Count - 2]}を削除します");
            logs.Remove(logs[logs.Count - 1]);
            logs.Remove(logs[logs.Count - 2]);
        }
        logs.Reverse();
        File.WriteAllText(logPath,string.Join("\n",logs.ToArray()));
    }
}