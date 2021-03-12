using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public class Config {
    public static List<World> Configs = new List<World>();

    public static string ConfigPath = @".\Config\config.txt";

    private static Logger logger = new Logger("Config");

    public static void MakeConfig() {
        logger.Info("call:MakeConfig");
        if (!Directory.Exists(Path.GetDirectoryName(ConfigPath))) {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
        }

        string value = $"configファイル[{ConfigPath}]生成完了";
        logger.Info(value);
        List<World> worlds = GetWorldDataFromHDD();
        //List<world> worlds = new List<world>();
        // ゲームディレクトリが見つからなかった場合
        if (worlds.Count <= 0) {
            logger.Info("ゲームディレクトリが一つも見つかりませんでした");
            DialogResult result = MessageBox.Show(
                "minecraftのゲームディレクトリが見つかりませんでした。手動で設定しますか？",
                "ゲームディレクトリが見つかりませんでした",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Error
                );
            if (result == DialogResult.OK) {
                CommonOpenFileDialog copd = new CommonOpenFileDialog();
                copd.Title = "ゲームディレクトリを選択してください（複数選択可）";
                copd.IsFolderPicker = true;
                copd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                copd.Multiselect = true;
                if (copd.ShowDialog() == CommonFileDialogResult.Ok) {
                    worlds.AddRange(GetWorldDataFromHDD(copd.FileNames.ToList()));
                }
            }
            else if (result == DialogResult.No) {

            }
        }
        foreach (var world in worlds) {
            logger.Info($"world[{world.WorldName}]を発見しました");
            Configs.Add(world);
            Write();
        }
    }

    /// <summary>
    /// ConfigファイルからAppに読み込む
    /// </summary>
    public static void Load() {
        logger.Info("call:LoadConfigToApp");
        List<string> texts = new List<string>();
        using (StreamReader reader = new StreamReader(ConfigPath, Encoding.GetEncoding("utf-8"))) {
            while (reader.Peek() >= 0) {
                List<string> datas = reader.ReadLine().Split(',').ToList();
                datas = datas.Select(x => Util.TrimDoubleQuotationMarks(x)).ToList();
                Configs.Add(new World(datas[2], Convert.ToBoolean(datas[0]), Convert.ToBoolean(datas[4])));
            }
            logger.Info($"Configから{Configs.Count()}件のワールドを読み込みました");
        }

    }

    /// <summary>
    /// configsをConfig.txtに上書きする
    /// </summary>
    public static void Write() {
        List<string> text = new List<string>();
        foreach (World config in Configs) {
            text.Add($"\"{config.WorldDoBackup}\",\"{config.WorldName}\",\"{config.WorldPath}\",\"{config.WorldDir}\",\"{config.IsAlive}\"\n");
        }
        File.WriteAllText(ConfigPath, string.Join("", text), Encoding.GetEncoding("utf-8"));
    }


    /// <summary>
    /// Configファイルを更新する
    /// </summary>
    public static List<World> SyncConfig() {
        logger.Info("call:reloadConfig");
        List<World> worldInHdd = GetWorldDataFromHDD();
        foreach (string world in AppConfig.AddGameDirPath) {
            worldInHdd.AddRange(GetWorldDataFromHDD(world));
        }
        List<World> worldInConfig = Config.Configs;
        logger.Debug($"config: {worldInConfig.Count()}");
        logger.Debug($"HDD   : {worldInHdd.Count()}");

        int i = 0;
        //configに存在しないpathをconfigに追加する
        foreach (World pc in worldInHdd) {
            logger.Debug($"pc:{i}回目");
            //dobackup以外を比較して判定
            //List<WorldForComparison> _comp = worldInConfig.Select(x => new WorldForComparison(x)).ToList();
            if (!worldInConfig.Select(x => $"{x.WorldPath}_{x.IsAlive}").ToList().Contains($"{pc.WorldPath}_{pc.IsAlive}")) {
                logger.Debug($"ADD {pc.WorldName}");
                Configs.Add(pc);
            }
            i++;
        }
        List<World> removeWorlds = new List<World>();
        logger.Debug($"config: {worldInConfig.Count()}");
        logger.Debug($"HDD   : {worldInHdd.Count()}");

        i = 0;
        //configに存在するがhddに存在しない(削除されたワールド)pathをconfigで死亡扱いにする
        //isAliveプロパティを追加したので、そちらで管理
        int wI = 0;
        foreach (World world in worldInConfig) {
            WorldForComparison cf = new WorldForComparison(world);
            logger.Debug($"config:{i}回目");
            //dobackup以外を比較して判定
            if (!worldInHdd.Select(x => $"{x.WorldPath}_{x.IsAlive}").ToList().Contains($"{world.WorldPath}_{world.IsAlive}")) {
                //config内のworldがHDDになかった場合
                if (Util.GetBackup(world).Count() == 0) {
                    // バックアップが一つもない場合はconfigから削除
                    logger.Debug($"バックアップが一つもないのでRemoveWorldsに{world.WorldName}を追加");
                    removeWorlds.Add(world);
                }
                else {
                    if (world.IsAlive) {
                        //バックアップが一つでもある場合は、backup一覧に表示するために殺すだけにする
                        logger.Debug($"{world.WorldName}のバックアップが残っているため殺害");
                        Config.Configs[wI].IsAlive = false;
                        int count = 1;
                        while (Directory.Exists($"{AppConfig.BackupPath}\\{Config.Configs[wI].WorldDir}\\{Config.Configs[wI].WorldName}_(削除済み)_{count}")) {
                            logger.Debug($" path[ {AppConfig.BackupPath}\\{Config.Configs[wI].WorldDir}\\{Config.Configs[wI].WorldName}_(削除済み)_{count} ]");
                            count++;
                        }

                        Directory.Move($"{AppConfig.BackupPath}\\{ Config.Configs[wI].WorldDir}\\{ Config.Configs[wI].WorldName}",
                            $"{AppConfig.BackupPath}\\{ Config.Configs[wI].WorldDir}\\{ Config.Configs[wI].WorldName}_(削除済み)_{count}");
                        Config.Configs[wI].WorldPath += "_(削除済み)_" + count;
                        Config.Configs[wI].WorldName += "_(削除済み)_" + count;
                    }
                }
            }
            wI++;
            i++;
        }

        logger.Debug($"config: {worldInConfig.Count()}");
        logger.Debug($"HDD   : {worldInHdd.Count()}");

        foreach (World w in removeWorlds) {
            if (Configs.Remove(w)) {
                logger.Info($"REMOVE {w.WorldName} suc");
            }
            else {
                logger.Warn($"REMOVE {w.WorldName} 見つかりませんでした");
            }
        }

        Write();

        logger.Debug($"config: {worldInConfig.Count()}");
        logger.Debug($"HDD   : {worldInHdd.Count()}");

        return removeWorlds;
    }

    public static void Change(string worldName, string worldDir, string doBackup) {
        logger.Info("call:Change");
        logger.Info("GET  worldName: " + worldName + ",  worldDir: " + worldDir + ",  dobackup: " + doBackup);
        List<World> _configs = new List<World>();
        foreach (World config in Configs) {
            if (config.WorldName == worldName && config.WorldDir == worldDir) {
                config.WorldDoBackup = bool.Parse(doBackup);
                _configs.Add(new World(config.WorldPath, Convert.ToBoolean(doBackup), config.IsAlive));
            }
            else {
                _configs.Add(new World(config.WorldPath, config.WorldDoBackup, config.IsAlive));
            }
        }
        Configs = _configs;
        //ConsoleConfig();
    }

    /// <summary>
    /// PCからワールドデータ一覧を取得
    /// </summary>
    /// <returns>取得したList<world></returns>
    private static List<World> GetWorldDataFromHDD() {
        logger.Info("call:GetWorldDataFromPC");
        List<World> worlds = new List<World>();
        List<string> _gameDirectory = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).ToList();
        List<string> gameDirectory = new List<string>();
        foreach (string dir in _gameDirectory) {
            List<string> dirsInDir = Directory.GetDirectories(dir).ToList();
            dirsInDir = dirsInDir.Select(x => Path.GetFileName(x)).Cast<string>().ToList();
            if (dirsInDir.Contains("logs") && dirsInDir.Contains("resourcepacks") && dirsInDir.Contains("saves")) {
                logger.Debug($"ゲームディレクトリ[{dir}]を発見しました");
                gameDirectory.Add(dir);
            }
        }
        foreach (string dir in gameDirectory) {
            List<string> _worlds = Directory.GetDirectories($"{dir}\\saves").ToList();
            foreach (string worldPath in _worlds) {
                worlds.Add(new World(Util.TrimDoubleQuotationMarks(worldPath)));
            }
        }
        foreach (var a in worlds) {
            logger.Debug($"world[{a.WorldName}]");
        }
        return worlds;
    }

    /// <summary>
    /// PCからワールドデータ一覧を取得
    /// </summary>
    /// <param name="gameDirectory"></param>
    /// <returns>取得したList<world></returns>
    public static List<World> GetWorldDataFromHDD(List<string> gameDirectory) {
        List<World> worlds = new List<World>();
        logger.Info("call:GetWorldDataFromPC");
        foreach (string dir in gameDirectory) {
            if (Directory.Exists($"{dir}\\saves")) {
                List<string> _worlds = Directory.GetDirectories($"{dir}\\saves").ToList();
                foreach (string worldPath in _worlds) {
                    worlds.Add(new World(Util.TrimDoubleQuotationMarks(worldPath)));
                }
            }
        }
        //foreach(var a in worlds) {
        //    logger.Info($"world[{a.WName}]");
        //}
        return worlds;
    }
    public static List<World> GetWorldDataFromHDD(string gameDirectory) {
        List<World> worlds = new List<World>();
        logger.Info("call:GetWorldDataFromPC");
        if (Directory.Exists($"{gameDirectory}\\saves")) {
            List<string> _worlds = Directory.GetDirectories($"{gameDirectory}\\saves").ToList();
            foreach (string worldPath in _worlds) {
                worlds.Add(new World(Util.TrimDoubleQuotationMarks(worldPath)));
            }
        }
        return worlds;
    }

    public static void ConsoleConfig() {
        logger.Info("----Configs----");
        foreach (World w in Configs) {
            logger.Info($"[{w.WorldDoBackup},{w.WorldName},{w.WorldPath},{w.WorldDir},]");
        }
        logger.Info("---------------");
    }
    /// <summary>
    /// ワールドのバックアップソースが生きているかどうか
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static bool IsBackupAlive(World w) {
        if (w.IsAlive) {
            logger.Debug("バックアップは死んでいます");
            return false;
        }
        else {
            logger.Debug("バックアップは生きています");
            return true;
        }
    }

    //configsに存在しているゲームディレクトリをすべて返す
    public static List<string> GetGameDirInConfigs() {
        List<string> gameDirs = new List<string>();
        foreach (World cfg in Configs) {
            logger.Debug($"cfg:{cfg.WorldDir}");
            if (!gameDirs.Contains(cfg.WorldDir)) {
                gameDirs.Add(cfg.WorldDir);
            }
        }

        foreach (string gameDir in gameDirs) {
            logger.Debug($"gameDirInConfigs:{gameDir}");
        }
        return gameDirs;
    }
}
