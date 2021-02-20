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
        public static void MainProcess(string[] args) {
            Logger log = new Logger();
            int zippingCount = 0;
            int errorCount = 0;
            int skipCount = 0;

            if (args.ToList().Count() == 0) {
                log.Error("argsが存在しません");
                EndTimeProcess();
                return;
            }

            //一つだけzipされたとき=>zip化する
            //zip,decomされた時

            //tmpファイルを作りそこへバックアップ先を移す
            log.Info("tmpファイルを作成します");
            tmpPath = $"{Path.GetTempPath()}MABtmp";
            log.Info($"tmpフォルダへバックアップをコピーしています");
            if (File.Exists(tmpPath)) {
                log.Warn("MABtmpを削除します");
                Directory.Delete(tmpPath, true);
            }
            FileSystem.CreateDirectory(tmpPath);
            try { FileSystem.CopyDirectory(AppConfig.BackupPath + "\\", tmpPath); }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            //圧縮 & 非圧縮するファイルへのパスの配列を作る

            log.Info("");
            Console.WriteLine("call: GetBackups");
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
            //Console.WriteLine($"dir:{dirs.Count()}, worlds:{worlds.Count()}, backups:{backups.Count()}");
            //while (GetLog.isRunningOtherZipper()) {
            //    Task.Delay(10000);
            //}


            if (args[0] == "0") {
                //0番ならzippingMode
                //try {
                List<string> pasess = backups;
                log.Info("=========DoZipping=========");
                log.Info($"{pasess.Count()}件のバックアップを検討します");
                foreach (var path in pasess) {
                    log.Info($"-------{path} の検討をします-------");
                    log.Info($"zipファイル判定:{path.Contains(".zip")}");

                    if (!path.Contains(".zip")) {

                        // -------Zip--------
                        //バックアップがzipじゃない場合
                        log.Info($"{path} の処理を開始します");
                        try {
                            ZipFile.CreateFromDirectory(path, $"{path}.zip");
                        }
                        catch (IOException) {
                            log.Error($"{path}: zipping io exception");
                            //Console.ReadLine();
                            errorCount++;
                            continue;
                        }

                        catch (Exception e) {
                            log.Error(e.GetType().ToString());
                            log.Error(e.Message);
                            log.Error(e.StackTrace);
                            //Console.ReadLine();
                            errorCount++;
                            continue;
                        }

                        log.Info($"{path} zip化完了");

                        // -------Delete--------
                        try {
                            FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
                        }
                        catch (Exception e) {
                            log.Error(e.GetType().ToString());
                            log.Error(e.Message);
                            log.Error(e.StackTrace);
                            //Console.ReadLine();
                            errorCount++;
                            continue;
                        }
                        log.Info($"[{path}]削除完了");
                        zippingCount++;
                        //log.Info($"{Path.GetDirectoryName(backupPath)}\\{Path.GetFileName(backupPath)}をzipにします");
                    }
                    else {
                        skipCount++;
                    }
                }
                //}
                //catch (Exception) {
                //    log.Info("error");
                //}
                log.Info($"{zippingCount}件圧縮済み,{skipCount}件のスルー,{errorCount}件のエラーが発生しました");
                Directory.Delete(AppConfig.BackupPath, true);
                FileSystem.CopyDirectory(tmpPath, AppConfig.BackupPath);
                EndTimeProcess();

            }
            else if (args[0] == "1") {
                //1番ならdecompressionMode
                //try {
                List<string> pasess = backups;
                log.Info("=========Decompression=========");

                log.Info($"{pasess.Count()}件のバックアップを検討します");
                foreach (var path in pasess) {
                    log.Info($"-------{path} の検討をします-------");
                    log.Info($"zipファイル判定:{path.Contains(".zip")}");
                    if (path.Contains(".zip")) {
                        //バックアップがzipの場合
                        log.Info($"{path}の処理を開始します");
                        try { ZipFile.ExtractToDirectory($"{path}", path.Substring(0, path.Length - 4)); }
                        catch (IOException) {
                            log.Error($"{path.Substring(0, path.Length - 4)}は既に存在します");
                            //Console.ReadLine();
                            errorCount++;
                            continue;
                        }
                        catch (InvalidDataException) {
                            log.Error($"{path}が破損しているため解凍できません");
                            errorCount++;
                            continue;
                        }
                        catch (Exception e) {
                            log.Error(e.GetType().ToString());
                            log.Error(e.Message);
                            log.Error(e.StackTrace);
                            //Console.ReadLine();
                            errorCount++;
                            continue;
                        }
                        try { File.Delete($"{path}"); }
                        catch (IOException) {
                            log.Warn($"{path}が使用中だったため10秒後再試行します");
                            Task.Delay(10000);
                            try { File.Delete($"{path}"); }
                            catch {
                                log.Error($"{path}が使用中のためスルーします");
                                errorCount++;
                                continue;
                            }
                            //Console.ReadLine();
                        }
                        catch (Exception e) {
                            log.Error(e.GetType().ToString());
                            log.Error(e.Message);
                            log.Error(e.StackTrace);
                            //Console.ReadLine();
                            errorCount++;
                            continue;
                        }
                        //log.Info($"{Path.GetDirectoryName(backupPath)}\\{Path.GetFileName(backupPath)}をzipにします");
                    }
                    else {
                        skipCount++;
                    }
                }
                log.Info($"{zippingCount}件圧縮済み,{skipCount}件のスルー,{errorCount}件のエラーが発生しました");
                Directory.Delete(AppConfig.BackupPath, true);
                FileSystem.CopyDirectory(tmpPath, AppConfig.BackupPath);
                EndTimeProcess();
                //}
                //catch (Exception) {
                //    log.Info("error");
                //}
            }
            else {
                log.Error("Args Error");
                EndTimeProcess();
                return;
            }

        }

        public static void EndTimeProcess() {
            Logger log = new Logger();
            log.Info("Exit Process");
            Directory.Delete(tmpPath, true);
        }
    }
}
