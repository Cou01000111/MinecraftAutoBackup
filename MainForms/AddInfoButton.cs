using System.Windows.Forms;

class AddInfoButton :Button {
    public int id { get; set; }

    public World World { get; set; }

    private Logger logger = new Logger("MainForm",3);

    public AddInfoButton(string path) {
        logger.Debug(path);
        World = Config.Configs.Find(x => x.WorldPath == path);
        Text = "バックアップ一覧";
        Width = (int)Util.FontStyle.Size * 14;
        Height = (int)Util.FontStyle.Size * 3;
    }

}
