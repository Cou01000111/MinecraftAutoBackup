using System.Windows.Forms;
#region tab backup

#endregion

class BackupDataListViewItem :ListViewItem {
    public BackupDataListViewItem(string[] items, World w) : base(items) {
        world = w;
    }

    public World world { get; set; }
}
