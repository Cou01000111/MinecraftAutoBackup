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

    private static Logger logger = new Logger("MainForm");

    /*
    バックアップオプション
    １．バックアップ元を消す
    ２．バックアップ先を上書きしない(別名で新規作成する)
    */
    public RestoreFromBackupForm(string backupSourcePath, World backupTarget) {
        logger.Info($"src[{backupSourcePath}]");
        logger.Info($"tar[{backupTarget.WorldPath}]");
        pathSrc = backupSourcePath;
        pathTar = backupTarget.WorldPath;
        if (!((Directory.Exists(backupSourcePath) || (File.Exists(backupSourcePath))))) {
            logger.Error($"normal {Directory.Exists(backupSourcePath)}");
            logger.Error($"zip    {File.Exists(backupSourcePath)}");
            logger.Error($"バックアップは存在しません");
            return;
        }
        Text = "バックアップから復元します";
        Icon = new Icon(".\\Image\\app.ico");
        Font = Util.FontStyle;
        Padding = new Padding((int)Util.FontStyle.Size * 2);
        ClientSize = new Size((int)Util.FontStyle.Size * 30, (int)Util.FontStyle.Size * 35);
        FormBorderStyle = FormBorderStyle.FixedSingle;

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
            Width = Width - Padding.Left * 2 - panel.Padding.Left * 2 - 10,
            Height = (int)Util.FontStyle.Height * 2 + 2,
            //BackColor = Color.Blue,
            //AutoSize = true,
        };

        dontOverwriting = new CheckBox() {
            Text = $"バックアップ先（{Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(pathTar)))}\\saves\\{Path.GetFileName(pathTar)}）を上書きしない\n(別名で新規作成する)",
            Width = Width - Padding.Left * 2 - panel.Padding.Left * 2 - 10,
            Height = (int)Util.FontStyle.Height * 6,
            //BackColor = Color.Blue,
        };

        doRestore = new Button() {
            Text = "バックアップから復元する",
            Height = (int)Util.FontStyle.Size * 5,
            Width = Width - Padding.Left * 2 - panel.Padding.Left * 2 - 10,
            Margin = new Padding(0, 0, 0, 20),
            UseVisualStyleBackColor = true,
            BackColor = SystemColors.Control
        };
        doRestore.Click += new EventHandler(doRestore_Click);

        logger.Info($"{pathSrc}を{pathTar}に上書きします");
        panel.Controls.Add(description);
        panel.Controls.Add(removeBackup);
        if (backupTarget.IsAlive) {
            //バックアップ先ワールドが生きている場合
            panel.Controls.Add(dontOverwriting);
        }
        panel.Controls.Add(doRestore);
        Controls.Add(panel);

        description.Width = panel.Width - panel.Padding.Left * 2 - 10;
        removeBackup.Width = panel.Width - panel.Padding.Left * 2 - 10;
        dontOverwriting.Width = panel.Width - panel.Padding.Left * 2 - 10;
        doRestore.Width = panel.Width - panel.Padding.Left * 2 - 10;

    }

    private void doRestore_Click(object sender, EventArgs e) {
        restoreFromBackup(sender, e);
    }
    private void restoreFromBackup(object sender, EventArgs e) {
        Close();
        string src = pathSrc;
        string tar = pathTar;

        logger.Info($" src[{src}]");
        logger.Info($" tar[{tar}]");
        if (dontOverwriting.Checked) {
            logger.Info("restore from backup");
            logger.Info($"option:dont over writing [{dontOverwriting.Checked}],remove backup [{removeBackup.Checked}]");
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
            logger.Info($"option:dont over writing [{dontOverwriting.Checked}],remove backup [{removeBackup.Checked}]");
            FileSystem.CopyDirectory(src, tar, true);
        }

        if (removeBackup.Checked) {
            logger.Info("元データを消します");
            Directory.Delete(src);
        }
    }
}
