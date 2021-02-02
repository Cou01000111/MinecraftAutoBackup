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
                Config.Load();
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
                if (worldPasses.Count == 0) {
                    Console.WriteLine("info:どうやらバックアップ予定のデータはないようです");
                }
                notifyIcon.Text = $"{backupCount}/{worldPasses.Count}";
                foreach (string worldPath in worldPasses) {
                    notifyIcon.Text = $"{backupCount++}/{worldPasses.Count}";
                    if (worldPath != "dead") {
                        //前回のリロードとバックアップまでの間にワールドが消された場合
                        try { doBackup(worldPath, nowTime); }
                        catch (DirectoryNotFoundException dnfe) {
                            string backupPath = backupDataPath + "\\" + Path.GetFileName(Directory.GetParent(Directory.GetParent(worldPath).ToString()).ToString()) + "\\" + Path.GetFileName(worldPath) + "\\" + nowTime;
                            string worldBackupPath = backupDataPath + "\\" + Path.GetFileName(Directory.GetParent(Directory.GetParent(worldPath).ToString()).ToString()) + "\\" + Path.GetFileName(worldPath);
                            Console.Error.WriteLine(worldPath + ":DirectoryNotFoundException : " + dnfe.Message);
                            if (Directory.GetDirectories(worldBackupPath).Count() != 0) {
                                DialogResult r = MessageBox.Show(
                                $"バックアップ予定のワールドデータ[{Path.GetFileName(worldPath)}]が見つかりませんでした。",
                                "Minecraft Auto Backup",
                                MessageBoxButtons.YesNo);
                                //if (r == DialogResult.Yes) {
                                //    DialogResult _r = MessageBox.Show(
                                //        $"この操作は取り消せません。本当によろしいでしょうか？",
                                //        "Minecraft Auto Backup",
                                //        MessageBoxButtons.YesNo);
                                //    if (_r == DialogResult.Yes) {
                                //        //現在存在していないワールドデータのバックあぱっぷを削除する
                                //        Directory.Delete(worldBackupPath);
                                //    }
                                //}
                            }
                        }
                    }
                }
                Config.ReloadConfig();
                Console.WriteLine("全バックアップが完了しました ");

                timer.Enabled = true;
                notifyIcon.Icon = new Icon(".\\Image\\app_sub.ico");
                notifyIcon.Text = "MAB待機モジュール";
            }
        }

        //バックアップをするワールドデータのパスを配列にして返す
        List<string> GetWorldPasses() {
            List<world> _worldPasses = new List<world>();
            List<string> worldPasses = new List<string>();
            _worldPasses = Config.GetConfig();
            foreach(world w in _worldPasses) {
                if (w.WDoBackup) {
                    worldPasses.Add(w.WPath);
                }
            }
            return worldPasses;
        }

        void doBackup(string path, string Time) {
            string backupPath = backupDataPath + "\\" + Path.GetFileName(Directory.GetParent(Directory.GetParent(path).ToString()).ToString()) + "\\" + Path.GetFileName(path) + "\\" + Time;
            string worldBackupPath = backupDataPath + "\\" + Path.GetFileName(Directory.GetParent(Directory.GetParent(path).ToString()).ToString()) + "\\" + Path.GetFileName(path);
            if (AppConfig.doZip) {
                Console.WriteLine(path + " を " + backupPath + ".zip へバックアップ中です");
                ZipFile.CreateFromDirectory(path, $"{backupPath}.zip");
                Console.WriteLine(path + " を " + backupPath + " .zipへバックアップしました");
            }
            else {
                Console.WriteLine(path + " を " + backupPath + "へバックアップ中です");
                FileSystem.CopyDirectory(path, backupPath);
                Console.WriteLine(path + " を " + backupPath + "へバックアップしました");
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

    public class Config {
        /*
         必要な関数
        与えられたワールドオブジェクトをコンフィグファイルに書き変える
        コンフィグファイルの中身を渡す関数
        コンフィグファイルがないときにコンフィグファイルを作る関数
        コンフィグファイルからメモリに読み込む関数
        メモリの内容をコンフィグファイルに書き込む関数
        コンフィグファイルの内容をハードディスクの内容と照らし合わせて更新する
         ハードディスクの内容をワールドオブジェクトのListにして返す
        与えられたワールドオブジェクトをコンフィグファイルに書き加える
        与えられたワールドオブジェクトをコンフィグファイルから消す

        必要な関数:改良案

         コンフィグファイルがないときにコンフィグファイルを作る関数
         コンフィグファイルからメモリに読み込む関数
         メモリの内容をコンフィグファイルに書き込む関数
        コンフィグファイルの内容をハードディスクの内容と照らし合わせて更新する
         ハードディスクの内容をワールドオブジェクトのListにして返す
        与えられたワールドオブジェクトをメモリに書き変える
         */
        /*
        バックアップに関するオプションを記録するtxtファイル
        "バックアップの可否","ワールド名","ワールドへのパス","ワールドの所属するディレクトリ"
        が入っている
        */
        public static List<world> configs = new List<world>();

        public static string configPath = @".\Config\config.txt";

        //datasの中にworldName,worldDirに当てはまる要素があるかどうか
        private static bool IsWorldParticular(string worldName, string worldDir, string[] datas) {
            //Console.WriteLine(datas[1] + ",\"" + worldName + "\"と" + datas[3] + ",\"" + worldDir + "\"");
            return datas[1] == "\"" + worldName + "\"" && datas[3] == "\"" + worldDir + "\"";
        }

        public static List<world> GetConfig() => configs;

        /// <summary>
        /// ConfigファイルからAppに読み込む
        /// </summary>
        public static void Load() {
            Console.WriteLine("call:LoadConfigToApp");
            List<string> texts = new List<string>();
            using (StreamReader reader = new StreamReader(configPath, Encoding.GetEncoding("utf-8"))) {
                while (reader.Peek() >= 0) {
                    List<string> datas = reader.ReadLine().Split(',').ToList();
                    datas = datas.Select(x => Util.TrimDoubleQuotationMarks(x)).ToList();
                    configs.Add(new world(datas[2], Convert.ToBoolean(datas[0])));
                }
                Console.WriteLine($"info:Configから{configs.Count()}件のワールドを読み込みました");
            }
        }

        /// <summary>
        /// configsをConfig.txtに上書きする
        /// </summary>
        public static void Write() {
            List<string> text = new List<string>();
            foreach (world config in configs) {
                text.Add($"\"{config.WDoBackup}\",\"{config.WName}\",\"{config.WPath}\",\"{config.WDir}\"\n");
            }
            File.WriteAllText(configPath, string.Join("", text), Encoding.GetEncoding("utf-8"));
        }


        /// <summary>
        /// Configファイルを更新する
        /// こちらの関数はmainFormと違い、configから消されたワールドを返り値にしている
        /// </summary>

        /// <summary>
        /// Configファイルを更新する
        /// </summary>
        public static List<world> ReloadConfig() {
            Console.WriteLine("call:reloadConfig");
            List<world> worldInPc = GetWorldDataFromHDD();
            List<world> worldInConfig = GetConfig();
            //Console.WriteLine(worldInConfig.Count());
            //Console.WriteLine(worldInPc.Count());

            //configに存在しないpathを追加する
            foreach (world pc in worldInPc) {
                if (!worldInConfig.Select(x => x.WPath).ToList().Contains(pc.WPath)) {
                    Console.WriteLine($"info:ADD {pc.WName}");
                    configs.Add(pc);
                }
            }
            List<world> removeWorlds = new List<world>();

            //削除されたワールドはconfig.pathに"dead"と入れる
            int wI = 0;
            foreach (world world in worldInConfig) {
                if (!worldInPc.Select(x => x.WPath).ToList().Contains(world.WPath)) {
                    if (Directory.GetDirectories($"{AppConfig.backupPath}\\{world.WDir}\\{world.WName}").Count() == 0) {
                        removeWorlds.Add(world);
                    }
                    else { 
                        Config.configs[wI].WPath = "dead"; 
                    }
                }
                wI++;
            }

            foreach (world w in removeWorlds) {
                if (configs.Remove(w)) {
                    Console.WriteLine($"info:REMOVE {w.WName} suc");
                }
                else {
                    Console.WriteLine($"info:REMOVE {w.WName} 見つかりませんでした");
                }
            }

            return removeWorlds;
        }

        public static void KillConfig(world w) {

        }
        public static void Change(string worldName, string worldDir, string doBackup) {
            Console.WriteLine("call:Change");
            Console.WriteLine("info:GET  worldName: " + worldName + ",  worldDir: " + worldDir + ",  dobackup: " + doBackup);
            List<world> _configs = new List<world>();
            foreach (world config in configs) {
                if (config.WName == worldName && config.WDir == worldDir) {
                    config.WDoBackup = bool.Parse(doBackup);
                    _configs.Add(new world(config.WPath, Convert.ToBoolean(doBackup)));
                }
                else {
                    _configs.Add(new world(config.WPath, config.WDoBackup));
                }
            }
            configs = _configs;
            //ConsoleConfig();
        }

        /// <summary>
        /// PCからワールドデータ一覧を取得
        /// </summary>
        /// <returns>取得したList<world></returns>
        private static List<world> GetWorldDataFromHDD() {
            Console.WriteLine("call:GetWorldDataFromPC");
            List<world> worlds = new List<world>();
            List<string> _gameDirectory = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).ToList();
            List<string> gameDirectory = new List<string>();
            foreach (string dir in _gameDirectory) {
                List<string> dirsInDir = Directory.GetDirectories(dir).ToList();
                dirsInDir = dirsInDir.Select(x => Path.GetFileName(x)).Cast<string>().ToList();
                if (dirsInDir.Contains("logs") && dirsInDir.Contains("resourcepacks") && dirsInDir.Contains("saves")) {
                    //Console.WriteLine($"info:ゲームディレクトリ[{dir}]を発見しました");
                    gameDirectory.Add(dir);
                }
            }
            foreach (string dir in gameDirectory) {
                List<string> _worlds = Directory.GetDirectories($"{dir}\\saves").ToList();
                foreach (string worldPath in _worlds) {
                    worlds.Add(new world(Util.TrimDoubleQuotationMarks(worldPath)));
                }
            }
            //foreach(var a in worlds) {
            //    Console.WriteLine($"info:world[{a.WName}]");
            //}
            return worlds;
        }

        /// <summary>
        /// PCからワールドデータ一覧を取得
        /// </summary>
        /// <returns>取得したList<world></returns>
        private static List<world> GetWorldDataFromHDD(List<string> gameDirectory) {
            List<world> worlds = new List<world>();
            Console.WriteLine("call:GetWorldDataFromPC");
            foreach (string dir in gameDirectory) {
                if (Directory.Exists($"{dir}\\saves")) {
                    List<string> _worlds = Directory.GetDirectories($"{dir}\\saves").ToList();
                    foreach (string worldPath in _worlds) {
                        worlds.Add(new world(Util.TrimDoubleQuotationMarks(worldPath)));
                    }
                }
            }
            //foreach(var a in worlds) {
            //    Console.WriteLine($"info:world[{a.WName}]");
            //}
            return worlds;
        }

        public static void ConsoleConfig() {
            Console.WriteLine("----Configs----");
            foreach (world w in configs) {
                Console.WriteLine($"[{w.WDoBackup},{w.WName},{w.WPath},{w.WDir},]");
            }
            Console.WriteLine("---------------");
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


    public class world {
        public bool WDoBackup { get; set; }
        public string WPath { get; set; }
        public string WName { get; set; }
        public string WDir { get; set; }
        public world(string path) {
            //if (!Directory.Exists(path)) {
            //    Console.WriteLine($"info:不正なpath[{path}]が渡されました");
            //    return;
            //}
            WDoBackup = true;
            WPath = path;
            WName = Path.GetFileName(path);
            WDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
        }

        public world(string path, bool doBackup) {
            //if (!Directory.Exists(path)) {
            //    Console.WriteLine($"info:不正なpath[{path}]が渡されました");
            //    return;
            //}
            WDoBackup = doBackup;
            WPath = path;
            WName = Path.GetFileName(path);
            WDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
        }

    }

}

