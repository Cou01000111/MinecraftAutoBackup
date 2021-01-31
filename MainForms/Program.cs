using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
class Program {
    [STAThread]
    static void Main() {
        //起動時処理
        Console.WriteLine("-----MakeConfig-------");

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
        new AppConfig();
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
        this.Font = new Font(AppConfig.font.Name, 11);

        Util.FontStyle = AppConfig.font;
        this.FormClosing += new FormClosingEventHandler(worldListForm_FormClosing);
        this.Resize += new EventHandler(form_Resize);
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
        configMenu.Click += new EventHandler(config_Click);
        fileMenu.DropDownItems.Add(configMenu);

        ToolStripMenuItem exitMenu = new ToolStripMenuItem() {
            Text = "終了(&X)"
        };
        exitMenu.Click += new EventHandler(exit_Click);
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
        ok.Click += new EventHandler(ok_Click);

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
        cancel.Click += new EventHandler(cancel_Click);

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
        this.ClientSize = AppConfig.clientSize;
        this.Location = AppConfig.clientPoint;
    }

    void form_Resize(object sender, EventArgs e) {
        //Console.WriteLine("大きさが変更されました大きさが変更されました");
        int i = 0;
        foreach (Control a in this.backupDataTable.Controls) {
            this.backupDataTable.Controls[i].Width = this.Width - 60;
            //this.backupDataTable.Controls[i].Controls[0].Width = this.Width - 60;
            i++;
        }
    }
    void ok_Click(object sender, EventArgs e) {
        Console.WriteLine("info:push ok");
        List<string[]> configs = worldListView.GetWorldListView();
        foreach (var config in configs) {
            Console.WriteLine($"{config[0]}, {config[1]}, {config[2]}");
            Config.Change(config[1], config[2], config[0]);
        }
        Config.Write();
        this.Close();
        //Application.Exit();
    }
    void cancel_Click(object sender, EventArgs e) {
        string message = "設定を保存しないで終了しますか？";
        string caption = "保存できてないよ？";
        MessageBoxButtons buttons = MessageBoxButtons.YesNo;
        DialogResult result = MessageBox.Show(this, message, caption, buttons, MessageBoxIcon.Question);
        if (result == DialogResult.Yes) {
            this.Close();
        }
    }
    void worldListForm_FormClosing(object sender, CancelEventArgs e) {
        Console.WriteLine("info:終了時処理を開始します");
        AppConfig.clientPoint = this.Location;
        AppConfig.clientSize = this.ClientSize;
        AppConfig.WriteAppConfig();
        try {
            System.Diagnostics.Process.Start(".\\SubModule\\MABProcessAtWait.exe");
        }
        catch (Win32Exception w) {
            Console.WriteLine(w.Message);
            Console.WriteLine(w.ErrorCode.ToString());
            Console.WriteLine(w.NativeErrorCode.ToString());
            Console.WriteLine(w.StackTrace);
            Console.WriteLine(w.Source);
            Exception f = w.GetBaseException();
            Console.WriteLine(f.Message);
        }
    }
    void exit_Click(object sender, EventArgs e) {
        Console.WriteLine("info:push exit");
        this.Close();
    }
    void config_Click(object sender, EventArgs e) {
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
        List<world> listDatas = Config.GetConfig();
        int iItemCount = 0;
        foreach (var datas in listDatas) {
            this.Items.Add(new ListViewItem(new string[] { " ", datas.WName, datas.WDir }));
            Console.WriteLine(datas.WDoBackup);
            this.Items[iItemCount].Checked = Convert.ToBoolean(datas.WDoBackup);
            iItemCount++;
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
        if (Config.GetConfig().Count() == 0) {
            Console.WriteLine("info:バックアップが存在しません");
            Label notBackupFile = new Label() {
                Text = "バックアップが存在しません",
            };
            this.Controls.Add(notBackupFile);
        }
        else {
            foreach (world world in Config.GetConfig()) {

                if (GetBackupFiles(world.WName, world.WDir).Count() <= 0) {
                    continue;
                }

                backupDataDir = new Label() {
                    Text = world.WName + "/" + world.WDir + "",
                    AutoSize = true,
                    Height = (int)Util.FontStyle.Size * 2,
                    Margin = new Padding((int)Util.FontStyle.Size),
                    //BackColor = Color.Yellow
                };
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
        backupDataList = new BackupDataListView(new string[] { button.World.WName, button.World.WPath, button.World.WDir });
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
        if (!Directory.Exists(AppConfig.backupPath + "\\" + worldDir)) {
            Console.WriteLine($"info:{worldDir}ディレクトリのバックアップデータが一切ないのでフォルダを生成します");
            Directory.CreateDirectory(AppConfig.backupPath + "\\" + worldDir);
        }
        if (!Directory.Exists(AppConfig.backupPath + "\\" + worldDir + "\\" + worldName)) {
            Console.WriteLine($"info:{worldName}のバックアップデータが一切ないのでフォルダを生成します");
            Directory.CreateDirectory(AppConfig.backupPath + "\\" + worldDir + "\\" + worldName);
        }
        List<string> folders = new List<string>(Directory.GetDirectories(AppConfig.backupPath + "\\" + worldDir + "\\" + worldName));
        return folders;
    }
}

class AddInfoButton :Button {
    public int id { get; set; }

    public world World { get; set; }

    public AddInfoButton(string path) {
        Console.WriteLine("info[DEBUG]:" + path);
        World = new world(Util.TrimDoubleQuotationMarks(path));
        Text = "バックアップ一覧";
        Width = (int)Util.FontStyle.Size * 14;
        Height = (int)Util.FontStyle.Size * 3;
    }

}
class BackupDataListView :ListView {
    private ContextMenuStrip cMenu;
    ListViewItem selectedItem = null;
    private ColumnHeader clmnBackupTime; // 'バックアップ日時' 列ヘッダ
    private ColumnHeader clmnAffiliationWorldName;  // '所属ワールド名' 列ヘッダ
    private ColumnHeader clmnWorldAffiliationDir;  // '所属ディレクトリ' 列ヘッダ
    public BackupDataListView(string[] worldObj) {
        FullRowSelect = true;
        MultiSelect = false;
        Anchor = AnchorStyles.Left | AnchorStyles.Right;
        Scrollable = true;
        View = View.Details;
        Margin = new Padding(0);
        BorderStyle = BorderStyle.None;
        Dock = DockStyle.Fill;
        Scrollable = false;

        cMenu = new ContextMenuStrip();

        #region contextmenu

        cMenu.Opening += new CancelEventHandler(menu_Opening);
        ToolStripMenuItem returnBackup = new ToolStripMenuItem() {
            Text = "このバックアップから復元する(&B)"
        };
        returnBackup.Click += new EventHandler(ReturnBackup_Click);
        cMenu.Items.Add(returnBackup);

        ToolStripMenuItem openInExplorer = new ToolStripMenuItem() {
            Text = "エクスプローラーで開く(&X)"
        };
        openInExplorer.Click += new EventHandler(OpenInExplorer_Click);
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
            ch.Width = -1;
        }

        Load(worldObj);
        //Console.WriteLine("list height:"+(int)((Items.Count+1) * (Util.FontStyle.Size + 4) * 2));
        Height = (int)((Items.Count + 1) * (Util.FontStyle.Size + 4) * 2);
    }
    void Load(string[] worldObj) {
        Console.WriteLine("info:" + worldObj[0] + "の一覧をロードします");
        //Console.WriteLine(AppConfig.backupPath);
        List<string> backupFolders = new List<string>(Directory.GetDirectories(AppConfig.backupPath + "\\" + worldObj[2] + "\\" + worldObj[0]));

        foreach (string backupFolder in backupFolders) {
            DateTime time = DateTime.ParseExact(Path.GetFileName(backupFolder), "yyyyMMddHHmm", null);
            this.Items.Add(new ListViewItem(new string[] { time.ToString("yyyy-MM-dd HH:mm"), worldObj[0], worldObj[2] }));

        }
    }

    void menu_Opening(object sender, CancelEventArgs e) {
        Point p = this.PointToClient(Cursor.Position);
        ListViewItem item = this.HitTest(p).Item;
        if (item == null) {
            e.Cancel = true;
        }
        else if (item.Bounds.Contains(p)) {
            ContextMenuStrip menu = sender as ContextMenuStrip;
            if (menu != null) {
                selectedItem = item;
            }
        }
        else {
            e.Cancel = true;
        }
    }

    private void ReturnBackup_Click(object sender, EventArgs e) {
        ContextMenuStrip menu = sender as ContextMenuStrip;
        if (selectedItem != null) {
            DateTime dt = DateTime.ParseExact(selectedItem.SubItems[0].Text, "yyyy-MM-dd HH:mm", null);
            string fileName = dt.ToString("yyyyMMddHHmm");
            string src = $"{AppConfig.backupPath}\\{selectedItem.SubItems[2].Text}\\{selectedItem.SubItems[1].Text}\\{fileName}";
            string tar = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{selectedItem.SubItems[2].Text}\\saves\\{selectedItem.SubItems[1].Text}";
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
    public RestoreFromBackupForm(string backupSourcePath, string backupTarget) {
        Console.WriteLine($"info: src[{backupSourcePath}]");
        Console.WriteLine($"info: tar[{backupTarget}]");
        pathSrc = backupSourcePath;
        pathTar = backupTarget;
        if (!Directory.Exists(backupSourcePath)) {
            Console.WriteLine($"error:バックアップは存在しません");
            return;
        }
        else if (!Directory.Exists(backupTarget)) {
            Console.WriteLine("error:そのワールドデータは存在しません");
            return;
        }
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
        DateTime time = DateTime.ParseExact(Path.GetFileName(backupSourcePath), "yyyyMMddHHmm", null);
        ;

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
            Text = $"バックアップ元（{Path.GetFileName(backupSourcePath)}）を削除する",
            Width = this.Width - this.Padding.Left * 2 - this.panel.Padding.Left * 2 - 10,
            Height = (int)Util.FontStyle.Height * 2 + 2,
            //BackColor = Color.Blue,
            //AutoSize = true,
        };

        Console.WriteLine("a" + Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(backupTarget))));
        Console.WriteLine("b" + Path.GetFileName(backupTarget));
        dontOverwriting = new CheckBox() {
            Text = $"バックアップ先（{Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(backupTarget)))}\\saves\\{Path.GetFileName(backupTarget)}）を上書きしない\n(別名で新規作成する)",
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

        Console.WriteLine($"info:{backupSourcePath}を{backupTarget}に上書きします");
        panel.Controls.Add(description);
        panel.Controls.Add(removeBackup);
        panel.Controls.Add(dontOverwriting);
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


public class Util {
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
        Console.WriteLine(datas[1] + ",\"" + worldName + "\"と" + datas[3] + ",\"" + worldDir + "\"");
        return datas[1] == "\"" + worldName + "\"" && datas[3] == "\"" + worldDir + "\"";
    }

    public static List<world> GetConfig() => configs;

    public static void MakeConfig() {
        Console.WriteLine("call:MakeConfig");
        if (!Directory.Exists(Path.GetDirectoryName(configPath))) {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
        }
        Console.WriteLine($"info:configファイル[{configPath}]生成完了");
        List<world> worlds = GetWorldDataFromHDD();
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
    /// </summary>
    public static void ReloadConfig() {
        Console.WriteLine("call:reloadConfig");
        List<world> worldInPc = GetWorldDataFromHDD();
        List<world> worldInConfig = GetConfig();
        Console.WriteLine(worldInConfig.Count());
        Console.WriteLine(worldInPc.Count());
        //configに存在しないpathを追加する
        foreach (world pc in worldInPc) {
            if (!worldInConfig.Select(x => x.WPath).ToList().Contains(pc.WPath)) {
                Console.WriteLine($"info:ADD {pc.WName}");
                configs.Add(pc);
            }
        }
        //削除されたワールドをconfigから消す
        foreach (world world in worldInConfig) {
            if (!worldInPc.Select(x => x.WPath).ToList().Contains(world.WPath)) {
                Console.WriteLine($"info:REMOVE {world.WName}");
                //configs.Remove(world);
            }
        }
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
        ConsoleConfig();
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
                Console.WriteLine($"info:ゲームディレクトリ[{dir}]を発見しました");
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
            doZip = datas[2] == "zip" ? true : false;
            language = datas[3];
            clientSize = new Size(int.Parse(datas[4]), int.Parse(datas[5]));
            clientPoint = new Point(int.Parse(datas[6]), int.Parse(datas[7]));
        }
    }

    public static void WriteAppConfig() {
        string Text =
            $"{backupPath}\n{font.Name}\n" +
            (doZip ? "zip" : "normal") +
            $"\n{language}\n{clientSize.Width}\n{clientSize.Height}\n{clientPoint.X}\n{clientPoint.Y}";
        File.WriteAllText(appConfigPath, Text);
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

    FlowLayoutPanel fontTabF = new FlowLayoutPanel();
    Label fontName = new Label();
    Button fontChange = new Button();

    FlowLayoutPanel okCanselFlowPanel = new FlowLayoutPanel();
    Button ok = new Button();
    Button cansel = new Button();


    public AppConfigForm() {

        //controls追加
        backupPathPanel.Controls.AddRange(new Control[] { backupPathInput, refe });
        backupTabF.Controls.AddRange(new Control[] { backupPath, backupPathPanel, doZip });
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
        Font = new Font(AppConfig.font.Name, 10);
        Padding = new Padding(8);

        //各種コントロール設定

        tab.Dock = DockStyle.Fill;
        tab.Location = new Point(10, 10);
        backupTab.BackColor = SystemColors.Window;
        fontTab.BackColor = SystemColors.Window;
        okCanselFlowPanel.Dock = DockStyle.Bottom;
        okCanselFlowPanel.Height = 40;
        okCanselFlowPanel.FlowDirection = FlowDirection.RightToLeft;

        backupTab.Text = AppConfig.language == "ja" ? "バックアップ" : "backup";
        fontTab.Text = AppConfig.language == "ja" ? "フォント" : "font";

        backupPath.Text = "バックアップの保存先";
        backupPath.AutoSize = true;

        backupPathPanel.FlowDirection = FlowDirection.LeftToRight;
        backupPathPanel.Width = 480;
        backupPathPanel.Height = 32;
        //backupPathPanel.BackColor = Color.Blue;

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

        backupPathInput.Text = AppConfig.backupPath;
        backupPathInput.Width = 400;
        backupPathInput.Margin = new Padding(5);
        //backupPathInput.BackColor = Color.Red;

        refe.Text = "...";
        refe.Width = 32;
        refe.Height = 28;
        refe.BackColor = SystemColors.Control;
        refe.Click += new EventHandler(refe_Click);

        doZip.Text = "バックアップデータをZip圧縮する";
        doZip.AutoSize = true;
        doZip.Checked = AppConfig.doZip;
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
            Util.FontStyle = fontDialog.Font;
        }
    }

    private void ok_Click(object sender, EventArgs e) {
        AppConfig.backupPath = this.backupPathInput.Text;
        AppConfig.doZip = this.doZip.Checked;
        AppConfig.WriteAppConfig();
        this.Close();
    }
    private void cansel_Click(object sender, EventArgs e) {
        this.Close();
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
