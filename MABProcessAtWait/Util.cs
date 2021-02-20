using System.Collections.Generic;
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
            //一回もzipperが起動されていない場合
            if (!File.Exists($".\\logs\\Zipper.txt")) {
                return false;
            }
            //zipperのlogからlog取得
            List<string> strs = new List<string>();
            using (StreamReader r = new StreamReader($".\\logs\\Zipper.txt")) {
                while (r.Peek() > -1) {
                    strs.Add(r.ReadLine());
                }
            }
            //最終行がExit Processかどうかを取得
            string decisionStr = strs[strs.Count() - 2].Substring(28, strs[strs.Count() - 2].Length);
            Logger.Info($"decisionStrは{decisionStr}です");
            return decisionStr != "Exit Process";
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
