using Microsoft.VisualBasic.FileIO;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

class RestoreFromBackupForm :Form {
    private FlowLayoutPanel panel;
    private Label description;
    private CheckBox removeBackup;
    private CheckBox dontOverwriting;
    private Button doRestore;
    private string pathSrc;
    private string pathTar;

    /*
    バックアップオプション
    １．バックアップ元を消す
    ２．バックアップ先を上書きしない(別名で新規作成する)
    */
    public RestoreFromBackupForm(string backupSourcePath, World backupTarget) {
        Logger.Info($"src[{backupSourcePath}]");
        Logger.Info($"tar[{backupTarget.WPath}]");
        pathSrc = backupSourcePath;
        pathTar = backupTarget.WPath;
        if (!((Directory.Exists(backupSourcePath) || (File.Exists(backupSourcePath))))) {
            Logger.Error($"normal {Directory.Exists(backupSourcePath)}");
            Logger.Error($"zip    {File.Exists(backupSourcePath)}");
            Logger.Error($"バックアップは存在しません");
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

        Logger.Info($"{pathSrc}を{pathTar}に上書きします");
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

    private void doRestore_Click(object sender, EventArgs e) {
        restoreFromBackup(sender, e);
    }
    private void restoreFromBackup(object sender, EventArgs e) {
        this.Close();
        string src = this.pathSrc;
        string tar = this.pathTar;

        Logger.Info($" src[{src}]");
        Logger.Info($" tar[{tar}]");
        if (this.dontOverwriting.Checked) {
            Logger.Info("restore from backup");
            Logger.Info($"option:dont over writing [{this.dontOverwriting.Checked}],remove backup [{this.removeBackup.Checked}]");
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
            Logger.Info($"option:dont over writing [{this.dontOverwriting.Checked}],remove backup [{this.removeBackup.Checked}]");
            FileSystem.CopyDirectory(src, tar, true);
        }

        if (this.removeBackup.Checked) {
            Logger.Info("元データを消します");
            Directory.Delete(src);
        }
    }
}
