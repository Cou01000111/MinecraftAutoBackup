﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using MainForm;
//ちょっとFormの書き方変えてみたやつ
//結局メリットはよくわからなかった
internal class AppConfigForm :Form {
    private FormWindowState preWindowState;
    private Logger logger = new Logger("MainForm", ".\\logs\\MainForm.log", 3);
    private TabControl tab = new TabControl();
    private TabPage backupTab = new TabPage();
    private TabPage fontTab = new TabPage();
    private TabPage startupTab = new TabPage();

    private FlowLayoutPanel backupTabF = new FlowLayoutPanel();
    private Label backupPath = new Label();
    private FlowLayoutPanel backupPathPanel = new FlowLayoutPanel();
    private TextBox backupPathInput = new TextBox();
    private Button refe = new Button();
    private CheckBox doZip = new CheckBox();
    private FlowLayoutPanel backupCountPanel = new FlowLayoutPanel();
    private Label backupCountText = new Label();
    private ComboBox backupCount = new ComboBox();
    private Button addGameDir = new Button();
    private Button addWorldData = new Button();


    private FlowLayoutPanel fontTabF = new FlowLayoutPanel();
    private Label fontName = new Label();
    private Button fontChange = new Button();

    private FlowLayoutPanel okCanselFlowPanel = new FlowLayoutPanel();
    private Button ok = new Button();
    private Button cansel = new Button();

    private FlowLayoutPanel startupTabF = new FlowLayoutPanel();
    private CheckBox addStartup = new CheckBox();
    public AppConfigForm() {
        //初期化
        if (this.WindowState == FormWindowState.Minimized) {
            this.preWindowState = FormWindowState.Normal;
        }
        else {
            this.preWindowState = this.WindowState;
        }
        this.SizeChanged += new EventHandler(AppConfigForm_SizeChanged);

        //controls追加（ほかのフォームと比べ先に追加している。メリットは忘れた）
        backupPathPanel.Controls.AddRange(new Control[] { backupPathInput, refe });
        backupCountPanel.Controls.AddRange(new Control[] { backupCountText, backupCount });
        backupTabF.Controls.AddRange(new Control[] { backupPath, backupPathPanel, doZip, backupCountPanel, addGameDir });
        backupTab.Controls.Add(backupTabF);
        fontTabF.Controls.AddRange(new Control[] { fontName, fontChange });
        fontTab.Controls.Add(fontTabF);
        startupTabF.Controls.AddRange(new Control[] { addStartup });
        startupTab.Controls.Add(startupTabF);

        tab.Controls.AddRange(new Control[] { backupTab, fontTab, startupTab });
        this.Controls.Add(tab);
        okCanselFlowPanel.Controls.AddRange(new Control[] { cansel, ok });
        this.Controls.Add(okCanselFlowPanel);

        //form設定
        Text = "環境設定";
        Icon = new Icon(".\\Image\\app.ico");
        ClientSize = new Size(500, 300);
        Font = new Font(AppConfig.Font.Name, 10);
        Padding = new Padding(8);

        //各種コントロール設定

        tab.Dock = DockStyle.Fill;
        tab.Location = new Point(10, 10);
        backupTab.BackColor = SystemColors.Window;
        fontTab.BackColor = SystemColors.Window;
        startupTab.BackColor = SystemColors.Window;

        okCanselFlowPanel.Dock = DockStyle.Bottom;
        okCanselFlowPanel.Height = 40;
        okCanselFlowPanel.FlowDirection = FlowDirection.RightToLeft;

        backupTab.Text = "バックアップ";
        fontTab.Text = "フォント";
        startupTab.Text = "スタートアップに追加";

        backupPath.Text = "バックアップの保存先";
        backupPath.AutoSize = true;

        backupPathPanel.FlowDirection = FlowDirection.LeftToRight;
        backupPathPanel.Width = 480;
        backupPathPanel.Height = 32;
        //backupPathPanel.BackColor = Color.Blue;

        backupCountPanel.FlowDirection = FlowDirection.LeftToRight;
        backupCountPanel.Width = 480;
        backupCountPanel.Height = 32;

        backupTabF.Padding = new Padding(8);
        backupTabF.Dock = DockStyle.Fill;
        backupTabF.WrapContents = false;
        backupTabF.FlowDirection = FlowDirection.TopDown;
        backupTabF.Height = 20;

        fontTabF.Padding = new Padding(8);
        fontTabF.Dock = DockStyle.Fill;
        fontTabF.WrapContents = false;
        fontTabF.FlowDirection = FlowDirection.TopDown;
        fontTabF.Height = 20;

        backupPathInput.Text = AppConfig.BackupPath;
        backupPathInput.Width = 400;
        backupPathInput.Margin = new Padding(5);
        //backupPathInput.BackColor = Color.Red;

        backupCountText.Text = "保存するバックアップ数";
        backupCountText.AutoSize = true;

        backupCount.Items.AddRange(new string[] { "無制限", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "20" });
        backupCount.SelectedItem = AppConfig.BackupCount;

        refe.Text = "...";
        refe.Width = 32;
        refe.Height = 28;
        refe.BackColor = SystemColors.Control;
        refe.Click += new EventHandler(refe_Click);

        doZip.Text = "バックアップデータをZip圧縮する";
        doZip.AutoSize = true;
        doZip.Checked = AppConfig.DoZip;
        logger.Info($"dozip[{AppConfig.DoZip}]");
        //doZip.BackColor = Color.Blue;

        addGameDir.Text = "ゲームディレクトリを手動で追加する";
        addGameDir.AutoSize = true;
        addGameDir.Click += new EventHandler(addGameDir_Click);
        addGameDir.UseVisualStyleBackColor = true;

        fontName.Text = $"フォント名 :  {Util.FontStyle.Name}";
        fontName.AutoSize = true;

        fontChange.Text = "フォントを変更する";
        fontChange.AutoSize = true;
        fontChange.BackColor = SystemColors.Control;
        fontChange.Click += new EventHandler(fontChange_Click);
        fontChange.UseVisualStyleBackColor = true;

        startupTabF.Padding = new Padding(8);
        startupTabF.Dock = DockStyle.Fill;
        startupTabF.WrapContents = false;
        startupTabF.FlowDirection = FlowDirection.TopDown;
        startupTabF.Height = 20;

        addStartup.Text = "スタートアップに登録する";
        addStartup.AutoSize = true;
        addStartup.Checked = (File.Exists(Environment.GetFolderPath(System.Environment.SpecialFolder.Startup) + "\\MinecraftAutoBackup.lnk"));
        addStartup.CheckedChanged += new EventHandler(addStartup_CheckedChanged);

        ok.Text = "OK";
        ok.Width = 96;
        ok.Height = 28;
        ok.Margin = new Padding(8, 8, 0, 0);
        ok.Click += new EventHandler(ok_Click);
        cansel.Text = "キャンセル";
        cansel.Width = 96;
        cansel.Height = 28;
        cansel.Margin = new Padding(8, 8, 0, 0);
        cansel.Click += new EventHandler(cansel_Click);


    }
    //SizeChangedイベントハンドラ
    private void AppConfigForm_SizeChanged(object sender, EventArgs e) {
        //最小化された以外の時に、状態を覚えておく
        if (this.WindowState != FormWindowState.Minimized) {
            this.preWindowState = this.WindowState;
        }
    }

    //フォームが最小化されている時、元の状態に戻す
    public void RestoreMinimizedWindow() {
        if (this.WindowState == FormWindowState.Minimized) {
            this.WindowState = this.preWindowState;
        }
    }
    private void addGameDir_Click(object sender, EventArgs e) {
        List<World> worlds = new List<World>();
        CommonOpenFileDialog copd = new CommonOpenFileDialog();
        copd.Title = "ゲームディレクトリを選択してください（複数選択可）";
        copd.IsFolderPicker = true;
        copd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        copd.Multiselect = true;
        if (copd.ShowDialog() == CommonFileDialogResult.Ok) {
            List<string> addGameDir = copd.FileNames.ToList();
            List<string> removeAddGameDir = new List<string>();
            foreach (string str in addGameDir) {
                logger.Info($"{str}の判定を行います");
                if (GameDirectoryConfigExists(str)) {
                    MessageBox.Show($"ゲームディレクトリ{str}はすでに認識しています", "Minecraft Auto Backup", MessageBoxButtons.OK);
                    removeAddGameDir.Add(str);
                }
            }
            foreach (string str in removeAddGameDir) {
                addGameDir.Remove(str);
            }
            foreach (string str in addGameDir) {
                logger.Info($"{str}をconfigに追加します");
                AppConfig.AddGameDirPath.Add(str);
            }
            worlds.AddRange(Config.GetWorldDataFromHDD(addGameDir));
        }
        foreach (var w in worlds) {
            logger.Info($"configsに{w.WorldName}を追加しました");
            Config.Configs.Add(w);
            Config.Write();

        }
    }
    private void refe_Click(object sender, EventArgs e) {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Title = "バックアップ先フォルダを選択してください";
        openFileDialog.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MinecraftAutoBackup";
        openFileDialog.FileName = "SelectFolder";
        openFileDialog.Filter = "Folder |.";
        openFileDialog.CheckFileExists = false;
        if (openFileDialog.ShowDialog() == DialogResult.OK) {
            backupPathInput.Text = Path.GetDirectoryName(openFileDialog.FileName);
        }
    }

    private void fontChange_Click(object sender, EventArgs e) {
        FontDialog fontDialog = new FontDialog();
        fontDialog.MaxSize = 12;
        fontDialog.MaxSize = 8;
        fontDialog.AllowVerticalFonts = true;
        fontDialog.ShowColor = false;
        fontDialog.ShowEffects = false;
        if (fontDialog.ShowDialog() != DialogResult.Cancel) {
            AppConfig.Font = fontDialog.Font;
        }
        this.fontName.Text = fontDialog.Font.Name;
    }

    private void ok_Click(object sender, EventArgs e) {

        //backupPath
        AppConfig.BackupPath = this.backupPathInput.Text;

        //doZip
        if (AppConfig.DoZip != this.doZip.Checked) {
            List<string> backups = Util.GetBackups();
            if (AppConfig.DoZip) {
                //設定上はtrue,formのほうはflaseの場合（falseに変更された場合）
                if (backups.Count > 0) {
                    //バックアップが存在している場合
                    DialogResult r = MessageBox.Show("現在保存されているバックアップをすべて解凍しますか？\n(管理者権限が必要になります)", "保存方式", MessageBoxButtons.YesNo);
                    if (r == DialogResult.Yes) {
                        // 既存のバックアップ.zipをすべて解凍する
                        Process p = new Process();
                        string command = ".\\SubModule\\Zipper.exe";
                        string _args = "1";
                        var Decompression = new ProcessStartInfo {
                            FileName = command,
                            Arguments = _args
                        };
                        p.StartInfo = Decompression;
                        logger.Info($"Zipper {command} {_args}");
                        p.Start();
                        this.Enabled = false;
                        this.WindowState = FormWindowState.Minimized;
                        p.WaitForExit();
                        this.Enabled = true;
                        this.WindowState = FormWindowState.Normal;
                        RestoreMinimizedWindow();
                        ((WorldListForm)this.Owner).WindowState = FormWindowState.Normal;
                    }
                }
            }
            else if (!AppConfig.DoZip) {
                //設定上はfalse,formのほうはtrueの場合（trueに変更された場合）
                if (backups.Count > 0) {
                    //バックアップが存在している場合
                    DialogResult r = MessageBox.Show("現在保存されているバックアップをすべて圧縮しますか？\n(管理者権限が必要になります)", "保存方式", MessageBoxButtons.YesNo);
                    if (r == DialogResult.Yes) {
                        Process p = new Process();
                        string command = ".\\SubModule\\Zipper.exe";
                        string _args = "0";
                        var Decompression = new ProcessStartInfo {
                            FileName = command,
                            Arguments = _args
                        };
                        p.StartInfo = Decompression;
                        logger.Info($"Zipper {command} {_args}");
                        p.Start();
                        this.Enabled = false;
                        this.WindowState = FormWindowState.Minimized;
                        p.WaitForExit();
                        this.Enabled = true;
                        this.WindowState = FormWindowState.Normal;
                        RestoreMinimizedWindow();
                        ((WorldListForm)this.Owner).WindowState = FormWindowState.Normal;
                    }
                }
            }
            else {
                throw new Exception();
            }
        }
        AppConfig.DoZip = this.doZip.Checked;

        AppConfig.BackupCount = this.backupCount.SelectedItem.ToString();
        logger.Info($"selectedItem[{this.backupCount.SelectedItem.ToString()}]");
        AppConfig.WriteAppConfig();

        //WorldListViewを更新する
        ((WorldListView)((TabControl)(((WorldListForm)this.Owner).Controls[0])).TabPages[0].Controls[0]).LoadFromConfigToList();

        this.Close();
    }
    private void cansel_Click(object sender, EventArgs e) {
        this.Close();
    }

    private void addStartup_CheckedChanged(object sender, EventArgs e) {
        logger.Debug("call:addStartup_CheckedChanged");
        if (addStartup.Checked) {
            logger.Debug("true");
            //スタートアップ登録
            //ショートカットの作成
            string shortcutPath = System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.Startup), @"MinecraftAutoBackup.lnk");
            // ショートカットのリンク先(起動するプログラムのパス)
            string targetPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\SubModule\\MABProcessAtWait";
            logger.Debug($"{targetPath}へのショートカットを{shortcutPath}に作成します");

            // ショートカットを作成
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.Arguments = "";
            shortcut.WorkingDirectory = Application.StartupPath;
            shortcut.WindowStyle = 1;
            shortcut.Description = "MABProcessへのショートカットです。";
            shortcut.IconLocation = Application.ExecutablePath + ",0";
            shortcut.Save();

            // 後始末
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);

            if (File.Exists(shortcutPath)) {
                MessageBox.Show(
                    "スタートアップに登録しました。\n\n",
                    "MABProcessAtWait",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else {
                MessageBox.Show(
                    "スタートアップへの登録に失敗しました。\n\n",
                    "MABProcessAtWait",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        else {
            logger.Debug("false");
            //解除
            logger.Debug($"{Environment.GetFolderPath(System.Environment.SpecialFolder.Startup)}\\MinecraftAutoBackup.lnkを削除します");
            File.Delete($"{Environment.GetFolderPath(System.Environment.SpecialFolder.Startup)}\\MinecraftAutoBackup.lnk");
        }

    }

    private bool GameDirectoryConfigExists(string str) {
        List<string> gameDirsInConfigs = Config.GetGameDirInConfigs();
        logger.Debug($"return:{gameDirsInConfigs.Contains(Path.GetFileName(str))}");
        return gameDirsInConfigs.Contains(Path.GetFileName(str));
    }
}
