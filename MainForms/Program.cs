using Microsoft.VisualBasic.FileIO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
class Program {
    [STAThread]
    static void Main() {
        //起動時処理
        Console.WriteLine("-----起動時処理-------");

        new AppConfig();

        if (!File.Exists(Config.configPath)) {
            Console.WriteLine("info:configファイルがないのでconfigファイルを作成します");
            Config.MakeConfig();
        }
        else {
            Config.Load();
            Config.ReloadConfig();
        }

        Console.WriteLine("----------------------");
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new WorldListForm());

    }
}

class WorldListForm :Form {
    private MenuStrip menu;
    private TabControl tabControl;
    private TabPage tabPageEdit;
    private TabPage tabPageBackup;
    private Color yesColor;
    private Color cancelColor;
    //編集画面
    private WorldListView worldListView;
    private Label description;
    private Button ok;
    private Button cancel;
    private Panel buttomPanel;
    private FlowLayoutPanel flowLayoutPanel;
    //バックアップ画面
    private BackupDataPanel backupDataTable;

    public WorldListForm() {
        //Form設定
        this.Text = "Minecraft Auto Backup";
        this.Icon = new Icon(".\\Image\\app.ico");
        this.Font = new Font(AppConfig.Font.Name, 11);
        Console.WriteLine(AppConfig.Font.Name);

        Util.FontStyle = AppConfig.Font;
        this.FormClosing += new FormClosingEventHandler(WorldListForm_FormClosing);
        this.Resize += new EventHandler(Form_Resize);
        this.yesColor = Color.FromArgb(15, 27, 51);
        this.cancelColor = Color.FromArgb(147, 31, 31);


        #region tabControl
        tabControl = new TabControl() {
            Dock = DockStyle.Fill,
            Location = new Point(0, 30),
            Margin = new Padding(30, 0, 0, 0),
        };
        tabPageEdit = new TabPage() {
            Name = "edit",
            Text = "設定編集",
        };
        tabPageBackup = new TabPage() {
            Name = "backup",
            Text = "バックアップから復元",
        };
        #endregion

        #region ツールバー

        menu = new MenuStrip() {
            Font = this.Font,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 0, 0, 0),
            Height = (int)Util.FontStyle.Size * 2 + 4,
        };

        //ファイル

        ToolStripMenuItem fileMenu = new ToolStripMenuItem() {
            Text = "ファイル(&F)",
            Margin = new Padding(0, 0, 0, 0),
        };
        menu.Items.Add(fileMenu);

        ToolStripMenuItem configMenu = new ToolStripMenuItem() {
            Text = "環境設定(P)"
        };
        configMenu.Click += new EventHandler(Config_Click);
        fileMenu.DropDownItems.Add(configMenu);

        ToolStripMenuItem exitMenu = new ToolStripMenuItem() {
            Text = "終了(&X)"
        };
        exitMenu.Click += new EventHandler(Exit_Click);
        fileMenu.DropDownItems.Add(exitMenu);

        //編集
        //ToolStripMenuItem editMenu = new ToolStripMenuItem() {
        //    Text = "表示(&V)",
        //    Margin = new Padding(0, 0, 0, 0),
        //};
        //menu.Items.Add(editMenu);

        //表示
        //ヘルプ

        //this.Controls.Add(menu);[

        #endregion

        #region 編集画面

        tabControl.TabPages.Add(tabPageEdit);
        tabControl.TabPages.Add(tabPageBackup);


        worldListView = new WorldListView() {
            GridLines = true,
            View = View.Details,
            Dock = DockStyle.Fill,
        };

        description = new Label() {
            Text = "以上のワールドを認識しました。",
            //BackColor = Color.Blue,
            Anchor = AnchorStyles.Left,
            Width = (int)Util.FontStyle.Size * 25,
            Height = (int)Util.FontStyle.Size * 2 + 4,
            Location = new Point(8, 8),
        };

        ok = new Button() {
            Text = "OK",
            UseVisualStyleBackColor = true,
            //Width = (int)Util.FontStyle.Size * 10,
            //Height = (int)Util.FontStyle.Size * 2 + 4,
            ForeColor = yesColor,
            Width = 96,
            Height = 28,
            Margin = new Padding(8, 8, 0, 0),
        };
        ok.Click += new EventHandler(Ok_Click);

        cancel = new Button() {
            Text = "キャンセル",
            UseVisualStyleBackColor = true,
            //Width = (int)Util.FontStyle.Size * 10,
            //Height = (int)Util.FontStyle.Size * 2 + 4,
            ForeColor = cancelColor,
            Width = 96,
            Height = 28,
            Margin = new Padding(8, 8, 0, 0),
        };
        cancel.Click += new EventHandler(Cancel_Click);

        buttomPanel = new Panel() {
            Dock = DockStyle.Bottom,
            //BackColor = Color.Blue,
            Height = 40,
        };

        flowLayoutPanel = new FlowLayoutPanel() {
            FlowDirection = FlowDirection.RightToLeft,
            //BackColor = Color.Red,
            Anchor = (AnchorStyles.Left | AnchorStyles.Right)

        };

        this.BackColor = SystemColors.Window;

        flowLayoutPanel.Controls.Add(cancel);
        flowLayoutPanel.Controls.Add(ok);


        buttomPanel.Controls.Add(description);
        buttomPanel.Controls.Add(flowLayoutPanel);

        tabPageEdit.Controls.Add(worldListView);
        tabPageEdit.Controls.Add(buttomPanel);

        #endregion

        #region バックアップ画面
        backupDataTable = new BackupDataPanel();
        tabPageBackup.Controls.Add(backupDataTable);
        #endregion

        this.Controls.Add(tabControl);
        this.Controls.Add(menu);
        int i = 0;
        foreach (Control a in this.backupDataTable.Controls) {
            this.backupDataTable.Controls[i].Width = this.Width - 60;
            //this.backupDataTable.Controls[i].Controls[0].Width = this.Width - 60;
            i++;
        }
        this.Load += new EventHandler(Form_Load);
    }

    void Form_Load(object sender, EventArgs e) {
        this.ClientSize = AppConfig.ClientSize;
        this.Location = AppConfig.ClientPoint;
    }

    void Form_Resize(object sender, EventArgs e) {
        //Console.WriteLine("大きさが変更されました大きさが変更されました");
        int i = 0;
        foreach (Control a in this.backupDataTable.Controls) {
            this.backupDataTable.Controls[i].Width = this.Width - 60;
            //this.backupDataTable.Controls[i].Controls[0].Width = this.Width - 60;
            i++;
        }
    }
    void Ok_Click(object sender, EventArgs e) {
        Console.WriteLine("info:push ok");
        List<string[]> configs = worldListView.GetWorldListView();
        foreach (var config in configs) {
            //Console.WriteLine($"{config[0]}, {config[1]}, {config[2]}");
            Config.Change(config[1], config[2], config[0]);
        }
        Config.Write();
        this.Close();
        //Application.Exit();
    }
    void Cancel_Click(object sender, EventArgs e) {
        string message = "設定を保存しないで終了しますか？";
        string caption = "保存できてないよ？";
        MessageBoxButtons buttons = MessageBoxButtons.YesNo;
        DialogResult result = MessageBox.Show(this, message, caption, buttons, MessageBoxIcon.Question);
        if (result == DialogResult.Yes) {
            this.Close();
        }
    }
    void WorldListForm_FormClosing(object sender, CancelEventArgs e) {
        Console.WriteLine("info:終了時処理を開始します");

        AppConfig.ClientPoint = this.Location;
        AppConfig.ClientSize = this.ClientSize;
        AppConfig.WriteAppConfig();
        //try {
        //    System.Diagnostics.Process.Start(".\\SubModule\\MABProcessAtWait.exe");
        //}
        //catch (Win32Exception w) {
        //    Console.WriteLine(w.Message);
        //    Console.WriteLine(w.ErrorCode.ToString());
        //    Console.WriteLine(w.NativeErrorCode.ToString());
        //    Console.WriteLine(w.StackTrace);
        //    Console.WriteLine(w.Source);
        //    Exception f = w.GetBaseException();
        //    Console.WriteLine(f.Message);
        //}
        if ((Util.task != null)) {
            if (!(Util.task.IsCompleted)) {
                //非同期で実行しているタスクがまだ終わっていない場合
                Console.WriteLine("info:タスクが未完了なのでバックグラウンドで処理します");
                //this.Close();
                BackGroundTasks f = new BackGroundTasks();
                Application.Run();
            }
        }
    }
    void Exit_Click(object sender, EventArgs e) {
        Console.WriteLine("info:push exit");
        this.Close();
    }
    void Config_Click(object sender, EventArgs e) {
        AppConfigForm appConfigForm = new AppConfigForm();
        appConfigForm.Show();
    }
}



class WorldListView :ListView {
    private ColumnHeader clmnDoBackup; // 'バックアップ' 列ヘッダ
    private ColumnHeader clmnWorldName;  // 'ワールド名' 列ヘッダ
    private ColumnHeader clmnWorldDir;  // '所属ディレクトリ' 列ヘッダ

    //コンストラクタ
    public WorldListView() {
        FullRowSelect = true;
        MultiSelect = false;
        CheckBoxes = true;
        Name = "worldListView";
        clmnDoBackup = new ColumnHeader() { Text = "ﾊﾞｯｸｱｯﾌﾟの可否" };
        clmnWorldName = new ColumnHeader() { Text = "ワールド名" };
        clmnWorldDir = new ColumnHeader() { Text = "所属ディレクトリ" };

        this.Columns.AddRange(new ColumnHeader[]{
            clmnDoBackup,
            clmnWorldName,
            clmnWorldDir,
        });

        foreach (ColumnHeader ch in this.Columns) {
            ch.Width = -1;
        }

        this.Columns[0].Width = (int)Util.FontStyle.Size * 14;

        this.ItemCheck += new ItemCheckEventHandler(listView_ItemClick);

        LoadFromConfigToList();

    }

    public void LoadFromConfigToList() {
        this.Items.Clear();
        List<World> listDatas = Config.GetConfig();
        int iItemCount = 0;
        foreach (var datas in listDatas) {
            if (datas.isAlive) {
                this.Items.Add(new ListViewItem(new string[] { " ", datas.WName, datas.WDir }));
                //Console.WriteLine(datas.WDoBackup);
                this.Items[iItemCount].Checked = Convert.ToBoolean(datas.WDoBackup);
                iItemCount++;
            }

        }
    }
    private void worldEditForm_Closed(object sender, EventArgs e) {
        LoadFromConfigToList();
    }
    private void listView_ItemClick(object sender, EventArgs e) {

    }

    /// <summary>
    /// ListViewの内容を{"ワールド名","ディレクトリ名","true or false"}のListにして返す
    /// </summary>
    /// <returns>{"ワールド名","ディレクトリ名","true or false"}の文字列配列</returns>
    public List<string[]> GetWorldListView() {
        List<string[]> _return = new List<string[]>();
        int i = 0;
        foreach (var item in this.Items) {
            _return.Add(new string[] { this.Items[i].Checked.ToString(), this.Items[i].SubItems[1].Text, this.Items[i].SubItems[2].Text });
            i++;
        }
        return _return;
    }
}

#region tab backup
class BackupDataPanel :FlowLayoutPanel {
    private Label backupDataDir;
    private BackupDataListView backupDataList;
    private AddInfoButton addInfo;
    private FlowLayoutPanel dualPanel;
    private FlowLayoutPanel panel;
    //コンストラクタ
    public BackupDataPanel() {
        this.Dock = DockStyle.Fill;
        this.FlowDirection = FlowDirection.TopDown;
        this.AutoScroll = true;
        this.WrapContents = false;


        int iCount = 0;
        List<string> backups = Util.GetBackups();

        if (backups.Count() == 0) {
            Console.WriteLine("info:バックアップが存在しません");
            Label notBackupFile = new Label() {
                Text = "バックアップが存在しません",
                Margin = new Padding(10),
            };
            this.Controls.Add(notBackupFile);
        }
        else {
            Console.WriteLine($"{Config.GetConfig().Count()}件のワールドのバックアップを読み込みます");
            foreach (World world in Config.GetConfig()) {
                ////バックアップがない場合表示しない
                ////バックアップがなくかつ、元が死んでいる場合はconfigsから削除
                //if (GetBackupFiles(world.WName, world.WDir).Count() <= 0) {
                //    if(world.WPath == "dead") {
                //        deadF = true;
                //        if (!Config.configs.Remove(world)) {
                //            //reloadでバックアップが存在しない削除済みワールドはconfigにないはずなのでエラー
                //            throw new Exception();
                //        };
                //        Config.Write();
                //    }
                //    continue;
                //}
                if (Util.GetBackup(world).Count() == 0) {
                    //バックアップが一つもない場合continue
                    continue;
                }

                backupDataDir = new Label() {
                    Text = world.WName + "/" + world.WDir + "",
                    AutoSize = true,
                    Height = (int)Util.FontStyle.Size * 2,
                    Margin = new Padding((int)Util.FontStyle.Size),
                    //BackColor = Color.Yellow
                };
                if (!world.isAlive) {
                    backupDataDir.ForeColor = Color.Red;
                }
                addInfo = new AddInfoButton(world.WPath) {

                    id = iCount,
                    //BackColor = Color.Red
                };
                dualPanel = new FlowLayoutPanel() {
                    FlowDirection = FlowDirection.LeftToRight,
                    //BackColor = Color.Aquamarine,
                    Width = (int)Util.FontStyle.Size * 60,
                    Height = addInfo.Height + 8,
                };
                panel = new FlowLayoutPanel() {
                    FlowDirection = FlowDirection.TopDown,
                    //BackColor = Color.Coral,
                    Width = (int)Util.FontStyle.Size * 60 - 50,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding((int)Util.FontStyle.Size / 2),
                    Height = dualPanel.Height + Margin.Left * 2,
                    WrapContents = false
                };
                addInfo.Click += new EventHandler(addInfo_Click);
                dualPanel.Controls.Add(addInfo);
                dualPanel.Controls.Add(backupDataDir);
                panel.Controls.Add(dualPanel);
                this.Controls.Add(panel);
                iCount++;
            }

        }
    }

    void addInfo_Click(object sender, EventArgs e) {
        var button = (AddInfoButton)sender;
        int index = button.id;
        //Console.WriteLine("-------------------");
        //Console.WriteLine("押されたボタンの名前:" + index);
        //Console.WriteLine("比較対象:" + index);
        //this.Controls[index].BackColor = Color.Red;
        //Console.WriteLine("true or false:" + (this.Controls[index].Controls.Count == 2).ToString());
        //Console.WriteLine("個数:" + this.Controls[index].Controls.Count.ToString());
        if (this.Controls[index].Controls.Count == 2) {
            //押下されたボタンの次のcontrolがBackupDataListViewだった時
            removeListView(sender, e);
        }
        else {
            //押下されたボタンの次のcontrolがBackupDataListViewではなかった時
            addListView(sender, e);
        }
    }

    void addListView(object sender, EventArgs e) {
        var button = (AddInfoButton)sender;
        int index = button.id;
        Console.WriteLine("call:addListView");
        backupDataList = new BackupDataListView(button.World);
        //if (backupDataList.Items.Count == 0) {
        //    Console.WriteLine("info:not backup folder");
        //    return;
        //}
        this.Controls[index].Controls.Add(backupDataList);
        int a = (int)((dualPanel.Height + Margin.Left * 2) + backupDataList.Height);
        int dh = (int)(dualPanel.Height + Margin.Left * 2);
        this.Controls[index].Height = a;
    }

    void removeListView(object sender, EventArgs e) {
        Console.WriteLine("call:removeListView");
        var button = (AddInfoButton)sender;
        int index = button.id;
        this.Controls[index].Controls.Remove(this.Controls[index].Controls[1]);
        this.Controls[index].Height = dualPanel.Height + Margin.Left * 2;
    }
    public static List<string> GetBackupFiles(string worldName, string worldDir) {
        //Console.WriteLine("call:get backup files");
        if (!Directory.Exists(AppConfig.BackupPath + "\\" + worldDir)) {
            Console.WriteLine($"info:{worldDir}ディレクトリのバックアップデータが一切ないのでフォルダを生成します");
            Directory.CreateDirectory(AppConfig.BackupPath + "\\" + worldDir);
        }
        if (!Directory.Exists(AppConfig.BackupPath + "\\" + worldDir + "\\" + worldName)) {
            Console.WriteLine($"info:{worldName}のバックアップデータが一切ないのでフォルダを生成します");
            Directory.CreateDirectory(AppConfig.BackupPath + "\\" + worldDir + "\\" + worldName);
        }
        List<string> folders = new List<string>(Directory.GetDirectories(AppConfig.BackupPath + "\\" + worldDir + "\\" + worldName));
        return folders;
    }
}

class AddInfoButton :Button {
    public int id { get; set; }

    public World World { get; set; }

    public AddInfoButton(string path) {
        //Console.WriteLine("info[DEBUG]:" + path);
        World = Config.configs.Find(x => x.WPath == path);
        Text = "バックアップ一覧";
        Width = (int)Util.FontStyle.Size * 14;
        Height = (int)Util.FontStyle.Size * 3;
    }

}
class BackupDataListView :ListView {
    private ContextMenuStrip cMenu;
    private BackupDataListViewItem selectedItem = null;
    private ColumnHeader clmnBackupTime; // 'バックアップ日時' 列ヘッダ
    private ColumnHeader clmnAffiliationWorldName;  // '所属ワールド名' 列ヘッダ
    private ColumnHeader clmnWorldAffiliationDir;  // '所属ディレクトリ' 列ヘッダ
    public BackupDataListView(World worldObj) {
        FullRowSelect = true;
        MultiSelect = false;
        Anchor = AnchorStyles.Left | AnchorStyles.Right;
        Scrollable = false;
        View = View.Details;
        Margin = new Padding(0);
        BorderStyle = BorderStyle.None;
        Dock = DockStyle.Fill;
        Scrollable = false;
        Sorting = SortOrder.Descending;


        cMenu = new ContextMenuStrip();

        #region contextmenu

        cMenu.Opening += new CancelEventHandler(Menu_Opening);
        ToolStripMenuItem returnBackup = new ToolStripMenuItem() {
            Text = "このバックアップから復元する(&B)"
        };
        returnBackup.Click += new EventHandler(ReturnBackup_Click);
        cMenu.Items.Add(returnBackup);

        ToolStripMenuItem openInExplorer = new ToolStripMenuItem() {
            Text = "エクスプローラーで開く(&X)"
        };

        openInExplorer.Click += new EventHandler(OpenInExplorer_Click);
        if (worldObj.isAlive)
            cMenu.Items.Add(openInExplorer);

        this.ContextMenuStrip = cMenu;
        #endregion

        clmnBackupTime = new ColumnHeader() { Text = "バックアップ日時" };
        clmnAffiliationWorldName = new ColumnHeader() { Text = "バックアップ元" };
        clmnWorldAffiliationDir = new ColumnHeader() { Text = "バックアップ元の所属ディレクトリ" };

        Columns.AddRange(new ColumnHeader[]{
            clmnBackupTime,clmnAffiliationWorldName,clmnWorldAffiliationDir
        });

        foreach (ColumnHeader ch in this.Columns) {
            ch.Width = -2;
        }

        Load(worldObj);
        //Console.WriteLine("list height:"+(int)((Items.Count+1) * (Util.FontStyle.Size + 4) * 2));
        if (Items.Count > 0) {
            Console.WriteLine(Height);
            Height /= 4;
            //var rect = this.GetItemRect(this.Items.Count - 1);
            //Console.WriteLine(rect.X);
            //Console.WriteLine(rect.Height);

            //Height = this.hei;
            Height *= Items.Count + 1;
        }
    }
    void Load(World worldObj) {
        Console.WriteLine("info:" + worldObj.WName + "の一覧以下のパスからロードします");
        Console.WriteLine($"path:{worldObj.WPath}");
        //Console.WriteLine(AppConfig.backupPath);
        try {
            List<string> backupFolders = new List<string>(GetBackups(worldObj));
            Console.WriteLine($"info:backupFolderCount[{backupFolders.Count()}]");
            foreach (string backupFolder in backupFolders) {
                if (Path.GetExtension(backupFolder) == "zip") {
                    backupFolder.Substring(0, backupFolder.Length - 4);
                    Console.WriteLine("a::::::::::::" + backupFolder);
                }
                DateTime time = DateTime.ParseExact((Path.GetFileName(backupFolder)).Substring(0, 12), "yyyyMMddHHmm", null);
                this.Items.Add(new BackupDataListViewItem(new string[] { time.ToString("yyyy-MM-dd HH:mm"), worldObj.WName, worldObj.WDir }, worldObj));
            }
        }
        catch (DirectoryNotFoundException) {
            Console.WriteLine("info:バックアップが存在しません");
        }
    }

    private List<string> GetBackups(World w) {
        return Directory.GetFileSystemEntries(AppConfig.BackupPath + "\\" + w.WDir + "\\" + w.WName).ToList();
    }

    void Menu_Opening(object sender, CancelEventArgs e) {
        Console.WriteLine("call:Menu_Opening");
        Point p = this.PointToClient(Cursor.Position);
        BackupDataListViewItem item = this.HitTest(p).Item as BackupDataListViewItem;
        if (item == null) {
            e.Cancel = true;
        }
        else if (item.Bounds.Contains(p)) {
            ContextMenuStrip menu = sender as ContextMenuStrip;
            if (menu != null) {
                Console.WriteLine(item);
                Console.WriteLine(item.world.WName);
                Console.WriteLine(item.world.isAlive);
                selectedItem = item;
            }
        }
        else {
            e.Cancel = true;
        }
    }

    private void ReturnBackup_Click(object sender, EventArgs e) {
        Console.WriteLine("call:ReturnBackup_Click");
        ContextMenuStrip menu = sender as ContextMenuStrip;
        if (selectedItem != null) {
            Console.WriteLine(selectedItem.world.isAlive);
            if (!selectedItem.world.isAlive) {
                //バックアップ元ワールドが死んでいる場合messageBox出現
                Console.WriteLine("info: 死亡済みワールドのバックアップが選択されました");
                MessageBox.Show("同名の別ワールドがゲームディレクトリ内に存在しています", "Minecraft Auto Backup", buttons: MessageBoxButtons.OK);
            }
            DateTime dt = DateTime.ParseExact(selectedItem.SubItems[0].Text, "yyyy-MM-dd HH:mm", null);
            string fileName = dt.ToString("yyyyMMddHHmm");
            string src;
            if (File.Exists($"{AppConfig.BackupPath}\\{selectedItem.SubItems[2].Text}\\{selectedItem.world.WName}\\{fileName}.zip")) {
                // バックアップがzipだった場合
                src = $"{AppConfig.BackupPath}\\{selectedItem.SubItems[2].Text}\\{selectedItem.world.WName}\\{fileName}.zip";
            }
            else {
                // バックアップがzipじゃなかった場合
                src = $"{AppConfig.BackupPath}\\{selectedItem.SubItems[2].Text}\\{selectedItem.world.WName}\\{fileName}";
            }

            World tar = selectedItem.world;
            RestoreFromBackupForm restoreFrom = new RestoreFromBackupForm(src, tar);
            restoreFrom.Show();
        }
    }

    private void OpenInExplorer_Click(object sender, EventArgs e) {
        ContextMenuStrip menu = sender as ContextMenuStrip;
        System.Diagnostics.Process.Start("EXPLORER.EXE", Util.makePathToWorld(selectedItem.SubItems[1].Text, selectedItem.SubItems[2].Text));
    }

}

#endregion

class BackupDataListViewItem :ListViewItem {
    public BackupDataListViewItem(string[] items, World w) : base(items) {
        world = w;
    }

    public World world { get; set; }
}

class RestoreFromBackupForm :Form {
    FlowLayoutPanel panel;
    Label description;
    CheckBox removeBackup;
    CheckBox dontOverwriting;
    Button doRestore;
    string pathSrc;
    string pathTar;

    /*
    バックアップオプション
    １．バックアップ元を消す
    ２．バックアップ先を上書きしない(別名で新規作成する)
    */
    public RestoreFromBackupForm(string backupSourcePath, World backupTarget) {
        Console.WriteLine($"info: src[{backupSourcePath}]");
        Console.WriteLine($"info: tar[{backupTarget.WPath}]");
        pathSrc = backupSourcePath;
        pathTar = backupTarget.WPath;
        if (!((Directory.Exists(backupSourcePath) || (File.Exists(backupSourcePath))))) {
            Console.WriteLine($"info: normal {Directory.Exists(backupSourcePath)}");
            Console.WriteLine($"info: zip    {File.Exists(backupSourcePath)}");
            Console.WriteLine($"error:バックアップは存在しません");
            return;
        }
        //else if (!Directory.Exists(backupTarget)) {
        //    Console.WriteLine("error:そのワールドデータは存在しません");
        //    return;
        //}
        this.Text = "バックアップから復元します";
        this.Icon = new Icon(".\\Image\\app.ico");
        this.Font = Util.FontStyle;
        this.Padding = new Padding((int)Util.FontStyle.Size * 2);
        this.ClientSize = new Size((int)Util.FontStyle.Size * 30, (int)Util.FontStyle.Size * 35);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;

        panel = new FlowLayoutPanel() {
            Padding = new Padding((int)Util.FontStyle.Size),
            BorderStyle = BorderStyle.FixedSingle,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Dock = DockStyle.Fill,
            BackColor = SystemColors.Window,
            WrapContents = false,
            FlowDirection = FlowDirection.TopDown
        };
        DateTime time;
        try {
            time = DateTime.ParseExact(Path.GetFileName(pathSrc), "yyyyMMddHHmm", null);
        }
        catch (FormatException) {
            //zipファイルの場合FormatExceptionが発生するため
            time = DateTime.ParseExact(Path.GetFileName(pathSrc.Substring(0, pathSrc.Length - 4)), "yyyyMMddHHmm", null);

        }


        Font lFont = (Util.FontStyle);

        description = new Label() {
            Font = new Font(Util.FontStyle.FontFamily, Util.FontStyle.Size + 2),
            Text = time.ToString("yyyy-MM-dd HH:mm") + " のバックアップから復元します ",
            Height = (int)Util.FontStyle.Height * 3 + 4,
            //BackColor = Color.Blue,
            Margin = new Padding(0, 0, 0, 20),
            //AutoSize = true,
        };

        removeBackup = new CheckBox() {
            Text = $"バックアップ元（{Path.GetFileName(pathSrc)}）を削除する",
            Width = this.Width - this.Padding.Left * 2 - this.panel.Padding.Left * 2 - 10,
            Height = (int)Util.FontStyle.Height * 2 + 2,
            //BackColor = Color.Blue,
            //AutoSize = true,
        };

        dontOverwriting = new CheckBox() {
            Text = $"バックアップ先（{Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(pathTar)))}\\saves\\{Path.GetFileName(pathTar)}）を上書きしない\n(別名で新規作成する)",
            Width = this.Width - this.Padding.Left * 2 - this.panel.Padding.Left * 2 - 10,
            Height = (int)Util.FontStyle.Height * 6,
            //BackColor = Color.Blue,
        };

        doRestore = new Button() {
            Text = "バックアップから復元する",
            Height = (int)Util.FontStyle.Size * 5,
            Width = this.Width - this.Padding.Left * 2 - this.panel.Padding.Left * 2 - 10,
            Margin = new Padding(0, 0, 0, 20),
            UseVisualStyleBackColor = true,
            BackColor = SystemColors.Control
        };
        doRestore.Click += new EventHandler(doRestore_Click);

        Console.WriteLine($"info:{pathSrc}を{pathTar}に上書きします");
        panel.Controls.Add(description);
        panel.Controls.Add(removeBackup);
        if (backupTarget.isAlive) {
            //バックアップ先ワールドが生きている場合
            panel.Controls.Add(dontOverwriting);
        }
        panel.Controls.Add(doRestore);
        this.Controls.Add(panel);

        description.Width = this.panel.Width - this.panel.Padding.Left * 2 - 10;
        removeBackup.Width = this.panel.Width - this.panel.Padding.Left * 2 - 10;
        dontOverwriting.Width = this.panel.Width - this.panel.Padding.Left * 2 - 10;
        doRestore.Width = this.panel.Width - this.panel.Padding.Left * 2 - 10;

    }

    void doRestore_Click(object sender, EventArgs e) {
        restoreFromBackup(sender, e);
    }
    private void restoreFromBackup(object sender, EventArgs e) {
        this.Close();
        string src = this.pathSrc;
        string tar = this.pathTar;

        Console.WriteLine($"info: src[{src}]");
        Console.WriteLine($"info: tar[{tar}]");
        if (this.dontOverwriting.Checked) {
            Console.WriteLine("info:restore from backup");
            Console.WriteLine($"option:dont over writing [{this.dontOverwriting.Checked}],remove backup [{this.removeBackup.Checked}]");
            int i = 0;
            bool ok = false;
            while (!ok) {
                i++;
                if (!Directory.Exists($"{tar}({i})"))
                    ok = true;
            }
            FileSystem.CopyDirectory(src, $"{tar}({i})", true);
        }
        else {
            Console.WriteLine($"option:dont over writing [{this.dontOverwriting.Checked}],remove backup [{this.removeBackup.Checked}]");
            FileSystem.CopyDirectory(src, tar, true);
        }

        if (this.removeBackup.Checked) {
            Console.WriteLine("info:元データを消します");
            Directory.Delete(src);
        }
    }
}

class BackGroundTasks :Form {
    private string backupDataPath;
    private NotifyIcon notifyIcon = new NotifyIcon();
    public BackGroundTasks() {
        backupDataPath = AppConfig.BackupPath;
        this.ShowInTaskbar = false;
        this.Icon = new Icon(".\\Image\\app.ico");

        notifyIcon.Icon = new Icon(".\\Image\\app_sub.ico");
        notifyIcon.Visible = true;
        notifyIcon.Text = "ただいましています";
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
}


public class Util {
    public static Task task;

    private static Font fontStyle;

    public static Font FontStyle {
        set { fontStyle = value; }
        get { return fontStyle; }
    }

    public static string TrimDoubleQuotationMarks(string target) {
        return target.Trim(new char[] { '"' });
    }

    public static string makePathToWorld(string name, string dir) {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{dir}\\saves\\{name}";
    }

    //現在存在しているバックアップへのパスをListにして返す
    public static List<string> GetBackups() {
        Console.WriteLine("call: GetBackups");
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
        Console.WriteLine($"dir:{dirs.Count()}, worlds:{worlds.Count()}, backups:{backups.Count()}");
        Console.WriteLine("-----GetBackups-----");
        foreach (var a in backups) {
            Console.WriteLine(a);
        }
        Console.WriteLine("--------------------");
        return backups;
    }

    //渡されたworldの現在存在するバックアップをListにして返す
    public static List<string> GetBackup(World w) {
        List<string> backups = new List<string>();
        if (Directory.Exists($"{AppConfig.BackupPath}\\{w.WDir}\\{w.WName}")) {
            //バックアップフォルダがある場合のみ実行
            backups.AddRange(Directory.GetDirectories($"{AppConfig.BackupPath}\\{w.WDir}\\{w.WName}"));
            backups.AddRange(Directory.GetFiles($"{AppConfig.BackupPath}\\{w.WDir}\\{w.WName}"));
        }
        return backups;
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
    public static List<World> configs = new List<World>();

    public static string configPath = @".\Config\config.txt";

    //datasの中にworldName,worldDirに当てはまる要素があるかどうか
    private static bool IsWorldParticular(string worldName, string worldDir, string[] datas) {
        //Console.WriteLine(datas[1] + ",\"" + worldName + "\"と" + datas[3] + ",\"" + worldDir + "\"");
        return datas[1] == "\"" + worldName + "\"" && datas[3] == "\"" + worldDir + "\"";
    }

    public static List<World> GetConfig() => configs;

    public static void MakeConfig() {
        Console.WriteLine("call:MakeConfig");
        if (!Directory.Exists(Path.GetDirectoryName(configPath))) {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
        }

        string value = $"info:configファイル[{configPath}]生成完了";
        Console.WriteLine(value);
        List<World> worlds = GetWorldDataFromHDD();
        //List<world> worlds = new List<world>();
        // ゲームディレクトリが見つからなかった場合
        if (worlds.Count <= 0) {
            Console.WriteLine("info:ゲームディレクトリが一つも見つかりませんでした");
            DialogResult result = MessageBox.Show(
                "minecraftのゲームディレクトリが見つかりませんでした。手動で設定しますか？",
                "ゲームディレクトリが見つかりませんでした",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Error
                );
            if (result == DialogResult.OK) {
                CommonOpenFileDialog copd = new CommonOpenFileDialog();
                copd.Title = "ゲームディレクトリを選択してください（複数選択可）";
                copd.IsFolderPicker = true;
                copd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                copd.Multiselect = true;
                if (copd.ShowDialog() == CommonFileDialogResult.Ok) {
                    worlds.AddRange(GetWorldDataFromHDD(copd.FileNames.ToList()));
                }
            }
            else if (result == DialogResult.No) {

            }
        }
        foreach (var world in worlds) {
            Console.WriteLine($"info:world[{world.WName}]を発見しました");
            configs.Add(world);
            Write();
        }
    }

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
                configs.Add(new World(datas[2], Convert.ToBoolean(datas[0]), Convert.ToBoolean(datas[4])));
            }
            Console.WriteLine($"info:Configから{configs.Count()}件のワールドを読み込みました");
        }

    }

    /// <summary>
    /// configsをConfig.txtに上書きする
    /// </summary>
    public static void Write() {
        List<string> text = new List<string>();
        foreach (World config in configs) {
            text.Add($"\"{config.WDoBackup}\",\"{config.WName}\",\"{config.WPath}\",\"{config.WDir}\",\"{config.isAlive}\"\n");
        }
        File.WriteAllText(configPath, string.Join("", text), Encoding.GetEncoding("utf-8"));
    }


    /// <summary>
    /// Configファイルを更新する
    /// </summary>
    public static List<World> ReloadConfig() {
        Console.WriteLine("call:reloadConfig");
        List<World> worldInHdd = GetWorldDataFromHDD();
        List<World> worldInConfig = GetConfig();
        Console.WriteLine($"config: {worldInConfig.Count()}");
        Console.WriteLine($"HDD   : {worldInHdd.Count()}");

        int i = 0;
        //configに存在しないpathをconfigに追加する
        foreach (World pc in worldInHdd) {
            Console.WriteLine($"pc:{i}回目");
            //dobackup以外を比較して判定
            //List<WorldForComparison> _comp = worldInConfig.Select(x => new WorldForComparison(x)).ToList();
            if (!worldInConfig.Select(x => $"{x.WPath}_{x.isAlive}").ToList().Contains($"{pc.WPath}_{pc.isAlive}")) {
                Console.WriteLine($"info:ADD {pc.WName}");
                configs.Add(pc);
            }
            i++;
        }
        List<World> removeWorlds = new List<World>();
        Console.WriteLine($"config: {worldInConfig.Count()}");
        Console.WriteLine($"HDD   : {worldInHdd.Count()}");

        i = 0;
        //configに存在するがhddに存在しない(削除されたワールド)pathをconfigで死亡扱いにする
        //isAliveプロパティを追加したので、そちらで管理
        int wI = 0;
        //Console.WriteLine("-----config一覧-----");
        //foreach(var a in worldInHdd.Select(x => new WorldForComparison(x)).ToList()) {
        //    Console.WriteLine($"pc : {a.path}/{a.isAlive.ToString()}");
        //}
        //Console.WriteLine("--------------------");
        foreach (World world in worldInConfig) {
            WorldForComparison cf = new WorldForComparison(world);
            Console.WriteLine($"config:{i}回目");
            //dobackup以外を比較して判定
            if (!worldInHdd.Select(x => $"{x.WPath}_{x.isAlive}").ToList().Contains($"{world.WPath}_{world.isAlive}")) {
                //config内のworldがHDDになかった場合
                if (Util.GetBackup(world).Count() == 0) {
                    // バックアップが一つもない場合はconfigから削除
                    Console.WriteLine($"info:バックアップが一つもないのでRemoveWorldsに{world.WName}を追加");
                    removeWorlds.Add(world);
                }
                else {
                    if (world.isAlive) {
                        //バックアップが一つでもある場合は、backup一覧に表示するために殺すだけにする
                        Console.WriteLine($"info:{world.WName}のバックアップが残っているため殺害");
                        Config.configs[wI].isAlive = false;
                        int count = 1;
                        while (Directory.Exists($"{AppConfig.BackupPath}\\{Config.configs[wI].WDir}\\{Config.configs[wI].WName}_(削除済み)_{count}")) {
                            Console.WriteLine($"info: path[ {AppConfig.BackupPath}\\{Config.configs[wI].WDir}\\{Config.configs[wI].WName}_(削除済み)_{count} ]");
                            count++;
                        }

                        Directory.Move($"{AppConfig.BackupPath}\\{ Config.configs[wI].WDir}\\{ Config.configs[wI].WName}",
                            $"{AppConfig.BackupPath}\\{ Config.configs[wI].WDir}\\{ Config.configs[wI].WName}_(削除済み)_{count}");
                        Config.configs[wI].WPath += "_(削除済み)_" + count;
                        Config.configs[wI].WName += "_(削除済み)_" + count;
                    }
                }
            }
            wI++;
            i++;
        }

        Console.WriteLine($"config: {worldInConfig.Count()}");
        Console.WriteLine($"HDD   : {worldInHdd.Count()}");

        foreach (World w in removeWorlds) {
            if (configs.Remove(w)) {
                Console.WriteLine($"info:REMOVE {w.WName} suc");
            }
            else {
                Console.WriteLine($"info:REMOVE {w.WName} 見つかりませんでした");
            }
        }

        Write();

        Console.WriteLine($"config: {worldInConfig.Count()}");
        Console.WriteLine($"HDD   : {worldInHdd.Count()}");

        return removeWorlds;
    }

    public static void Change(string worldName, string worldDir, string doBackup) {
        Console.WriteLine("call:Change");
        Console.WriteLine("info:GET  worldName: " + worldName + ",  worldDir: " + worldDir + ",  dobackup: " + doBackup);
        List<World> _configs = new List<World>();
        foreach (World config in configs) {
            if (config.WName == worldName && config.WDir == worldDir) {
                config.WDoBackup = bool.Parse(doBackup);
                _configs.Add(new World(config.WPath, Convert.ToBoolean(doBackup), config.isAlive));
            }
            else {
                _configs.Add(new World(config.WPath, config.WDoBackup, config.isAlive));
            }
        }
        configs = _configs;
        //ConsoleConfig();
    }

    /// <summary>
    /// PCからワールドデータ一覧を取得
    /// </summary>
    /// <returns>取得したList<world></returns>
    private static List<World> GetWorldDataFromHDD() {
        Console.WriteLine("call:GetWorldDataFromPC");
        List<World> worlds = new List<World>();
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
                worlds.Add(new World(Util.TrimDoubleQuotationMarks(worldPath)));
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
    /// <param name="gameDirectory"></param>
    /// <returns>取得したList<world></returns>
    private static List<World> GetWorldDataFromHDD(List<string> gameDirectory) {
        List<World> worlds = new List<World>();
        Console.WriteLine("call:GetWorldDataFromPC");
        foreach (string dir in gameDirectory) {
            if (Directory.Exists($"{dir}\\saves")) {
                List<string> _worlds = Directory.GetDirectories($"{dir}\\saves").ToList();
                foreach (string worldPath in _worlds) {
                    worlds.Add(new World(Util.TrimDoubleQuotationMarks(worldPath)));
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
        foreach (World w in configs) {
            Console.WriteLine($"[{w.WDoBackup},{w.WName},{w.WPath},{w.WDir},]");
        }
        Console.WriteLine("---------------");
    }
    /// <summary>
    /// ワールドのバックアップソースが生きているかどうか
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static bool isBackupAlive(World w) {
        if (w.isAlive) {
            //Console.WriteLine("info[DEBUG]:バックアップは死んでいます");
            return false;
        }
        else {
            //Console.WriteLine("info[DEBUG]:バックアップは生きています");
            return true;
        }
    }
}

//ちょっとFormの書き方変えてみたやつ
//結局メリットはよくわからなかった
internal class AppConfigForm :Form {

    TabControl tab = new TabControl();
    TabPage backupTab = new TabPage();
    TabPage fontTab = new TabPage();

    FlowLayoutPanel backupTabF = new FlowLayoutPanel();
    Label backupPath = new Label();
    FlowLayoutPanel backupPathPanel = new FlowLayoutPanel();
    TextBox backupPathInput = new TextBox();
    Button refe = new Button();
    CheckBox doZip = new CheckBox();
    FlowLayoutPanel backupCountPanel = new FlowLayoutPanel();
    Label backupCountText = new Label();
    ComboBox backupCount = new ComboBox();


    FlowLayoutPanel fontTabF = new FlowLayoutPanel();
    Label fontName = new Label();
    Button fontChange = new Button();

    FlowLayoutPanel okCanselFlowPanel = new FlowLayoutPanel();
    Button ok = new Button();
    Button cansel = new Button();


    public AppConfigForm() {

        //controls追加
        backupPathPanel.Controls.AddRange(new Control[] { backupPathInput, refe });
        backupCountPanel.Controls.AddRange(new Control[] { backupCountText, backupCount });
        backupTabF.Controls.AddRange(new Control[] { backupPath, backupPathPanel, doZip, backupCountPanel });
        backupTab.Controls.Add(backupTabF);
        fontTabF.Controls.AddRange(new Control[] { fontName, fontChange });
        fontTab.Controls.Add(fontTabF);
        tab.Controls.AddRange(new Control[] { backupTab, fontTab });
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
        okCanselFlowPanel.Dock = DockStyle.Bottom;
        okCanselFlowPanel.Height = 40;
        okCanselFlowPanel.FlowDirection = FlowDirection.RightToLeft;

        backupTab.Text = AppConfig.Language == "ja" ? "バックアップ" : "backup";
        fontTab.Text = AppConfig.Language == "ja" ? "フォント" : "font";

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
        Console.WriteLine($"info:dozip[{AppConfig.DoZip}]");
        //doZip.BackColor = Color.Blue;

        fontName.Text = $"フォント名 :  {Util.FontStyle.Name}";
        fontName.AutoSize = true;

        fontChange.Text = "フォントを変更する";
        fontChange.AutoSize = true;
        fontChange.BackColor = SystemColors.Control;
        fontChange.Click += new EventHandler(fontChange_Click);

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
                //設定上はtrue,formのほうはfalseの場合（falseに変更された場合）
                //List<string> backups = Util.GetBackups();
                //FileSystem.CreateDirectory(".\\tmp");
                //var t = Task.Run(() => {
                //    try{ FileSystem.CopyDirectory(AppConfig.BackupPath, ".\\tmp"); }
                //    catch( Exception ex) {
                //        Console.WriteLine("catch error");
                //        Console.WriteLine(ex.StackTrace);
                //        Console.WriteLine(ex.Message);
                //        Console.WriteLine(ex.Data);
                //    }
                //});

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
                    Console.WriteLine($"Zipper {command} {_args}");
                    Process.Start(Decompression);

                }
            }
            else if (!AppConfig.DoZip) {
                //設定上はfalse,formのほうはtrueの場合（trueに変更された場合）
                List<string> backups = Util.GetBackups();
                FileSystem.CreateDirectory(".\\tmp");
                var t = Task.Run(() => {
                    try { FileSystem.CopyDirectory(AppConfig.BackupPath, ".\\tmp"); }
                    catch (Exception ex) {
                        Console.WriteLine("catch error");
                        Console.WriteLine(ex.StackTrace);
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.Data);
                    }
                });
                foreach (string b in backups) {
                    b.Replace(AppConfig.BackupPath, ".\\tmp");
                    Console.WriteLine($"info:{b}");
                }

                if (backups.Count > 0) {
                    //バックアップが存在している場合
                    DialogResult r = MessageBox.Show("現在保存されているバックアップをすべて圧縮しますか？", "保存方式", MessageBoxButtons.YesNo);
                    //if (r == DialogResult.Yes) {
                    //    // 既存のバックアップ.zipをすべて解凍する
                    //    string command = ".\\SubModule\\Zipper.exe";
                    //    string _args = "0 ";
                    //    foreach (string s in backups) {
                    //        _args += $"\"{s}\" ";
                    //    }
                    //    var doZipping = new ProcessStartInfo {
                    //        FileName = command,
                    //        Arguments = _args
                    //    };
                    //    Console.WriteLine($"Zipper {command} {_args}");
                    //    Process.Start(doZipping);
                    //    //Util.task = Task.Run(() => { DoZipping(backups); });
                    //}
                }
            }
            else {
                throw new Exception();
            }
        }
        AppConfig.DoZip = this.doZip.Checked;

        AppConfig.BackupCount = this.backupCount.SelectedItem.ToString();
        Console.WriteLine($"info:selectedItem[{this.backupCount.SelectedItem.ToString()}]");
        AppConfig.WriteAppConfig();
        this.Close();
    }
    private void cansel_Click(object sender, EventArgs e) {
        this.Close();
    }
    //private void DoZipping(List<string> pasess) {
    //    Console.WriteLine("call:DoZipping");
    //    Console.WriteLine($"{pasess.Count()}件のバックアップを検討します");
    //    foreach (string path in pasess) {
    //        if (!path.Contains(".zip")) {
    //            //バックアップがzipじゃない場合
    //            Console.WriteLine($"info: [{path}]のスレッド開始");
    //            try { ZipFile.CreateFromDirectory(path, $"{path}.zip"); }
    //            catch (IOException) {
    //                Console.WriteLine($"{path}: zipping io exception");
    //                continue;
    //            }
    //            Console.WriteLine($" info: [{path}]zip化完了");
    //            try { Directory.Delete(path, true); }
    //            catch (IOException) {
    //                Console.WriteLine($"{path}: deleting io exception");
    //            }
    //            Console.WriteLine($" info: [{path}]削除完了");
    //            //Console.WriteLine($"{Path.GetDirectoryName(backupPath)}\\{Path.GetFileName(backupPath)}をzipにします");
    //        }
    //    }
    //}

    //private void Decompression(List<string> pasess) {
    //    Console.WriteLine("call:Decompression");
    //    Console.WriteLine($"{pasess.Count()}件のバックアップを検討します");
    //    foreach (var path in pasess) {
    //        if (path.Contains(".zip")) {
    //            //バックアップがzipの場合
    //            Console.WriteLine($"{path}を解凍します");
    //            try { ZipFile.ExtractToDirectory($"{path}", path.Substring(0,path.Length - 4)); }
    //            catch (IOException) { Console.WriteLine($"{path}: extracting io exception"); }
    //            try{ File.Delete($"{path}"); }
    //            catch (IOException) { Console.WriteLine($"{path}: deleting io exception"); }
    //            //Console.WriteLine($"{Path.GetDirectoryName(backupPath)}\\{Path.GetFileName(backupPath)}をzipにします");
    //        }
    //    }
    //    Console.WriteLine("解凍完了");
    //}
}

public class WorldForComparison {
    public string path { get; set; }
    public bool isAlive { get; set; }

    public WorldForComparison(World w) {
        path = w.WPath;
        isAlive = w.isAlive;
    }
}
