using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

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
        Logger.Debug(AppConfig.Font.Name);

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
        //Logger.Info("大きさが変更されました大きさが変更されました");
        int i = 0;
        foreach (Control a in this.backupDataTable.Controls) {
            this.backupDataTable.Controls[i].Width = this.Width - 60;
            //this.backupDataTable.Controls[i].Controls[0].Width = this.Width - 60;
            i++;
        }
    }
    void Ok_Click(object sender, EventArgs e) {
        Logger.Info("push ok");
        List<string[]> configs = worldListView.GetWorldListView();
        foreach (var config in configs) {
            //Logger.Info($"{config[0]}, {config[1]}, {config[2]}");
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
        Logger.Info("終了時処理を開始します");

        AppConfig.ClientPoint = this.Location;
        AppConfig.ClientSize = this.ClientSize;
        AppConfig.WriteAppConfig();
        try {
            System.Diagnostics.Process.Start(".\\SubModule\\MABProcessAtWait.exe");
        }
        catch (Win32Exception w) {
            Logger.Error(w.Message);
            Logger.Error(w.ErrorCode.ToString());
            Logger.Error(w.NativeErrorCode.ToString());
            Logger.Error(w.StackTrace);
            Logger.Error(w.Source);
            Exception f = w.GetBaseException();
            Logger.Error(f.Message);
        }
    }
    void Exit_Click(object sender, EventArgs e) {
        Logger.Info("push exit");
        this.Close();
    }
    void Config_Click(object sender, EventArgs e) {
        AppConfigForm appConfigForm = new AppConfigForm();
        appConfigForm.Owner = this;
        appConfigForm.Show();
    }
}
