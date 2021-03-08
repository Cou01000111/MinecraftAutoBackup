using System;
using System.IO;
using System.Windows.Forms;
class Program {
    [STAThread]
    static void Main() {
        Logger logger = new Logger("MainForm", ".\\logs\\MainForm.txt", 3);
        try {//起動時処理
            
            logger.Info("-----起動時処理-------");

            new AppConfig();

            if (!File.Exists(Config.ConfigPath)) {
                logger.Info("configファイルがないのでconfigファイルを作成します");
                Config.MakeConfig();
            }
            else {
                Config.Load();
                Config.SyncConfig();
            }

            logger.Info("----------------------");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WorldListForm());
        }
        catch (Exception e){
            logger.Error(e.Message);
            logger.Error(e.StackTrace);
        }

    }
}
