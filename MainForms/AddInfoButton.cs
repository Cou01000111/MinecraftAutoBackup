using System.Windows.Forms;
#region tab backup

class AddInfoButton :Button {
    public int id { get; set; }

    public World World { get; set; }

    public AddInfoButton(string path) {
        Logger.Debug(path);
        World = Config.configs.Find(x => x.WPath == path);
        Text = "バックアップ一覧";
        Width = (int)Util.FontStyle.Size * 14;
        Height = (int)Util.FontStyle.Size * 3;
    }

}
