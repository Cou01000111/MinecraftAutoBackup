using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

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
        private static Logger logger = new Logger("Zipper");
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
                if (Directory.Exists(AppConfig.BackupPath) == false) {
                    logger.Warn($"バックアップ先フォルダが存在しないため{AppConfig.BackupPath}を作成します");
                    Directory.CreateDirectory(AppConfig.BackupPath);
                }
                Util.NotReadonly(AppConfig.BackupPath);
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
        private static Logger logger = new Logger("Zipper");
        private System.Windows.Forms.Timer timer;
        private string backupDataPath;
        private NotifyIcon notifyIcon;
        private bool isRunning = false;
        private Task backupTask;

        public Form1() {
            logger.Info("Current Dir: "+System.Environment.CurrentDirectory);
            backupDataPath = AppConfig.BackupPath;
            this.ShowInTaskbar = false;
            this.Icon = new Icon(".\\Image\\app.ico");
            this.FormClosing += new FormClosingEventHandler(Form1_Closing);

            timer = new System.Windows.Forms.Timer() {
                Enabled = true
            };
            timer.Interval = 1000;
            timer.Tick += new EventHandler(Timer_Tick);

            SetNotifyIconWait();
        }

        private void Close_Click(object sender, EventArgs e) {
            if ((backupTask == null) || (backupTask.IsCompleted)) {
                logger.Info("アプリケーションが終了しました");
            }
            else {
                logger.Warn("アプリケーションが強制終了しました");
            }

            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private void Form1_Closing(object sender, EventArgs e) {
            logger.Info("アプリケーションが強制終了しました");
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            // lancherが起動してない場合 => 何もしない
            // lancherが起動していてflagがfalse => flagをtrueにしてバックアップ動作
            // lancherが起動していてflagがtrue => 何もしない
            // lancherが起動していなくてflagがtrue => flagをfalseにする
            if (Process.GetProcessesByName("MinecraftLauncher").Length > 0 && !isRunning) {
                while (Util.IsZipperRunning()) {
                    System.Threading.Tasks.Task.Delay(10000);
                }
                isRunning = true;
                logger.Info("Minecraft Lancherの起動を検知しました");
                logger.Info("isRunningがfalseに設定されていました");

                SetNotifyIconBackuping();
                backupTask = Task.Run(() => {
                    DoBackupProcess();
                });
            }
            else if (!(Process.GetProcessesByName("MinecraftLauncher").Length > 0) && isRunning) {
                logger.Info("ランチャーの停止を検知しました");
                logger.Info("isRunningにfalseを設定します");
                isRunning = false;
            }
        }

        private void bootMainForm_Click(object sender, EventArgs e) {
            logger.Info(System.Environment.CurrentDirectory);
            
            var mainForm = new ProcessStartInfo();
            mainForm.FileName = ".\\Minecraft Auto Backup.exe";
            mainForm.UseShellExecute = true;
            mainForm.WorkingDirectory = ".\\";
            Process.Start(mainForm);
        }

        private void DoBackupProcess() {
            logger.Info("バックアッププロセスを始めます");
            //zipperが起動している場合はバックアップを保留にする
            //バックアップがない場合で、_tmpファイルがある場合は前回のZipperがmoveを失敗してるだけの可能性があるから名前変更
            if (Directory.Exists(AppConfig.BackupPath + "_tmp") && (!Directory.Exists(AppConfig.BackupPath))) {
                Directory.Move(AppConfig.BackupPath + "_tmp", AppConfig.BackupPath);
            }
            int backupCount = 0;
            List<string> worldPasses = GetWorldPasses();// バックアップをするワールドへのパス一覧
            string nowTime = DateTime.Now.ToString("yyyyMMddHHmm");
            if (worldPasses.Count == 0) {
                logger.Info("どうやらバックアップ予定のデータはないようです");
            }

            notifyIcon.Text = $"{backupCount}/{worldPasses.Count}";
            foreach (string worldPath in worldPasses) {
                notifyIcon.Text = $"{backupCount++}/{worldPasses.Count}";
                //前回のリロードとバックアップまでの間にワールドが消された場合
                string backupPath = backupDataPath + "\\" + Path.GetFileName(Directory.GetParent(Directory.GetParent(worldPath).ToString()).ToString()) + "\\" + Path.GetFileName(worldPath) + "\\" + nowTime;
                string worldBackupPath = backupDataPath + "\\" + Path.GetFileName(Directory.GetParent(Directory.GetParent(worldPath).ToString()).ToString()) + "\\" + Path.GetFileName(worldPath);
                try { DoBackup(worldPath, nowTime); }
                catch (DirectoryNotFoundException dnfe) {

                    Console.Error.WriteLine(worldPath + ":DirectoryNotFoundException : " + dnfe.Message);
                    if (!Directory.Exists(worldPath)) {
                        DialogResult r = MessageBox.Show(
                        $"バックアップ予定のワールドデータ[{Path.GetFileName(worldPath)}]が見つかりませんでした。",
                        "Minecraft Auto Backup",
                        MessageBoxButtons.OK);
                    }
                }

                //バックアップ超過分削除
                if (AppConfig.BackupCount != "無制限") {
                    if (Directory.GetFileSystemEntries(worldBackupPath).ToList().Count() > int.Parse(AppConfig.BackupCount)) {
                        logger.Info($"{worldBackupPath}のバックアップ数({Directory.GetFileSystemEntries(worldBackupPath).ToList().Count()})が超過している(AppConfig:{int.Parse(AppConfig.BackupCount)})ので削除処理に移ります");
                        //バックアップ数がappconfig.backupCountより多い場合超過分を削除する
                        List<string> backups = Directory.GetFileSystemEntries(worldBackupPath)
                            .OrderByDescending(filePath => File.GetLastWriteTime(filePath).Date)
                            .ThenByDescending(filePath => File.GetLastWriteTime(filePath).TimeOfDay).ToList();
                        logger.Debug($"buckups count: {backups.Count()}");
                        foreach (string s in backups) {
                            logger.Debug($"backups:[{s}]");
                        }
                        List<string> deleteBackups = new List<string>();
                        for (int i = int.Parse(AppConfig.BackupCount); i < backups.Count(); i++) {
                            logger.Info($"{backups[i]}を削除します");

                            //zipファイルかどうか判定
                            if (backups[i].Contains(".zip")) {
                                //zipファイルの場合
                                try { File.Delete(backups[i]); }
                                catch (Exception exc) {
                                    logger.Error($"{backups[i]}");
                                    logger.Error($"{exc.Message}");
                                    logger.Error($"{exc.StackTrace}");
                                }
                            }
                            else {
                                //ディレクトリの場合
                                try { Directory.Delete(backups[i], true); }
                                catch (Exception exc) {
                                    logger.Error($"{backups[i]}");
                                    logger.Error($"{exc.Message}");
                                    logger.Error($"{exc.StackTrace}");
                                }
                            }

                        }
                    }
                    else {
                        logger.Info($"{worldBackupPath}({Directory.GetFileSystemEntries(worldBackupPath).ToList().Count()})の超過分(AppConfig:{int.Parse(AppConfig.BackupCount)})は発見されませんでした");
                    }
                }

            }
            Config.SyncConfig();
            logger.Info("全バックアップが完了しました ");
            timer.Enabled = true;
            SetNotifyIconWait();
        }

        //バックアップをするワールドデータのパスを配列にして返す
        private List<string> GetWorldPasses() {
            List<World> _worldPasses = new List<World>();
            List<string> worldPasses = new List<string>();
            _worldPasses = Config.Configs;
            foreach (World w in _worldPasses) {
                if (w.WorldDoBackup && w.WorldIsAlive) {
                    //バックアップをする予定でかつ、バックアップ元が生きているワールドのみ追加
                    worldPasses.Add(w.WorldPath);
                }
            }
            return worldPasses;
        }

        private void DoBackup(string path, string Time) {
            string backupPath = backupDataPath + "\\" + Path.GetFileName(Directory.GetParent(Directory.GetParent(path).ToString()).ToString()) + "\\" + Path.GetFileName(path) + "\\" + Time;
            string worldBackupPath = backupDataPath + "\\" + Path.GetFileName(Directory.GetParent(Directory.GetParent(path).ToString()).ToString()) + "\\" + Path.GetFileName(path);
            if (AppConfig.DoZip)
                backupPath += ".zip";
            if (!Directory.Exists(worldBackupPath)) {
                Directory.CreateDirectory(worldBackupPath);
            }
            if (AppConfig.DoZip) {
                logger.Info(path + " を " + backupPath + " へバックアップ中です");
                ZipFile.CreateFromDirectory(path, backupPath);
                logger.Info(path + " を " + backupPath + "へバックアップしました");
            }
            else {
                logger.Info(path + " を " + backupPath + "へバックアップ中です");
                FileSystem.CopyDirectory(path, backupPath);
                logger.Info(path + " を " + backupPath + "へバックアップしました");
            }
        }

        private void SetNotifyIconWait() {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(".\\Image\\app_sub.ico");
            notifyIcon.Text = "MAB待機モジュール";
            notifyIcon.Visible = true;
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem exit = new ToolStripMenuItem();
            exit.Text = "終了";
            exit.Click += new EventHandler(Close_Click);
            menu.Items.Add(exit);
            ToolStripMenuItem bootMainForm = new ToolStripMenuItem();
            bootMainForm.Text = "バックアップの設定を編集する";
            bootMainForm.Click += new EventHandler(bootMainForm_Click);
            menu.Items.Add(bootMainForm);
            notifyIcon.ContextMenuStrip = menu;
        }

        private void SetNotifyIconBackuping() {
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(".\\Image\\app_sub_doing.ico");
            notifyIcon.Text = "MAB待機モジュール(バックアップ中)";
            notifyIcon.Visible = true;
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem exit = new ToolStripMenuItem();
            exit.Text = "強制終了";
            exit.Click += new EventHandler(Close_Click);
            menu.Items.Add(exit);
            notifyIcon.ContextMenuStrip = menu;
        }
    }
}
