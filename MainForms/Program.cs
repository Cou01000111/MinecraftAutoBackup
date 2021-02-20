using System;
using System.IO;
using System.Windows.Forms;
class Program {
    [STAThread]
    static void Main() {
        //起動時処理
        Logger.Info("-----起動時処理-------");

        new AppConfig();

        if (!File.Exists(Config.configPath)) {
            Logger.Info("configファイルがないのでconfigファイルを作成します");
            Config.MakeConfig();
        }
        else {
            Config.Load();
            Config.ReloadConfig();
        }

        Logger.Info("----------------------");
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new WorldListForm());

    }
}
