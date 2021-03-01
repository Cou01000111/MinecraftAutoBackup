using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
//ちょっとFormの書き方変えてみたやつ
//結局メリットはよくわからなかった
internal class AppConfigForm :Form {

    TabControl tab = new TabControl();
    TabPage backupTab = new TabPage();
    TabPage fontTab = new TabPage();
    TabPage startupTab = new TabPage();

    FlowLayoutPanel backupTabF = new FlowLayoutPanel();
    Label backupPath = new Label();
    FlowLayoutPanel backupPathPanel = new FlowLayoutPanel();
    TextBox backupPathInput = new TextBox();
    Button refe = new Button();
    CheckBox doZip = new CheckBox();
    FlowLayoutPanel backupCountPanel = new FlowLayoutPanel();
    Label backupCountText = new Label();
    ComboBox backupCount = new ComboBox();
    Button addGameDir = new Button();
    Button addWorldData = new Button();


    FlowLayoutPanel fontTabF = new FlowLayoutPanel();
    Label fontName = new Label();
    Button fontChange = new Button();

    FlowLayoutPanel okCanselFlowPanel = new FlowLayoutPanel();
    Button ok = new Button();
    Button cansel = new Button();

    FlowLayoutPanel startupTabF = new FlowLayoutPanel();
    CheckBox addStartup = new CheckBox();


    public AppConfigForm() {

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
        FormClosing += new FormClosingEventHandler(AppConfigForm_Closing);

        //各種コントロール設定

        tab.Dock = DockStyle.Fill;
        tab.Location = new Point(10, 10);
        backupTab.BackColor = SystemColors.Window;
        fontTab.BackColor = SystemColors.Window;
        startupTab.BackColor = SystemColors.Window;
        
        okCanselFlowPanel.Dock = DockStyle.Bottom;
        okCanselFlowPanel.Height = 40;
        okCanselFlowPanel.FlowDirection = FlowDirection.RightToLeft;

        backupTab.Text =  "バックアップ";
        fontTab.Text =  "フォント";
        startupTab.Text =  "スタートアップに追加";

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
        Logger.Info($"dozip[{AppConfig.DoZip}]");
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
                Logger.Info($"{str}の判定を行います");
                if (GameDirectoryConfigExists(str)) {
                    MessageBox.Show($"ゲームディレクトリ{str}はすでに認識しています", "Minecraft Auto Backup", MessageBoxButtons.OK);
                    removeAddGameDir.Add(str);
                }
            }
            foreach (string str in removeAddGameDir) {
                addGameDir.Remove(str);
            }
            foreach (string str in addGameDir) {
                Logger.Info($"{str}をconfigに追加します");
                AppConfig.AddGameDirPath.Add(str);
            }
            worlds.AddRange(Config.GetWorldDataFromHDD(addGameDir));
        }
        foreach (var w in worlds) {
            Logger.Info($"configsに{w.WName}を追加しました");
            Config.configs.Add(w);
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
            if (AppConfig.DoZip) {
                //設定上はtrue,formのほうはflaseの場合（falseに変更された場合）
                List<string> backups = Util.GetBackups();
                if (backups.Count > 0) {
                    //バックアップが存在している場合

                    //バックアップが存在している場合
                    DialogResult r = MessageBox.Show("現在保存されているバックアップをすべて解凍しますか？", "保存方式", MessageBoxButtons.YesNo);
                    if (r == DialogResult.Yes) {
                        // 既存のバックアップ.zipをすべて解凍する
                        string command = ".\\SubModule\\Zipper.exe";
                        string _args = "1";
                        var Decompression = new ProcessStartInfo {
                            FileName = command,
                            Arguments = _args
                        };
                        Logger.Info($"Zipper {command} {_args}");
                        Process.Start(Decompression);
                    }
                }
            }
            else if (!AppConfig.DoZip) {
                //設定上はfalse,formのほうはtrueの場合（trueに変更された場合）
                List<string> backups = Util.GetBackups();

                if (backups.Count > 0) {
                    //バックアップが存在している場合
                    DialogResult r = MessageBox.Show("現在保存されているバックアップをすべて圧縮しますか？", "保存方式", MessageBoxButtons.YesNo);
                    if (r == DialogResult.Yes) {
                        // 既存のバックアップ.zipをすべて解凍する
                        string command = ".\\SubModule\\Zipper.exe";
                        string _args = "0";
                        var doZipping = new ProcessStartInfo {
                            FileName = command,
                            Arguments = _args
                        };
                        Logger.Info($"Zipper {command} {_args}");
                        Process.Start(doZipping);
                        //Util.task = Task.Run(() => { DoZipping(backups); });
                    }
                }
            }
            else {
                throw new Exception();
            }
        }
        AppConfig.DoZip = this.doZip.Checked;

        AppConfig.BackupCount = this.backupCount.SelectedItem.ToString();
        Logger.Info($"selectedItem[{this.backupCount.SelectedItem.ToString()}]");
        AppConfig.WriteAppConfig();

        //WorldListViewを更新する
        ((WorldListView)((TabControl)(((WorldListForm)this.Owner).Controls[0])).TabPages[0].Controls[0]).LoadFromConfigToList();

        this.Close();
    }
    private void cansel_Click(object sender, EventArgs e) {
        this.Close();
    }

    private void addStartup_CheckedChanged(object sender, EventArgs e) {
        Logger.Debug("call:addStartup_CheckedChanged");
        if (addStartup.Checked) {
            Logger.Debug(true);
            //スタートアップ登録
            //ショートカットの作成
            string shortcutPath = System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.Startup), @"MinecraftAutoBackup.lnk");
            // ショートカットのリンク先(起動するプログラムのパス)
            string targetPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\SubModule\\MABProcessAtWait";
            Logger.Debug($"{targetPath}へのショートカットを{shortcutPath}に作成します");

            // WshShellを作成
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            // ショートカットのパスを指定して、WshShortcutを作成
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
            // ①リンク先
            shortcut.TargetPath = targetPath;
            // ②引数
            shortcut.Arguments = "";
            // ③作業フォルダ
            shortcut.WorkingDirectory = Application.StartupPath;
            // ④実行時の大きさ 1が通常、3が最大化、7が最小化
            shortcut.WindowStyle = 1;
            // ⑤コメント
            shortcut.Description = "MABProcessへのショートカットです。";
            // ⑥アイコンのパス 自分のEXEファイルのインデックス0のアイコン
            shortcut.IconLocation = Application.ExecutablePath + ",0";

            // ショートカットを作成
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
            Logger.Debug(false);
            //解除
            Logger.Debug($"{Environment.GetFolderPath(System.Environment.SpecialFolder.Startup)}\\MinecraftAutoBackup.lnkを削除します");
            File.Delete($"{Environment.GetFolderPath(System.Environment.SpecialFolder.Startup)}\\MinecraftAutoBackup.lnk");
        }
        
    }

    private void AppConfigForm_Closing(object sender, EventArgs e) {

    }

    private bool GameDirectoryConfigExists(string str) {
        List<string> gameDirsInConfigs = Config.GetGameDirInConfigs();
        Logger.Debug($"return:{gameDirsInConfigs.Contains(Path.GetFileName(str))}");
        return gameDirsInConfigs.Contains(Path.GetFileName(str));
    }
}
