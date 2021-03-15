using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class Util {
    public static Task Task;

    private static Font fontStyle;

    private static Logger logger = new Logger("MainForm");

    public static Font FontStyle {
        set { fontStyle = value; }
        get { return fontStyle; }
    }

    public static string TrimDoubleQuotationMarks(string target) {
        return target.Trim(new char[] { '"' });
    }

    public static string MakePathToWorld(string name, string dir) {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{dir}\\saves\\{name}";
    }

    //現在存在しているバックアップへのパスをListにして返す
    public static List<string> GetBackups() {
        logger.Info("call: GetBackups");
        List<string> backups = new List<string>();
        List<string> dirs;
        try { Directory.GetDirectories(AppConfig.BackupPath); }
        catch (DirectoryNotFoundException) { Directory.CreateDirectory(AppConfig.BackupPath); }
        finally { dirs = Directory.GetDirectories(AppConfig.BackupPath).ToList(); };
        List<string> worlds = new List<string>();
        foreach (string dir in dirs) {
            worlds.AddRange(Directory.GetDirectories(dir));
        }
        foreach (var w in worlds) {
            backups.AddRange(Directory.GetDirectories(w));
            backups.AddRange(Directory.GetFiles(w));
        }
        logger.Info($"dir:{dirs.Count()}, worlds:{worlds.Count()}, backups:{backups.Count()}");
        logger.Debug("-----GetBackups-----");
        foreach (var a in backups) {
            logger.Debug(a);
        }
        logger.Debug("--------------------");
        return backups;
    }

    //渡されたworldの現在存在するバックアップをListにして返す
    public static List<string> GetBackup(World w) {
        List<string> backups = new List<string>();
        if (Directory.Exists($"{AppConfig.BackupPath}\\{w.WorldDir}\\{w.WorldName}")) {
            //バックアップフォルダがある場合のみ実行
            backups.AddRange(Directory.GetDirectories($"{AppConfig.BackupPath}\\{w.WorldDir}\\{w.WorldName}"));
            backups.AddRange(Directory.GetFiles($"{AppConfig.BackupPath}\\{w.WorldDir}\\{w.WorldName}"));
        }
        return backups;
    }

}
