using System;
using System.Linq;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Threading;
using System.IO.Compression;

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
    static class Program {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main() {

            // © DOBON!.
            //Mutex関係
            // https://dobon.net/vb/dotnet/process/checkprevinstance.html
            //Mutex名を決める（必ずアプリケーション固有の文字列に変更すること！）
            string mutexName = "MABProcess";
            //Mutexオブジェクトを作成する
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, mutexName);

            bool hasHandle = false;
            try {
                try {
                    hasHandle = mutex.WaitOne(0, false);
                }
                //.NET Framework 2.0以降の場合
                catch (System.Threading.AbandonedMutexException) {
                    hasHandle = true;
                }
                if (hasHandle == false) {
                    return;
                }
                new AppConfig();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Form1 f = new Form1();
                Application.Run();
            }
            finally {
                if (hasHandle) {
                    mutex.ReleaseMutex();
                }
                mutex.Close();
            }
            
        }
    }

    public partial class Form1 :Form {
        System.Windows.Forms.Timer timer;
        string backupDataPath;
        NotifyIcon notifyIcon;

        public Form1() {
            backupDataPath = AppConfig.backupPath;
            this.ShowInTaskbar = false;
            this.Icon = new Icon(".\\Image\\app.ico");
            this.FormClosing += new FormClosingEventHandler(Form1_Closing);

            timer = new System.Windows.Forms.Timer() {
                Enabled = true
            };
            timer.Interval = 2000;
            timer.Tick += new EventHandler(timer_Tick);

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(".\\Image\\app_sub.ico");
            notifyIcon.Visible = true;
            notifyIcon.Text = "MAB待機モジュール";
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem exit = new ToolStripMenuItem();
            exit.Text = "終了";
            exit.Click += new EventHandler(Close_Click);
            menu.Items.Add(exit);
            notifyIcon.ContextMenuStrip = menu;
        }

        void Close_Click(object sender, EventArgs e) {
            Console.WriteLine("info:アプリケーションが終了しました");
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        void Form1_Closing(object sender, EventArgs e) {
            Console.WriteLine("info:アプリケーションが強制終了しました");
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        void timer_Tick(object sender, EventArgs e) {
            if (Process.GetProcessesByName("MinecraftLauncher").Length > 0) {
                timer.Enabled = false;
                Console.WriteLine("Minecraft Lancherの起動を検知しました");
                notifyIcon.Icon = new Icon(".\\Image\\app_sub_doing.ico");
                int backupCount = 0;
                ContextMenuStrip menu = new ContextMenuStrip();
                ToolStripMenuItem exit = new ToolStripMenuItem() {
                    Text = "強制終了",
                };
                exit.Click += new EventHandler(Close_Click);
                menu.Items.Add(exit);
                notifyIcon.ContextMenuStrip = menu;

                List<string> worldPasses = GetWorldPasses();
                string nowTime = DateTime.Now.ToString("yyyyMMddHHmm");
                if(worldPasses.Count == 0) {
                    Console.WriteLine("info:どうやらバックアップ予定のデータはないようです");
                }
                notifyIcon.Text = $"{backupCount}/{worldPasses.Count}";
                foreach (string worldPath in worldPasses) {
                    notifyIcon.Text = $"{backupCount++}/{worldPasses.Count}";
                    doBackup(worldPath, nowTime);
                }
                Console.WriteLine("全バックアップが完了しました ");
                
                timer.Enabled = true;
                notifyIcon.Icon = new Icon(".\\Image\\app_sub.ico");
                notifyIcon.Text = "MAB待機モジュール";
            }
        }

        //バックアップをするワールドデータのパスを配列にして返す
        List<string> GetWorldPasses() {
            List<string> worldPasses = new List<string>();
            string filePath = @".\Config\config.txt";
            StreamReader reader;
            try {
                reader = new StreamReader(filePath, Encoding.GetEncoding("Shift_jis"));
            }
            catch (FileNotFoundException e) {
                Console.Error.WriteLine(e.FileName + "が見つけられません");
                Application.Exit();
                return worldPasses;
            }
            catch (DirectoryNotFoundException e) {
                Console.Error.WriteLine(e.Message);
                Application.Exit();
                return worldPasses;
            }
            Console.WriteLine("info:config.txtを発見しました");
            while (reader.Peek() >= 0) {
                string[] datas = reader.ReadLine().Split(',');
                //Console.WriteLine("datas:" + datas[0]);
                datas = datas.Select(x => Util.TrimDoubleQuotationMarks(x)).Cast<string>().ToArray();
                if (Convert.ToBoolean(datas[0])) {
                    worldPasses.Add(datas[2]);
                }
            }
            reader.Close();
            return worldPasses;
        }

        void doBackup(string path, string Time) {
            string backupPath = backupDataPath + "\\" + Path.GetFileName(Directory.GetParent(Directory.GetParent(path).ToString()).ToString()) + "\\" + Path.GetFileName(path) + "\\" + Time;
            if (AppConfig.doZip) {
                try {
                    Console.WriteLine(path + " を " + backupPath + ".zip へバックアップ中です");
                    ZipFile.CreateFromDirectory(path, $"{backupPath}.zip");
                    Console.WriteLine(path + " を " + backupPath + " .zipへバックアップしました");
                }
                catch (DirectoryNotFoundException e) {
                    Console.Error.WriteLine(path + ":DirectoryNotFoundException : " + e.Message);
                    DialogResult r = MessageBox.Show($"バックアップ予定のワールドデータ[{Path.GetFileName(path)}]が見つかりませんでした。バックアップ予定から削除しますか？","Minecraft Auto Backup",MessageBoxButtons.OKCancel);
                    if(r == DialogResult.OK) {
                        DialogResult _r = MessageBox.Show($"バックアップ予定のワールドデータ[{Path.GetFileName(path)}]が見つかりませんでした。バックアップ予定から削除しますか？", "Minecraft Auto Backup", MessageBoxButtons.YesNo);
                    }
                }
                catch (IOException e) {
                    Console.Error.WriteLine(backupPath + ":IOException : " + e.Message);
                }
            }
            else {
                try {
                    Console.WriteLine(path + " を " + backupPath + "へバックアップ中です");
                    FileSystem.CopyDirectory(path, backupPath);
                    Console.WriteLine(path + " を " + backupPath + "へバックアップしました");
                }
                catch (DirectoryNotFoundException e) {
                    Console.Error.WriteLine(path + ":DirectoryNotFoundException : " + e.Message);
                }
                catch (IOException e) {
                    Console.Error.WriteLine(backupPath + ":IOException : " + e.Message);
                }
            }
        }
    }

    public class BackupTimes {
        public string worldPath;
        public DateTime nextBackupTime;
        public BackupTimes(string path, string time) {
            worldPath = path;
            nextBackupTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(time)).DateTime;
        }
    }

    public static class Util {
        public static string TrimDoubleQuotationMarks(string target) {
            return target.Trim(new char[] { '"' });
        }
    }
}

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
    public static string appConfigPath = ".\\Config\\AppConfig.txt";

    public AppConfig() {
        if (!File.Exists(appConfigPath)) {
            //AppConfigファイルがなかった場合
            string Text = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\MinecraftAutoBackup\nMeiryo UI\n12\nnormal\njp";
            File.WriteAllText(appConfigPath, Text);
        }
        List<string> datas = new List<string>();
        StreamReader reader = new StreamReader(appConfigPath, Encoding.GetEncoding("utf-8"));
        while (reader.Peek() >= 0) {
            datas.Add(reader.ReadLine());
        }
        backupPath = datas[0];
        font = new Font(datas[1], 11);
        doZip = datas[2] == "zip" ? true : false;
        language = datas[3];
        reader.Close();

    }

}

