using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Zipper;

public class AppConfig {
    /*
    アプリ自体の設定を保存するtxtファイル
    バックアップを保存するpath
    フォント名
    フォント大きさ
    保存形式(zip,normal)
    言語(jp,en)

    */
    public static string BackupPath { get; set; }
   // public static Font Font { get; set; }
    public static bool DoZip { get; set; }
    public static string Language { get; set; }
    //public static Size ClientSize { get; set; }
    //public static Point ClientPoint { get; set; }
    public static string BackupCount { get; set; }
    public static List<string> AddGameDirPath { get; set; }
    public static string appConfigPath = ".\\Config\\AppConfig.txt";

    public AppConfig() {
        AddGameDirPath = new List<string>();
        if (!File.Exists(appConfigPath)) {
            //AppConfigファイルがなかった場合
            string Text =
                $"{System.Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\MinecraftAutoBackup\nMeiryo UI\nnormal\nja\n600\n600\n0\n0\n5";
            File.WriteAllText(appConfigPath, Text);
        }
        List<string> datas = new List<string>();
        using (StreamReader reader = new StreamReader(appConfigPath, Encoding.GetEncoding("utf-8"))) {
            while (reader.Peek() >= 0) {
                datas.Add(reader.ReadLine());
            }
            BackupPath = datas[0];
            //Font = new Font(datas[1], 11);
            DoZip = (datas[2] == "zip") ? true : false;
            Language = datas[3];
            //ClientSize = new Size(int.Parse(datas[4]), int.Parse(datas[5]));
            //ClientPoint = new Point(int.Parse(datas[6]), int.Parse(datas[7]));
            BackupCount = datas[8];
            for (int i = 9; i < datas.Count; i++) {
                AddGameDirPath.Add(datas[i]);
            }
        }
        Logger.Info("-----loaded appConfig-----");
        Logger.Info($"backupPath:{BackupPath}");
        //Logger.Info($"font:{Font}");
        Logger.Info($"dozip:{DoZip}");
        //Logger.Info($"clientSize:{ClientSize.Width},{ClientSize.Height}");
        //Logger.Info($"clientPoint:{ClientPoint.X},{ClientPoint.Y}");
        Logger.Info($"backupCount:{BackupCount}");
        foreach (string path in AddGameDirPath) {
            Logger.Info($"addGameDirPath:{path}");
        }
        Logger.Info("--------------------------");
    }

}
