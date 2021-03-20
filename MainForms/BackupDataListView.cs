using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.FileIO;
#region tab backup
class BackupDataListView :ListView {
    private ContextMenuStrip clmnMenu;
    private BackupDataListViewItem selectedItem = null;
    private ColumnHeader clmnBackupTime; // 'バックアップ日時' 列ヘッダ
    private ColumnHeader clmnAffiliationWorldName;  // '所属ワールド名' 列ヘッダ
    private ColumnHeader clmnWorldAffiliationDir;  // '所属ディレクトリ' 列ヘッダ

    private Logger logger = new Logger("MainForm");
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
        clmnMenu = new ContextMenuStrip();

        #region contextmenu

        clmnMenu.Opening += new CancelEventHandler(Menu_Opening);
        ToolStripMenuItem returnBackup = new ToolStripMenuItem() {
            Text = "このバックアップから復元する(&B)"
        };
        returnBackup.Click += new EventHandler(ReturnBackup_Click);
        clmnMenu.Items.Add(returnBackup);

        ToolStripMenuItem openInExplorer = new ToolStripMenuItem() {
            Text = "エクスプローラーで開く(&X)"
        };
        openInExplorer.Click += new EventHandler(OpenInExplorer_Click);
        if (worldObj.IsAlive)
            clmnMenu.Items.Add(openInExplorer);

        ToolStripMenuItem deleteBackup = new ToolStripMenuItem() {
            Text = "このバックアップを削除する(&B)",
            ForeColor = Color.Red,
        };
        deleteBackup.Click += new EventHandler(DeleteBackup_Click);
        clmnMenu.Items.Add(deleteBackup);

        this.ContextMenuStrip = clmnMenu;
        #endregion

        clmnBackupTime = new ColumnHeader() { Text = "バックアップ日時" };
        clmnAffiliationWorldName = new ColumnHeader() { Text = "バックアップ元" };
        clmnWorldAffiliationDir = new ColumnHeader() { Text = "バックアップ元の所属ディレクトリ" };

        Columns.AddRange(new ColumnHeader[]{
            clmnBackupTime,clmnAffiliationWorldName,clmnWorldAffiliationDir
        });

        //なんかこれやると横幅そろうらしい
        foreach (ColumnHeader ch in this.Columns) {
            ch.Width = -2;
        }

        LoadFromBackupFolder(worldObj);
        if (Items.Count > 0) {
            logger.Debug($"{Height}");
            Height /= 4;
            Height *= Items.Count + 1;
        }
    }
    private void LoadFromBackupFolder(World worldObj) {
        logger.Info("" + worldObj.WorldName + "の一覧以下のパスからロードします");
        logger.Info($"path:{worldObj.WorldPath}");
        try {
            List<string> backupFolders = new List<string>(GetBackups(worldObj));
            logger.Info($"backupFolderCount[{backupFolders.Count()}]");
            foreach (string backupFolder in backupFolders) {
                if (Path.GetExtension(backupFolder) == "zip") {
                    backupFolder.Substring(0, backupFolder.Length - 4);
                    logger.Debug("a:" + backupFolder);
                }
                DateTime time = DateTime.ParseExact((Path.GetFileName(backupFolder)).Substring(0, 12), "yyyyMMddHHmm", null);
                this.Items.Add(new BackupDataListViewItem(new string[] { time.ToString("yyyy-MM-dd HH:mm"), worldObj.WorldName, worldObj.WorldDir }, worldObj));
            }
        }
        catch (DirectoryNotFoundException e) {
            logger.Warn("バックアップが存在しません");
            logger.Warn(e.Message);
            logger.Warn(e.StackTrace);
        }
    }

    private List<string> GetBackups(World w) {
        //バックアップがない場合で、_tmpファイルがある場合は前回のZipperがmoveを失敗してるだけの可能性があるから名前変更
        if (Directory.Exists(AppConfig.BackupPath + "_tmp") && (!Directory.Exists(AppConfig.BackupPath))) {
            Directory.Move(AppConfig.BackupPath + "_tmp", AppConfig.BackupPath);
        }

        return Directory.GetFileSystemEntries(AppConfig.BackupPath + "\\" + w.WorldDir + "\\" + w.WorldName).ToList();
    }

    private void Menu_Opening(object sender, CancelEventArgs e) {
        logger.Info("call:Menu_Opening");
        Point p = this.PointToClient(Cursor.Position);
        BackupDataListViewItem item = this.HitTest(p).Item as BackupDataListViewItem;
        if (item == null) {
            e.Cancel = true;
        }
        else if (item.Bounds.Contains(p)) {
            ContextMenuStrip menu = sender as ContextMenuStrip;
            if (menu != null) {
                logger.Info($"{item}");
                logger.Info(item.World.WorldName);
                logger.Info($"{item.World.IsAlive}");
                selectedItem = item;
            }
        }
        else {
            e.Cancel = true;
        }
    }

    private void ReturnBackup_Click(object sender, EventArgs e) {
        logger.Info("call:ReturnBackup_Click");
        ContextMenuStrip menu = sender as ContextMenuStrip;
        if (selectedItem != null) {
            logger.Info($"{ selectedItem.World.IsAlive}");
            if (!selectedItem.World.IsAlive) {
                //バックアップ元ワールドが死んでいる場合messageBox出現
                logger.Info(" 死亡済みワールドのバックアップが選択されました");
                //同名のワールドがゲームディレクトリ内に存在する場合
                if (Directory.Exists(selectedItem.World.WorldPath)) {
                    MessageBox.Show("同名の別ワールドがゲームディレクトリ内に存在しています", "Minecraft Auto Backup", buttons: MessageBoxButtons.OK);
                }
            }
            DateTime dt = DateTime.ParseExact(selectedItem.SubItems[0].Text, "yyyy-MM-dd HH:mm", null);
            string fileName = dt.ToString("yyyyMMddHHmm");
            string src;
            if (File.Exists($"{AppConfig.BackupPath}\\{selectedItem.SubItems[2].Text}\\{selectedItem.World.WorldName}\\{fileName}.zip")) {
                // バックアップがzipだった場合
                src = $"{AppConfig.BackupPath}\\{selectedItem.SubItems[2].Text}\\{selectedItem.World.WorldName}\\{fileName}.zip";
            }
            else {
                // バックアップがzipじゃなかった場合
                src = $"{AppConfig.BackupPath}\\{selectedItem.SubItems[2].Text}\\{selectedItem.World.WorldName}\\{fileName}";
            }

            World tar = selectedItem.World;
            RestoreFromBackupForm restoreFrom = new RestoreFromBackupForm(src, tar);
            restoreFrom.Show();
        }
    }

    private void OpenInExplorer_Click(object sender, EventArgs e) {
        System.Diagnostics.Process.Start("EXPLORER.EXE", Util.MakePathToWorld(selectedItem.SubItems[1].Text, selectedItem.SubItems[2].Text));
    }

    private void DeleteBackup_Click(object sender , EventArgs e) {
        DialogResult result = MessageBox.Show("このバックアップを削除しますか？","Minecraft Auto Backup",MessageBoxButtons.YesNo);
        if (result == DialogResult.Yes) {
            DateTime dt = DateTime.ParseExact(selectedItem.SubItems[0].Text, "yyyy-MM-dd HH:mm", null);
            string fileName = dt.ToString("yyyyMMddHHmm");
            string backupPath;
            if (File.Exists($"{AppConfig.BackupPath}\\{selectedItem.SubItems[2].Text}\\{selectedItem.World.WorldName}\\{fileName}.zip")) {
                // バックアップがzipだった場合
                backupPath = $"{AppConfig.BackupPath}\\{selectedItem.SubItems[2].Text}\\{selectedItem.World.WorldName}\\{fileName}.zip";
                FileSystem.DeleteFile(backupPath);
            }
            else {
                // バックアップがzipじゃなかった場合
                backupPath = $"{AppConfig.BackupPath}\\{selectedItem.SubItems[2].Text}\\{selectedItem.World.WorldName}\\{fileName}";
                FileSystem.DeleteDirectory(backupPath, UIOption.OnlyErrorDialogs,RecycleOption.DeletePermanently);
            }
            this.Items.Remove(selectedItem);
        }
    }

}
#endregion
