using System;
using System.Drawing;
using System.Windows.Forms;

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
        Logger.Info("アプリケーションが終了しました");
        notifyIcon.Visible = false;
        notifyIcon.Dispose();
        Application.Exit();
    }
}
