using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;


/*
 
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
 
 */


namespace Zipper {
    class Process {
        private static Logger logger = new Logger("Zipper");
        public static string tmpPath;
        public static int successCount = 0;
        public static int errorCount = 0;
        public static int skipCount = 0;
        public static async Task<int> MainProcess(string[] args) {

            //バックアップがない場合で、_tmpファイルがある場合は前回のZipperがmoveを失敗してるだけの可能性があるから名前変更
            if (Directory.Exists(AppConfig.BackupPath + "_tmp") && (!Directory.Exists(AppConfig.BackupPath))) {
                Directory.Move(AppConfig.BackupPath + "_tmp", AppConfig.BackupPath);
            }

            logger.Info("start Zipper");
            if (args.ToList().Count() == 0) {
                logger.Error("argsが存在しません");
                EndTimeProcess(false);
                return 1;
            }
            
            //tmpファイルを作りそこへバックアップ先を移す
            TmpProcess();
            logger.Info($"tmpPath:{tmpPath}");
            logger.Info($"tmpPathExits:{Directory.Exists(tmpPath)}");

            //圧縮 & 非圧縮するファイルへのパスの配列を作る
            List<string> backups = new List<string>();
            List<string> dirs = new List<string>();
            try {
                dirs = Directory.GetDirectories(tmpPath).ToList();
            }
            catch (Exception e) {
                logger.Error(e.StackTrace);
                logger.Error("バックアップが一つもありません");
                EndTimeProcess(false);
                return 1;
            }
            logger.Info($"dirs.Count: {dirs.Count()}");
            List<string> worlds = new List<string>();
            foreach (string dir in dirs) {
                logger.Debug($"game directory : {dir} , ({Directory.Exists(dir)})");
                worlds.AddRange(Directory.GetDirectories(dir));
            }
            foreach (var w in worlds) {
                logger.Debug($" world data    : {w} , ({Directory.Exists(w)})");
                backups.AddRange(Directory.GetDirectories(w));
                backups.AddRange(Directory.GetFiles(w));
            }
            foreach (var p in backups) {
                logger.Debug($"  backup data  : {p} , (dir {Directory.Exists(p)},file {File.Exists(p)})");
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
                logger.Error("Args Error");
                EndTimeProcess(false);
                return 1;
            }
            return 1;
        }

        private static void TmpProcess() {
            //前回のtmpファイルが残っている場合は削除
            tmpPath = $"{Path.GetTempPath()}MABtmp";
            if (Directory.Exists(tmpPath)) {
                try {
                    Directory.Delete(tmpPath);
                }
                catch {
                    logger.Error("前回の残存tmpファイルが削除できませんでした");
                    EndTimeProcess(false);
                    return;
                }
            }
            logger.Info("tmpファイルを作成します");

            try {
                FileSystem.CreateDirectory(tmpPath);
            }
            catch (Exception e) {
                logger.Error("tmpファイルの作成に失敗しました");
                logger.Error(e.StackTrace);
                EndTimeProcess(false);
                return;
            }

            logger.Info($"{tmpPath}:が作成されました");
            logger.Info($"tmpフォルダへバックアップをコピーしています");
            //前回のtmpファイルが残っていた場合
            if (File.Exists(tmpPath)) {
                logger.Warn("MABtmpを削除します");
                FileSystem.DeleteDirectory(tmpPath, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
            }
            try {
                FileSystem.CopyDirectory(AppConfig.BackupPath + "\\", tmpPath, UIOption.AllDialogs);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return;
            }
        }
        private static void ZipProcess(List<string> backups) {
            //0番ならzippingMode
            //try {

            List<string> pasess = backups;
            logger.Info("=========DoZipping=========");
            logger.Info($"{pasess.Count()}件のバックアップを検討します");
            foreach (var path in pasess) {
                logger.Info($"-------{path} の検討をします-------");
                logger.Info($"zipファイル判定:{path.Contains(".zip")}\n({path})");
                if (!path.Contains(".zip")) {
                    // ---Zip---
                    logger.Info($"{path} の処理を開始します");
                    try {
                        ZipFile.CreateFromDirectory(path, $"{path}.zip");
                    }
                    catch (IOException) {
                        logger.Error($"{path}: zipping io exception");
                        errorCount++;
                        continue;
                    }
                    catch (Exception e) {
                        logger.Error(e.GetType().ToString());
                        logger.Error(e.Message);
                        logger.Error(e.StackTrace);
                        errorCount++;
                        continue;
                    }
                    logger.Info($"{path} zip化完了");

                    // ---Delete---
                    try {
                        FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
                    }
                    catch (Exception e) {
                        logger.Error(e.GetType().ToString());
                        logger.Error(e.Message);
                        logger.Error(e.StackTrace);
                        errorCount++;
                        continue;
                    }
                    logger.Info($"[{path}]削除完了");
                    successCount++;
                }
                else {
                    skipCount++;
                }
            }
            EndTimeProcess(true);
        }

        private static void DecompProcess(List<string> backups) {
            List<string> pasess = backups;
            logger.Info("=========Decompression=========");
            logger.Info($"{pasess.Count()}件のバックアップを検討します");
            foreach (var path in pasess) {
                logger.Info($"-----{path} の検討をします-----");
                logger.Info($"zipファイル判定:{path.Contains(".zip")}\n({path})");
                if (path.Contains(".zip")) {
                    // ---Decomp---
                    logger.Info($"{path}の処理を開始します");
                    try { ZipFile.ExtractToDirectory($"{path}", path.Substring(0, path.Length - 4)); }
                    catch (IOException) {
                        logger.Error($"{path.Substring(0, path.Length - 4)}は既に存在します");
                        //Console.ReadLine();
                        errorCount++;
                        continue;
                    }
                    catch (InvalidDataException) {
                        logger.Error($"{path}が破損しているため解凍できません");
                        errorCount++;
                        continue;
                    }
                    catch (Exception e) {
                        logger.Error(e.GetType().ToString());
                        logger.Error(e.Message);
                        logger.Error(e.StackTrace);
                        //Console.ReadLine();
                        errorCount++;
                        continue;
                    }

                    // ---Delete---
                    try { File.Delete($"{path}"); }
                    catch (IOException) {
                        logger.Warn($"{path}が使用中だったため10秒後再試行します");
                        Task.Delay(10000);
                        try { File.Delete($"{path}"); }
                        catch {
                            logger.Error($"{path}が使用中のためスルーします");
                            errorCount++;
                            continue;
                        }
                        //Console.ReadLine();
                    }
                    catch (Exception e) {
                        logger.Error(e.GetType().ToString());
                        logger.Error(e.Message);
                        logger.Error(e.StackTrace);
                        //Console.ReadLine();
                        errorCount++;
                        continue;
                    }
                    logger.Info($"[{path}]削除完了");
                    successCount++;
                }
                else {
                    skipCount++;
                }
            }
            try {
                FileSystem.DeleteDirectory(AppConfig.BackupPath, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
                FileSystem.CopyDirectory(tmpPath, AppConfig.BackupPath);
            }
            catch (Exception e) {
                logger.Error(e.Message);
                logger.Error(e.StackTrace);
                logger.Error($"バックアップフォルダ{AppConfig.BackupPath}の削除ができなかったため、処理が完了できませんでした");
                System.Windows.Forms.MessageBox.Show("圧縮/解凍作業ができませんでした", "Minecraft Auto Backup");
                EndTimeProcess(false);
                return;
            }
            EndTimeProcess(true);

        }
        public static void EndTimeProcess(bool normalTermination) {
            if (normalTermination) {
                logger.Info($"{successCount}件圧縮/解凍済み,{skipCount}件のスルー,{errorCount}件のエラーが発生しました");
                //tmpファイルの内容をMinecraftAutoBackup_tmpに移す
                try {
                    if (Directory.Exists(AppConfig.BackupPath + "_tmp")) {
                        logger.Info("前回の異常終了時のdoc内tmpファイルを発見したので削除します");
                        FileSystem.DeleteDirectory(AppConfig.BackupPath + "_tmp", UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
                    }
                    logger.Info("tmpファイルをdocへ移すためにdoc内tmpファイルを作成します");
                    Directory.CreateDirectory(AppConfig.BackupPath + "_tmp");
                    logger.Info("tmpファイルをdocへ移すためにdoc内tmpファイルへコピーします");
                    FileSystem.CopyDirectory(tmpPath, AppConfig.BackupPath + "_tmp");
                }
                catch (Exception e) {
                    logger.Error("_tmpファイルの削除に失敗しました");
                    logger.Error(e.Message);
                    logger.Error(e.StackTrace);
                    goto NOTERROR;
                }
                //tmpファイルのコピーが成功した場合のみMinecraftAutoBackupを削除する
                try {
                    logger.Info("加工前フォルダを削除します");
                    Directory.Delete(AppConfig.BackupPath, true);
                }
                catch (Exception e) {
                    logger.Error("加工前フォルダの削除に失敗しました");
                    logger.Error(e.Message);
                    logger.Error(e.StackTrace);
                    goto NOTERROR;
                }
                //MinecraftAutoBackup_tmpをMinecraftAutoBackupに改名する
                try {
                    logger.Info("tmpファイルをリネームし、バックアップフォルダとする");
                    Directory.Move(AppConfig.BackupPath + "_tmp", AppConfig.BackupPath);
                }
                catch (Exception e) {
                    logger.Error("tmpファイルのリネームに失敗しました");
                    logger.Error(e.Message);
                    logger.Error(e.StackTrace);
                    goto NOTERROR;
                }
            }

        NOTERROR:
            logger.Info("Exit Process");
            try {
                Directory.Delete(tmpPath, true);
            }
            catch (DirectoryNotFoundException) {
                logger.Warn("削除予定のtmpフォルダが見つかりませんでした");
            }
            catch (Exception e) {
                logger.Error("EndTimeProcess内のtmpフォルダ削除で例外が発生しました");
                logger.Error(e.Message);
                logger.Error(e.StackTrace);
            }
        }
    }
}
