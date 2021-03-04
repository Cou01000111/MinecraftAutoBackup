using System;
using System.Collections.Generic;
using System.Windows.Forms;

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

        Columns.AddRange(new ColumnHeader[]{
            clmnDoBackup,
            clmnWorldName,
            clmnWorldDir,
        });

        foreach (ColumnHeader ch in Columns) {
            ch.Width = -1;
        }

        Columns[0].Width = (int)Util.FontStyle.Size * 14;

        ItemCheck += new ItemCheckEventHandler(listView_ItemClick);

        LoadFromConfigToList();

    }

    public void LoadFromConfigToList() {
        Items.Clear();
        List<World> listDatas = Config.GetConfig();
        int iItemCount = 0;
        foreach (var datas in listDatas) {
            if (datas.IsAlive) {
                Items.Add(new ListViewItem(new string[] { " ", datas.WorldName, datas.WorldDir }));
                Logger.Debug($"{datas.WorldDoBackup}");
                Items[iItemCount].Checked = Convert.ToBoolean(datas.WorldDoBackup);
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
        foreach (var item in Items) {
            _return.Add(new string[] { Items[i].Checked.ToString(), Items[i].SubItems[1].Text, Items[i].SubItems[2].Text });
            i++;
        }
        return _return;
    }
}
