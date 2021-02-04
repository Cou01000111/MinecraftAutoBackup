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
    public static string backupPath { get; set; }
    public static Font font { get; set; }
    public static bool doZip { get; set; }
    public static string language { get; set; }
    public static Size clientSize { get; set; }
    public static Point clientPoint { get; set; }
    public static string appConfigPath = ".\\Config\\AppConfig.txt";

    public AppConfig() {
        if (!File.Exists(appConfigPath)) {
            //AppConfigファイルがなかった場合
            string Text =
                $"{System.Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\MinecraftAutoBackup\nMeiryo UI\nnormal\nja\n600\n600\n0\n0";
            File.WriteAllText(appConfigPath, Text);
        }
        List<string> datas = new List<string>();
        using (StreamReader reader = new StreamReader(appConfigPath, Encoding.GetEncoding("utf-8"))) {
            while (reader.Peek() >= 0) {
                datas.Add(reader.ReadLine());
            }
            backupPath = datas[0];
            font = new Font(datas[1], 11);
            doZip = (datas[2] == "zip") ? true : false;
            language = datas[3];
            clientSize = new Size(int.Parse(datas[4]), int.Parse(datas[5]));
            clientPoint = new Point(int.Parse(datas[6]), int.Parse(datas[7]));
        }
        Console.WriteLine("-----loaded appConfig-----");
        Console.WriteLine($"backupPath:{backupPath}");
        Console.WriteLine($"font:{font}");
        Console.WriteLine($"dozip:{doZip}");
        Console.WriteLine($"clientSize:{clientSize.Width},{clientSize.Height}");
        Console.WriteLine($"clientPoint:{clientPoint.X},{clientPoint.Y}");
        Console.WriteLine("--------------------------");
    }

    public static void WriteAppConfig() {
        string Text =
            $"{backupPath}\n{font.Name}\n" +
            (doZip ? "zip" : "normal") +
            $"\n{language}\n{clientSize.Width}\n{clientSize.Height}\n{clientPoint.X}\n{clientPoint.Y}";
        File.WriteAllText(appConfigPath, Text);
    }

}
