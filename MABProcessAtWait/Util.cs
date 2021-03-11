using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

/*
 フォルダ構成
MinecraftAutoBackup.exe
Configs
    config.txt
    BackupPath.txt
SubModules
    MABProcess.exe
image
    app.ico
 */

namespace MABProcessAtWait {
    public static class Util {
        public static bool IsZipperRunning() {
            List<Process> p = Process.GetProcesses().ToList();
            Logger.Info($"return : {p.Select(x => x.ProcessName).Contains("Zipper.exe")}");
            return p.Select(x => x.ProcessName).Contains("Zipper.exe");
        }

        public static string TrimDoubleQuotationMarks(string target) {
            return target.Trim(new char[] { '"' });
        }

        public static void NotReadonly(string path) {
            Logger.Debug("call:NotReadonly");
            Logger.Info($"{path}を入力されました");
            List<string> pasess = Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories).ToList();
            foreach(string p in pasess) {
                if((File.GetAttributes(p) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                    Logger.Info($"{p} のreadonlyを外します");
                    File.SetAttributes(p, File.GetAttributes(p) & ~FileAttributes.ReadOnly);
                }
            }
        }
    }
}
