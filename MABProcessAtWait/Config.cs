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
        /*
         必要な関数
        与えられたワールドオブジェクトをコンフィグファイルに書き変える
        コンフィグファイルの中身を渡す関数
        コンフィグファイルがないときにコンフィグファイルを作る関数
        コンフィグファイルからメモリに読み込む関数
        メモリの内容をコンフィグファイルに書き込む関数
        コンフィグファイルの内容をハードディスクの内容と照らし合わせて更新する
         ハードディスクの内容をワールドオブジェクトのListにして返す
        与えられたワールドオブジェクトをコンフィグファイルに書き加える
        与えられたワールドオブジェクトをコンフィグファイルから消す

        必要な関数:改良案

         コンフィグファイルがないときにコンフィグファイルを作る関数
         コンフィグファイルからメモリに読み込む関数
         メモリの内容をコンフィグファイルに書き込む関数
        コンフィグファイルの内容をハードディスクの内容と照らし合わせて更新する
         ハードディスクの内容をワールドオブジェクトのListにして返す
        与えられたワールドオブジェクトをメモリに書き変える
         */
        /*
        バックアップに関するオプションを記録するtxtファイル
        "バックアップの可否","ワールド名","ワールドへのパス","ワールドの所属するディレクトリ"
        が入っている
        */
        public static List<World> configs = new List<World>();

        public static string configPath = @".\Config\config.txt";

        //datasの中にworldName,worldDirに当てはまる要素があるかどうか
        private static bool IsWorldParticular(string worldName, string worldDir, string[] datas) {
            //Logger.Info(datas[1] + ",\"" + worldName + "\"と" + datas[3] + ",\"" + worldDir + "\"");
            return datas[1] == "\"" + worldName + "\"" && datas[3] == "\"" + worldDir + "\"";
        }

        public static List<World> GetConfig() => configs;

        /// <summary>
        /// ConfigファイルからAppに読み込む
        /// </summary>
        public static void Load() {
            Logger.Debug("call:LoadConfigToApp");
            List<string> texts = new List<string>();
            using (StreamReader reader = new StreamReader(configPath, Encoding.GetEncoding("utf-8"))) {
                while (reader.Peek() >= 0) {
                    List<string> datas = reader.ReadLine().Split(',').ToList();
                    datas = datas.Select(x => Util.TrimDoubleQuotationMarks(x)).ToList();
                    configs.Add(new World(datas[2], Convert.ToBoolean(datas[0]), Convert.ToBoolean(datas[4])));
                }
                Logger.Info($"Configから{configs.Count()}件のワールドを読み込みました");
            }

        }

        /// <summary>
        /// configsをConfig.txtに上書きする
        /// </summary>
        public static void Write() {
            List<string> text = new List<string>();
            foreach (World config in configs) {
                text.Add($"\"{config.WDoBackup}\",\"{config.WName}\",\"{config.WPath}\",\"{config.WDir}\",\"{config.isAlive}\"\n");
            }
            File.WriteAllText(configPath, string.Join("", text), Encoding.GetEncoding("utf-8"));
        }


        /// <summary>
        /// Configファイルを更新する
        /// </summary>
        public static List<World> ReloadConfig() {
            Logger.Debug("call:reloadConfig");
            List<World> worldInHdd = GetWorldDataFromHDD();
            List<World> worldInConfig = GetConfig();
            Logger.Debug($"config: {worldInConfig.Count()}");
            Logger.Debug($"HDD   : {worldInHdd.Count()}");

            int i = 0;
            //configに存在しないpathをconfigに追加する
            foreach (World pc in worldInHdd) {
                Logger.Debug($"pc:{i}回目");
                //dobackup以外を比較して判定
                //List<WorldForComparison> _comp = worldInConfig.Select(x => new WorldForComparison(x)).ToList();
                if (!worldInConfig.Select(x => $"{x.WPath}_{x.isAlive}").ToList().Contains($"{pc.WPath}_{pc.isAlive}")) {
                    Logger.Info($"ADD {pc.WName}");
                    configs.Add(pc);
                }
                i++;
            }
            List<World> removeWorlds = new List<World>();
            Logger.Debug($"config: {worldInConfig.Count()}");
            Logger.Debug($"HDD   : {worldInHdd.Count()}");

            i = 0;
            //configに存在するがhddに存在しない(削除されたワールド)pathをconfigで死亡扱いにする
            //isAliveプロパティを追加したので、そちらで管理
            int wI = 0;
            //Logger.Info("-----config一覧-----");
            //foreach(var a in worldInHdd.Select(x => new WorldForComparison(x)).ToList()) {
            //    Logger.Info($"pc : {a.path}/{a.isAlive.ToString()}");
            //}
            //Logger.Info("--------------------");
            foreach (World world in worldInConfig) {
                Logger.Debug($"config:{i}回目");
                //dobackup以外を比較して判定
                if (!worldInHdd.Select(x => $"{x.WPath}_{x.isAlive}").ToList().Contains($"{world.WPath}_{world.isAlive}")) {
                    //config内のworldがHDDになかった場合
                    if (GetBackups(world).Count() == 0) {
                        // バックアップが一つもない場合はconfigから削除
                        Logger.Info($"バックアップが一つもないのでRemoveWorldsに{world.WName}を追加");
                        removeWorlds.Add(world);
                    }
                    else {
                        if (world.isAlive) {
                            //バックアップが一つでもある場合は、backup一覧に表示するために殺すだけにする
                            Logger.Info($"{world.WName}のバックアップが残っているため殺害");
                            Config.configs[wI].isAlive = false;
                            int count = 1;
                            while (Directory.Exists($"{AppConfig.BackupPath}\\{Config.configs[wI].WDir}\\{Config.configs[wI].WName}_(削除済み)_{count}")) {
                                Logger.Info($" path[ {AppConfig.BackupPath}\\{Config.configs[wI].WDir}\\{Config.configs[wI].WName}_(削除済み)_{count} ]");
                                count++;
                            }

                            Directory.Move($"{AppConfig.BackupPath}\\{ Config.configs[wI].WDir}\\{ Config.configs[wI].WName}",
                                $"{AppConfig.BackupPath}\\{ Config.configs[wI].WDir}\\{ Config.configs[wI].WName}_(削除済み)_{count}");
                            Config.configs[wI].WPath += "_(削除済み)_" + count;
                            Config.configs[wI].WName += "_(削除済み)_" + count;
                        }
                    }
                }
                wI++;
                i++;
            }

            Logger.Debug($"config: {worldInConfig.Count()}");
            Logger.Debug($"HDD   : {worldInHdd.Count()}");

            foreach (World w in removeWorlds) {
                if (configs.Remove(w)) {
                    Logger.Info($"REMOVE {w.WName} suc");
                }
                else {
                    Logger.Info($"REMOVE {w.WName} 見つかりませんでした");
                }
            }

            Write();

            Logger.Debug($"config: {worldInConfig.Count()}");
            Logger.Debug($"HDD   : {worldInHdd.Count()}");

            return removeWorlds;
        }
        private static List<string> GetBackups(World w) {
            try {
                return Directory.GetDirectories(AppConfig.BackupPath + "\\" + w.WDir + "\\" + w.WName).ToList();
            }
            catch (DirectoryNotFoundException) {
                Logger.Info($"{AppConfig.BackupPath}\\{w.WDir}\\{w.WName} にアクセスできませんでした");
                return new List<string>();
            }
        }

        public static void Change(string worldName, string worldDir, string doBackup) {
            Logger.Debug("call:Change");
            Logger.Debug("GET  worldName: " + worldName + ",  worldDir: " + worldDir + ",  dobackup: " + doBackup);
            List<World> _configs = new List<World>();
            foreach (World config in configs) {
                if (config.WName == worldName && config.WDir == worldDir) {
                    config.WDoBackup = bool.Parse(doBackup);
                    _configs.Add(new World(config.WPath, Convert.ToBoolean(doBackup), config.isAlive));
                }
                else {
                    _configs.Add(new World(config.WPath, config.WDoBackup, config.isAlive));
                }
            }
            configs = _configs;
            //ConsoleConfig();
        }

        /// <summary>
        /// PCからワールドデータ一覧を取得
        /// </summary>
        /// <returns>取得したList<world></returns>
        private static List<World> GetWorldDataFromHDD() {
            Logger.Debug("call:GetWorldDataFromPC");
            List<World> worlds = new List<World>();
            List<string> _gameDirectory = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).ToList();
            List<string> gameDirectory = new List<string>();
            foreach (string dir in _gameDirectory) {
                List<string> dirsInDir = Directory.GetDirectories(dir).ToList();
                dirsInDir = dirsInDir.Select(x => Path.GetFileName(x)).Cast<string>().ToList();
                if (dirsInDir.Contains("logs") && dirsInDir.Contains("resourcepacks") && dirsInDir.Contains("saves")) {
                    //Logger.Info($"ゲームディレクトリ[{dir}]を発見しました");
                    gameDirectory.Add(dir);
                }
            }
            foreach (string dir in gameDirectory) {
                List<string> _worlds = Directory.GetDirectories($"{dir}\\saves").ToList();
                foreach (string worldPath in _worlds) {
                    worlds.Add(new World(Util.TrimDoubleQuotationMarks(worldPath)));
                }
            }
            //foreach(var a in worlds) {
            //    Logger.Info($"world[{a.WName}]");
            //}
            return worlds;
        }

        /// <summary>
        /// PCからワールドデータ一覧を取得
        /// </summary>
        /// <param name="gameDirectory"></param>
        /// <returns>取得したList<world></returns>
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
            //foreach(var a in worlds) {
            //    Logger.Info($"world[{a.WName}]");
            //}
            return worlds;
        }

        public static void ConsoleConfig() {
            Logger.Info("----Configs----");
            foreach (World w in configs) {
                Logger.Info($"[{w.WDoBackup},{w.WName},{w.WPath},{w.WDir},]");
            }
            Logger.Info("---------------");
        }
        /// <summary>
        /// ワールドのバックアップソースが生きているかどうか
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public static bool isBackupAlive(World w) {
            if (w.isAlive) {
                //Logger.Info("info[DEBUG]:バックアップは死んでいます");
                return false;
            }
            else {
                //Logger.Info("info[DEBUG]:バックアップは生きています");
                return true;
            }
        }
    }
}
