using System.Windows.Forms;
#region tab backup

#endregion

class BackupDataListViewItem :ListViewItem {
    public BackupDataListViewItem(string[] items, World w) : base(items) {
        World = w;
    }

    public World World { get; set; }
}
