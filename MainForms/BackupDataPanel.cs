using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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
            Logger.Info("バックアップが存在しません");
            Label notBackupFile = new Label() {
                Text = "バックアップが存在しません",
                Margin = new Padding(10),
            };
            this.Controls.Add(notBackupFile);
        }
        else {
            Logger.Info($"{Config.GetConfig().Count()}件のワールドのバックアップを読み込みます");
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
        Logger.Info("call:addListView");
        backupDataList = new BackupDataListView(button.World);
        //if (backupDataList.Items.Count == 0) {
        //    Logger.Info("not backup folder");
        //    return;
        //}
        this.Controls[index].Controls.Add(backupDataList);
        int a = (int)((dualPanel.Height + Margin.Left * 2) + backupDataList.Height);
        int dh = (int)(dualPanel.Height + Margin.Left * 2);
        this.Controls[index].Height = a;
    }

    void removeListView(object sender, EventArgs e) {
        Logger.Info("call:removeListView");
        var button = (AddInfoButton)sender;
        int index = button.id;
        this.Controls[index].Controls.Remove(this.Controls[index].Controls[1]);
        this.Controls[index].Height = dualPanel.Height + Margin.Left * 2;
    }
    public static List<string> GetBackupFiles(string worldName, string worldDir) {
        Logger.Info("call:get backup files");
        if (!Directory.Exists(AppConfig.BackupPath + "\\" + worldDir)) {
            Logger.Info($"{worldDir}ディレクトリのバックアップデータが一切ないのでフォルダを生成します");
            Directory.CreateDirectory(AppConfig.BackupPath + "\\" + worldDir);
        }
        if (!Directory.Exists(AppConfig.BackupPath + "\\" + worldDir + "\\" + worldName)) {
            Logger.Info($"{worldName}のバックアップデータが一切ないのでフォルダを生成します");
            Directory.CreateDirectory(AppConfig.BackupPath + "\\" + worldDir + "\\" + worldName);
        }
        List<string> folders = new List<string>(Directory.GetDirectories(AppConfig.BackupPath + "\\" + worldDir + "\\" + worldName));
        return folders;
    }
}
