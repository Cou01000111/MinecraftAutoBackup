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

    private Logger logger = new Logger("MainForm",3);

    public WorldListForm() {
        //Form設定
        Text = "Minecraft Auto Backup";
        Icon = new Icon(".\\Image\\app.ico");
        Font = new Font(AppConfig.Font.Name, 11);
        logger.Debug(AppConfig.Font.Name);

        Util.FontStyle = AppConfig.Font;
        FormClosing += new FormClosingEventHandler(WorldListForm_FormClosing);
        Resize += new EventHandler(Form_Resize);
        yesColor = Color.FromArgb(15, 27, 51);
        cancelColor = Color.FromArgb(147, 31, 31);


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
            Font = Font,
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

        //Controls.Add(menu);[

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

        BackColor = SystemColors.Window;

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

        Controls.Add(tabControl);
        Controls.Add(menu);
        int i = 0;
        foreach (Control a in backupDataTable.Controls) {
            backupDataTable.Controls[i].Width = Width - 60;
            //backupDataTable.Controls[i].Controls[0].Width = Width - 60;
            i++;
        }
        Load += new EventHandler(Form_Load);
    }

    private void Form_Load(object sender, EventArgs e) {
        ClientSize = AppConfig.ClientSize;
        Location = AppConfig.ClientPoint;
    }

    private void Form_Resize(object sender, EventArgs e) {
        //logger.Info("大きさが変更されました大きさが変更されました");
        int i = 0;
        foreach (Control a in backupDataTable.Controls) {
            backupDataTable.Controls[i].Width = Width - 60;
            //backupDataTable.Controls[i].Controls[0].Width = Width - 60;
            i++;
        }
    }
    private void Ok_Click(object sender, EventArgs e) {
        logger.Info("push ok");
        List<string[]> configs = worldListView.GetWorldListView();
        foreach (var config in configs) {
            //logger.Info($"{config[0]}, {config[1]}, {config[2]}");
            Config.Change(config[1], config[2], config[0]);
        }
        Config.Write();
        Close();
        //Application.Exit();
    }
    private void Cancel_Click(object sender, EventArgs e) {
        string message = "設定を保存しないで終了しますか？";
        string caption = "保存できてないよ？";
        MessageBoxButtons buttons = MessageBoxButtons.YesNo;
        DialogResult result = MessageBox.Show(this, message, caption, buttons, MessageBoxIcon.Question);
        if (result == DialogResult.Yes) {
            Close();
        }
    }
    private void WorldListForm_FormClosing(object sender, CancelEventArgs e) {
        logger.Info("終了時処理を開始します");

        AppConfig.ClientPoint = Location;
        AppConfig.ClientSize = ClientSize;
        AppConfig.WriteAppConfig();
        try {
            System.Diagnostics.Process.Start(".\\SubModule\\MABProcessAtWait.exe");
        }
        catch (Win32Exception w) {
            logger.Error(w.Message);
            logger.Error(w.ErrorCode.ToString());
            logger.Error(w.NativeErrorCode.ToString());
            logger.Error(w.StackTrace);
            logger.Error(w.Source);
            Exception f = w.GetBaseException();
            logger.Error(f.Message);
        }
    }
    private void Exit_Click(object sender, EventArgs e) {
        logger.Info("push exit");
        Close();
    }
    private void Config_Click(object sender, EventArgs e) {
        AppConfigForm appConfigForm = new AppConfigForm();
        appConfigForm.Owner = this;
        appConfigForm.ShowDialog();
    }
}
