using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Collections.Generic;

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
    public static Font Font { get; set; }
    public static bool DoZip { get; set; }
    public static string Language { get; set; }
    public static Size ClientSize { get; set; }
    public static Point ClientPoint { get; set; }
    public static string BackupCount { get; set; }
    public static List<string> AddGameDirPath { get; set; }
    public static string AppConfigPath = ".\\Config\\AppConfig.txt";

    private static Logger logger = new Logger("AppConfig",".\\logs\\MainForm.txt",3);

    public AppConfig() {
        AddGameDirPath = new List<string>();
        if (!File.Exists(AppConfigPath)) {
            //AppConfigファイルがなかった場合
            string Text =
                $"{System.Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\MinecraftAutoBackup\nMeiryo UI\nnormal\nja\n600\n600\n0\n0\n5";
            File.WriteAllText(AppConfigPath, Text);
        }
        List<string> datas = new List<string>();
        using (StreamReader reader = new StreamReader(AppConfigPath, Encoding.GetEncoding("utf-8"))) {
            while (reader.Peek() >= 0) {
                datas.Add(reader.ReadLine());
            }
            BackupPath = datas[0];
            Font = new Font(datas[1], 11);
            DoZip = (datas[2] == "zip") ? true : false;
            Language = datas[3];
            ClientSize = new Size(int.Parse(datas[4]), int.Parse(datas[5]));
            ClientPoint = new Point(int.Parse(datas[6]), int.Parse(datas[7]));
            BackupCount = datas[8];
            for (int i = 9; i < datas.Count; i++) {
                AddGameDirPath.Add(datas[i]);
            }
        }
        logger.Info("-----loaded appConfig-----");
        logger.Info($"backupPath:{BackupPath}");
        logger.Info($"font:{Font}");
        logger.Info($"dozip:{DoZip}");
        logger.Info($"clientSize:{ClientSize.Width},{ClientSize.Height}");
        logger.Info($"clientPoint:{ClientPoint.X},{ClientPoint.Y}");
        logger.Info($"backupCount:{BackupCount}");
        foreach(string path in AddGameDirPath) {
            logger.Info($"addGameDirPath:{path}");
        }
        logger.Info("--------------------------");
    }

    public static void WriteAppConfig() {
        string Text =
            $"{BackupPath}\n{Font.Name}\n" +
            (DoZip ? "zip" : "normal") +
            $"\n{Language}\n{ClientSize.Width}\n{ClientSize.Height}\n{ClientPoint.X}\n{ClientPoint.Y}\n{BackupCount}";
        foreach(string path in AddGameDirPath) {
            Text += $"\n{path}";
        }
        logger.Info(" Writing "+string.Join(" , ",Text.Split('\n')));
        File.WriteAllText(AppConfigPath, Text);
    }

}
