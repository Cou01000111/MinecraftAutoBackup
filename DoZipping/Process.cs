using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Zipper {
    class Process {
        public static string tmpPath;
        public static int zippingCount = 0;
        public static int errorCount = 0;
        public static int skipCount = 0;
        public static void MainProcess(string[] args) {

            if (args.ToList().Count() == 0) {
                Logger.Error("argsが存在しません");
                EndTimeProcess();
                return;
            }

            //一つだけzipされたとき=>zip化する
            //zip,decomされた時

            //tmpファイルを作りそこへバックアップ先を移す
            TmpProcess();

            //圧縮 & 非圧縮するファイルへのパスの配列を作る
            Logger.Info("");
            Logger.Debug("call: GetBackups");
            List<string> backups = new List<string>();
            List<string> dirs;
            dirs = Directory.GetDirectories(AppConfig.BackupPath).ToList();
            List<string> worlds = new List<string>();
            foreach (string dir in dirs) {
                worlds.AddRange(Directory.GetDirectories(dir));
            }
            foreach (var w in worlds) {
                backups.AddRange(Directory.GetDirectories(w));
                backups.AddRange(Directory.GetFiles(w));
            }


            if (args[0] == "0") {
                //0番ならzippingMode
                ZipProcess(backups);
            }
            else if (args[0] == "1") {
                //1番ならdecompressionMode
                DecompProcess(backups);
            }
            else {
                Logger.Error("Args Error");
                EndTimeProcess();
                return;
            }

        }

        private static void TmpProcess() {
            Logger.Info("tmpファイルを作成します");
            tmpPath = $"{Path.GetTempPath()}MABtmp";
            FileSystem.CreateDirectory(tmpPath);
            Logger.Info($"{tmpPath}:が作成されました");
            Logger.Info($"tmpフォルダへバックアップをコピーしています");
            //前回のtmpファイルが残っていた場合
            if (File.Exists(tmpPath)) {
                Logger.Warn("MABtmpを削除します");
                Directory.Delete(tmpPath, true);
            }
            FileSystem.CreateDirectory(tmpPath);
            try {
                FileSystem.CopyDirectory(AppConfig.BackupPath + "\\", tmpPath, UIOption.AllDialogs);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        private static void ZipProcess(List<string> backups) {
            //0番ならzippingMode
            //try {
            List<string> pasess = backups;
            Logger.Info("=========DoZipping=========");
            Logger.Info($"{pasess.Count()}件のバックアップを検討します");
            foreach (var path in pasess) {
                Logger.Info($"-------{path} の検討をします-------");
                Logger.Info($"zipファイル判定:{path.Contains(".zip")}");

                if (!path.Contains(".zip")) {

                    // -------Zip--------
                    Zip(path);

                    // -------Delete--------
                    try {
                        FileSystem.DeleteDirectory(path, UIOption.AllDialogs, RecycleOption.DeletePermanently);
                    }
                    catch (Exception e) {
                        Logger.Error(e.GetType().ToString());
                        Logger.Error(e.Message);
                        Logger.Error(e.StackTrace);
                        //Console.ReadLine();
                        errorCount++;
                        continue;
                    }
                    Logger.Info($"[{path}]削除完了");
                    zippingCount++;
                }
                else {
                    skipCount++;
                }
            }
            Logger.Info($"{zippingCount}件圧縮済み,{skipCount}件のスルー,{errorCount}件のエラーが発生しました");
            Directory.Delete(AppConfig.BackupPath, true);
            FileSystem.CopyDirectory(tmpPath, AppConfig.BackupPath);
            EndTimeProcess();
        }

        private static void DecompProcess(List<string> backups) {
            List<string> pasess = backups;
            Logger.Info("=========Decompression=========");

            Logger.Info($"{pasess.Count()}件のバックアップを検討します");
            foreach (var path in pasess) {
                Logger.Info($"-------{path} の検討をします-------");
                Logger.Info($"zipファイル判定:{path.Contains(".zip")}");
                if (path.Contains(".zip")) {
                    //バックアップがzipの場合
                    Logger.Info($"{path}の処理を開始します");
                    try { ZipFile.ExtractToDirectory($"{path}", path.Substring(0, path.Length - 4)); }
                    catch (IOException) {
                        Logger.Error($"{path.Substring(0, path.Length - 4)}は既に存在します");
                        //Console.ReadLine();
                        errorCount++;
                        continue;
                    }
                    catch (InvalidDataException) {
                        Logger.Error($"{path}が破損しているため解凍できません");
                        errorCount++;
                        continue;
                    }
                    catch (Exception e) {
                        Logger.Error(e.GetType().ToString());
                        Logger.Error(e.Message);
                        Logger.Error(e.StackTrace);
                        //Console.ReadLine();
                        errorCount++;
                        continue;
                    }
                    try { File.Delete($"{path}"); }
                    catch (IOException) {
                        Logger.Warn($"{path}が使用中だったため10秒後再試行します");
                        Task.Delay(10000);
                        try { File.Delete($"{path}"); }
                        catch {
                            Logger.Error($"{path}が使用中のためスルーします");
                            errorCount++;
                            continue;
                        }
                        //Console.ReadLine();
                    }
                    catch (Exception e) {
                        Logger.Error(e.GetType().ToString());
                        Logger.Error(e.Message);
                        Logger.Error(e.StackTrace);
                        //Console.ReadLine();
                        errorCount++;
                        continue;
                    }
                    //Logger.Info($"{Path.GetDirectoryName(backupPath)}\\{Path.GetFileName(backupPath)}をzipにします");
                }
                else {
                    skipCount++;
                }
            }
            Logger.Info($"{zippingCount}件圧縮済み,{skipCount}件のスルー,{errorCount}件のエラーが発生しました");
            Directory.Delete(AppConfig.BackupPath, true);
            FileSystem.CopyDirectory(tmpPath, AppConfig.BackupPath);
            EndTimeProcess();
        }


        private static void Zip(string path) {
            //バックアップがzipじゃない場合
            Logger.Info($"{path} の処理を開始します");
            try {
                ZipFile.CreateFromDirectory(path, $"{path}.zip");
            }
            catch (IOException) {
                Logger.Error($"{path}: zipping io exception");
                //Console.ReadLine();
                errorCount++;
                return;
            }

            catch (Exception e) {
                Logger.Error(e.GetType().ToString());
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
                //Console.ReadLine();
                errorCount++;
                return;
            }

            Logger.Info($"{path} zip化完了");
        }
        public static void EndTimeProcess() {
            Logger.Info("Exit Process");
            FileSystem.DeleteDirectory(tmpPath, UIOption.AllDialogs, RecycleOption.DeletePermanently);
        }
    }
}
