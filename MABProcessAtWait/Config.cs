using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/*
 フォルダ構成
MinecraftAutoBackup.exe
Configs
    config.txt
    BackupPath.txt
SubModules
    MABProcess.exe
image
    app.ico
 */

namespace MABProcessAtWait {
    public class Config {
        public static List<World> Configs = new List<World>();
        public static string ConfigPath = @".\Config\config.txt";

        /// <summary>
        /// ConfigファイルからAppに読み込む
        /// </summary>
        public static void Load() {
            Logger.Debug("call:LoadConfigToApp");
            List<string> texts = new List<string>();
            using (StreamReader reader = new StreamReader(ConfigPath, Encoding.GetEncoding("utf-8"))) {
                while (reader.Peek() >= 0) {
                    List<string> datas = reader.ReadLine().Split(',').ToList();
                    datas = datas.Select(x => Util.TrimDoubleQuotationMarks(x)).ToList();
                    Configs.Add(new World(datas[2], Convert.ToBoolean(datas[0]), Convert.ToBoolean(datas[4])));
                }
                Logger.Info($"Configから{Configs.Count()}件のワールドを読み込みました");
            }

        }

        /// <summary>
        /// configsをConfig.txtに上書きする
        /// </summary>
        public static void Write() {
            List<string> text = new List<string>();
            foreach (World config in Configs) {
                text.Add($"\"{config.WorldDoBackup}\",\"{config.WorldName}\",\"{config.WorldPath}\",\"{config.WorldDir}\",\"{config.WorldIsAlive}\"\n");
            }
            File.WriteAllText(ConfigPath, string.Join("", text), Encoding.GetEncoding("utf-8"));
        }


        /// <summary>
        /// Configファイルを更新する
        /// </summary>
        public static List<World> SyncConfig() {
            ConsoleConfig();
            Logger.Debug("call:reloadConfig");
            List<World> worldInHdd = GetWorldDataFromHDD();
            List<World> worldInConfig = Configs;
            Logger.Debug($"config: {worldInConfig.Count()}");
            Logger.Debug($"HDD   : {worldInHdd.Count()}");

            int i = 0;
            //configに存在しないpathをconfigに追加する
            foreach (World pc in worldInHdd) {
                Logger.Debug($"pc:{i}回目");
                //dobackup以外を比較して判定
                if (!worldInConfig.Select(x => $"{x.WorldPath}_{x.WorldIsAlive}").ToList().Contains($"{pc.WorldPath}_{pc.WorldIsAlive}")) {
                    Logger.Info($"ADD {pc.WorldName}");
                    Configs.Add(pc);
                }
                i++;
            }
            List<World> removeWorlds = new List<World>();
            Logger.Debug($"config: {worldInConfig.Count()}");
            Logger.Debug($"HDD   : {worldInHdd.Count()}");

            i = 0;
            int wI = 0;
            //Logger.Info("-----config一覧-----");
            //foreach(var a in worldInHdd.Select(x => new WorldForComparison(x)).ToList()) {
            //    Logger.Info($"pc : {a.path}/{a.isAlive.ToString()}");
            //}
            //Logger.Info("--------------------");
            foreach (World world in worldInConfig) {
                Logger.Debug($"config:{i}回目");
                //dobackup以外を比較して判定
                if (!worldInHdd.Select(x => $"{x.WorldPath}_{x.WorldIsAlive}").ToList().Contains($"{world.WorldPath}_{world.WorldIsAlive}")) {
                    //config内のworldがHDDになかった場合
                    if (GetBackups(world).Count() == 0) {
                        // バックアップが一つもない場合はconfigから削除
                        Logger.Info($"バックアップが一つもないのでRemoveWorldsに{world.WorldName}を追加");
                        removeWorlds.Add(world);
                    }
                    else {
                        if (world.WorldIsAlive) {
                            //バックアップが一つでもある場合は、backup一覧に表示するために殺すだけにする
                            Logger.Info($"{world.WorldName}のバックアップが残っているため殺害");
                            Config.Configs[wI].WorldIsAlive = false;
                            int count = 1;
                            while (Directory.Exists($"{AppConfig.BackupPath}\\{Config.Configs[wI].WorldDir}\\{Config.Configs[wI].WorldName}_(削除済み)_{count}")) {
                                Logger.Info($" path[ {AppConfig.BackupPath}\\{Config.Configs[wI].WorldDir}\\{Config.Configs[wI].WorldName}_(削除済み)_{count} ]");
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

            Logger.Debug($"config: {worldInConfig.Count()}");
            Logger.Debug($"HDD   : {worldInHdd.Count()}");

            foreach (World w in removeWorlds) {
                if (Configs.Remove(w)) {
                    Logger.Info($"REMOVE {w.WorldName} suc");
                }
                else {
                    Logger.Info($"REMOVE {w.WorldName} 見つかりませんでした");
                }
            }

            Write();

            Logger.Debug($"config: {worldInConfig.Count()}");
            Logger.Debug($"HDD   : {worldInHdd.Count()}");

            return removeWorlds;
        }
        private static List<string> GetBackups(World w) {
            try {
                return Directory.GetDirectories(AppConfig.BackupPath + "\\" + w.WorldDir + "\\" + w.WorldName).ToList();
            }
            catch (DirectoryNotFoundException) {
                Logger.Info($"{AppConfig.BackupPath}\\{w.WorldDir}\\{w.WorldName} にアクセスできませんでした");
                return new List<string>();
            }
        }

        public static void Change(string worldName, string worldDir, string doBackup) {
            Logger.Debug("call:Change");
            Logger.Debug("GET  worldName: " + worldName + ",  worldDir: " + worldDir + ",  dobackup: " + doBackup);
            List<World> _configs = new List<World>();
            foreach (World config in Configs) {
                if (config.WorldName == worldName && config.WorldDir == worldDir) {
                    config.WorldDoBackup = bool.Parse(doBackup);
                    _configs.Add(new World(config.WorldPath, Convert.ToBoolean(doBackup), config.WorldIsAlive));
                }
                else {
                    _configs.Add(new World(config.WorldPath, config.WorldDoBackup, config.WorldIsAlive));
                }
            }
            Configs = _configs;
            //ConsoleConfig();
        }

        /// <summary>
        /// PCからワールドデータ一覧を取得
        /// </summary>
        /// <returns>取得したList(world)</returns>
        private static List<World> GetWorldDataFromHDD() {
            Logger.Debug("call:GetWorldDataFromPC");
            List<World> worlds = new List<World>();
            List<string> _gameDirectory = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).ToList();
            List<string> gameDirectory = new List<string>();
            foreach (string dir in _gameDirectory) {
                List<string> dirsInDir = Directory.GetDirectories(dir).ToList();
                dirsInDir = dirsInDir.Select(x => Path.GetFileName(x)).Cast<string>().ToList();
                if (dirsInDir.Contains("logs") && dirsInDir.Contains("resourcepacks") && dirsInDir.Contains("saves")) {
                    gameDirectory.Add(dir);
                }
            }
            foreach (string dir in gameDirectory) {
                List<string> _worlds = Directory.GetDirectories($"{dir}\\saves").ToList();
                foreach (string worldPath in _worlds) {
                    worlds.Add(new World(Util.TrimDoubleQuotationMarks(worldPath)));
                }
            }
            return worlds;
        }

        /// <summary>
        /// PCからワールドデータ一覧を取得
        /// </summary>
        /// <param name="gameDirectory"></param>
        /// <returns>取得したList(world)</returns>
        private static List<World> GetWorldDataFromHDD(List<string> gameDirectory) {
            List<World> worlds = new List<World>();
            Logger.Debug("call:GetWorldDataFromPC");
            foreach (string dir in gameDirectory) {
                if (Directory.Exists($"{dir}\\saves")) {
                    List<string> _worlds = Directory.GetDirectories($"{dir}\\saves").ToList();
                    foreach (string worldPath in _worlds) {
                        worlds.Add(new World(Util.TrimDoubleQuotationMarks(worldPath)));
                    }
                }
            }
            return worlds;
        }

        public static void ConsoleConfig() {
            Logger.Info("----Configs----");
            foreach (World w in Configs) {
                Logger.Info($"[{w.WorldDoBackup},{w.WorldName},{w.WorldPath},{w.WorldDir},]");
            }
            Logger.Info("---------------");
        }
    }
}
