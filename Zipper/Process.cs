using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;


/*
 
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
普通のフォルダはディレクトリだけど、zipファイルはファイルとして処理すること！！ディレクトリとzipファイルは全くの別物！！
 
 */

/*
MinecraftAutoBackupをC:tmpへコピー{tmp process}
C:tmp内で必要なファイル/フォルダには処理{zipper process, decomp process}
MinecraftAutoBackup_tmpへ処理後のフォルダをコピー(ここで直接コピーしないことによってコピー失敗した時に加工前のデータが生き残る){endtime process}
MinecraftAutoBackupの削除{endtime process}
MinecraftAutoBackup_tmpをMinecraftAutoBackupに改名{endtime process}
 */

/*
MinecraftAutoBackup内で処理が必要なファイルリストの作成{make backup list}
MinecraftAutoBackupをC:tmpへコピー
C:tmp内で必要なファイル/フォルダには処理
MinecraftAutoBackup_tmpへ処理後のフォルダをコピー(ここで直接コピーしないことによってコピー失敗した時に加工前のデータが生き残る)
MinecraftAutoBackupの削除
MinecraftAutoBackup_tmpをMinecraftAutoBackupに改名
 */


namespace Zipper {
    class Process {
        private static Logger logger = new Logger("Zipper");
        public static string tmpPath;
        public static int successCount = 0;
        public static int errorCount = 0;
        public static int skipCount = 0;
        public static bool CompMode;
        public static async Task<int> MainProcess(string[] args) {

            //バックアップがない場合で、_tmpファイルがある場合は前回のZipperがmoveを失敗してるだけの可能性があるから名前変更
            if (Directory.Exists(AppConfig.BackupPath + "_tmp") && (!Directory.Exists(AppConfig.BackupPath))) {
                Directory.Move(AppConfig.BackupPath + "_tmp", AppConfig.BackupPath);
            }

            logger.Info("start Zipper");
            if (args.ToList().Count() == 0) {
                logger.Error("argsが存在しません");
                ExitProcess(false);
                return 1;
            }
            CompMode = (args[0] == "0");
            List<string> srcProcessBackupList = new List<string>();
            try {
                if (CompMode) {
                    srcProcessBackupList = GetBackupFolderList(AppConfig.BackupPath);
                }
                else {
                    srcProcessBackupList = GetZipFileList(AppConfig.BackupPath);
                }
            }
            catch (Exception e) {
                logger.Error("処理予定バックアップリストの取得に失敗しました");
                logger.Error(e.Message);
                logger.Error(e.StackTrace);
                ExitProcess(false);
                return 1;
            }
            foreach(string a in srcProcessBackupList) {
                logger.Debug(a);
            }
            if (srcProcessBackupList.Count == 0) {
                logger.Info("処理予定のバックアップが一つもありませんでした");
                ExitProcess(true);
                return 1;
            }

            //tmpファイルを作りそこへバックアップ先を移す
            CreateTmpFolder(srcProcessBackupList);
            logger.Info($"tmpPath:{tmpPath}");
            logger.Info($"tmpPathExits:{Directory.Exists(tmpPath)}");

            //圧縮 & 非圧縮するファイルへのパスの配列を作る
            List<string> backupList = new List<string>();
            try {
                backupList = GetBackupList(tmpPath);
            }
            catch (Exception e) {
                logger.Error(e.StackTrace);
                logger.Error("バックアップが一つもありません");
                ExitProcess(false);
                return 1;
            }
            foreach(string a in backupList) {
                logger.Info(a);
            }


            if (args[0] == "0") {
                //0番ならzippingMode
                Compressions(backupList);
            }
            else if (args[0] == "1") {
                //1番ならdecompressionMode
                Decompressions(backupList);
            }
            else {
                logger.Error("Args Error");
                ExitProcess(false);
                return 1;
            }
            return 1;
        }
        private static List<string> GetZipFileList(string path) {
            //圧縮 & 非圧縮するファイルへのパスの配列を作る
            List<string> backups = new List<string>();
            List<string> dirs = new List<string>();
            dirs = Directory.GetDirectories(path).ToList();
            logger.Info($"dirs.Count: {dirs.Count()}");
            List<string> worlds = new List<string>();
            foreach (string dir in dirs) {
                logger.Debug($"game directory : {dir} , ({Directory.Exists(dir)})");
                worlds.AddRange(Directory.GetDirectories(dir));
            }
            foreach (var w in worlds) {
                logger.Debug($" world data    : {w} , ({Directory.Exists(w)})");
                backups.AddRange(Directory.GetFiles(w));
            }
            foreach (var p in backups) {
                logger.Debug($"  backup data  : {p} , (dir {Directory.Exists(p)},file {File.Exists(p)})");
            }
            return backups;
        }
        private static List<string> GetBackupFolderList(string path) {
            //圧縮 & 非圧縮するファイルへのパスの配列を作る
            List<string> backups = new List<string>();
            List<string> dirs = new List<string>();
            dirs = Directory.GetDirectories(path).ToList();
            logger.Info($"dirs.Count: {dirs.Count()}");
            List<string> worlds = new List<string>();
            foreach (string dir in dirs) {
                logger.Debug($"game directory : {dir} , ({Directory.Exists(dir)})");
                worlds.AddRange(Directory.GetDirectories(dir));
            }
            //comp mode
            foreach (var w in worlds) {
                logger.Debug($" world data    : {w} , ({Directory.Exists(w)})");
                backups.AddRange(Directory.GetDirectories(w));
            }

            foreach (var p in backups) {
                logger.Debug($"  backup data  : {p} , (dir {Directory.Exists(p)},file {File.Exists(p)})");
            }
            return backups;
        }
        private static void CreateTmpFolder(List<string> processBackupList) {
            //前回のtmpファイルが残っている場合は削除
            tmpPath = $"{Path.GetTempPath()}MABtmp";
            if (Directory.Exists(tmpPath)) {
                try {
                    FileSystem.DeleteDirectory(tmpPath, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
                }
                catch (Exception e) {
                    logger.Error("前回の残存tmpファイルが削除できませんでした");
                    logger.Error(e.Message);
                    logger.Error(e.StackTrace);
                    ExitProcess(false);
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
                ExitProcess(false);
                return;
            }

            logger.Info($"{tmpPath}:が作成されました");
            logger.Info($"tmpファイルにバックアップのディレクトリ構造を再現します");
            try { CreateDestDir(); }
            catch (Exception e) {
                logger.Error("ディレクトリ構造の再現に失敗しました");
                logger.Error($"{e.Message}");
                logger.Error($"{e.StackTrace}");
            }
            logger.Info($"tmpフォルダへバックアップをコピーしています");
            try {
                if (CompMode) {
                    // tmp/gameDir/world/
                    foreach (string srcPath in processBackupList) {
                        string destPath = tmpPath + srcPath.Substring(AppConfig.BackupPath.Length, srcPath.Length - AppConfig.BackupPath.Length);
                        logger.Info($"{srcPath} を {destPath} にコピーします");
                        FileSystem.CopyDirectory(srcPath, destPath);
                    }
                }
                else {
                    foreach (string srcPath in processBackupList) {
                        string destPath = tmpPath + "\\" + srcPath.Substring(AppConfig.BackupPath.Length, srcPath.Length - AppConfig.BackupPath.Length);
                        logger.Info($"{srcPath} を {destPath} にコピーします");
                        FileSystem.CopyFile(srcPath, destPath);
                    }
                }
            }
            catch (Exception e) {
                logger.Error(e.Message);
                return;
            }
        }
        private static void Compressions(List<string> backups) {
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
            ExitProcess(true);
        }

        private static void Decompressions(List<string> backups) {
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
                ExitProcess(false);
                return;
            }
            ExitProcess(true);

        }
        public static void ExitProcess(bool normalTermination) {
            if (successCount != 0) {
                logger.Info($"{successCount}件圧縮/解凍済み,{skipCount}件のスルー,{errorCount}件のエラーが発生しました");
                if (normalTermination) {
                    //tmpファイルの内容をMinecraftAutoBackup_tmpに移す
                    try {
                        if (Directory.Exists(AppConfig.BackupPath + "_tmp")) {
                            logger.Warn("前回の異常終了時のdoc内tmpファイルを発見したので削除します");
                            FileSystem.DeleteDirectory(AppConfig.BackupPath + "_tmp", UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
                        }
                    }
                    catch (Exception e) {
                        logger.Error("_tmpファイルの削除に失敗しました");
                        logger.Error(e.Message);
                        logger.Error(e.StackTrace);
                        goto NOTERROR;
                    }
                    try {
                        logger.Info("tmpファイルをdocへ移すためにdoc内tmpファイルを作成します");
                        Directory.CreateDirectory(AppConfig.BackupPath + "_tmp");
                    }
                    catch (Exception e) {
                        logger.Error("doc内tmpファイルの作成に失敗しました");
                        logger.Error(e.StackTrace);
                        goto NOTERROR;
                    }
                    try {
                        logger.Info("tmpファイルをdocへ移すためにdoc内tmpファイルへコピーします");
                        FileSystem.CopyDirectory(tmpPath, AppConfig.BackupPath + "_tmp");
                    }
                    catch (Exception e) {
                        logger.Error("doc内tmpファイルのコピーに失敗しました");
                        logger.Error(e.Message);
                        logger.Error(e.StackTrace);
                        goto NOTERROR;
                    }
                    try {
                        logger.Info("未処理のバックアップデータをdoc内tmpフォルダに移します");
                        if (CompMode) {
                            //compモードの場合(ファイルをcopy tmp in doc )
                            List<string> copyPlanFileList = GetZipFileList(AppConfig.BackupPath);
                            foreach (string srcPath in copyPlanFileList) {
                                string destPath = AppConfig.BackupPath + "_tmp" + srcPath.Substring(AppConfig.BackupPath.Length, srcPath.Length - AppConfig.BackupPath.Length);
                                logger.Debug($"file copy:{srcPath} => {destPath}");
                                File.Copy(srcPath, destPath);
                            }

                        }
                        else {
                            List<string> copyPlanFileList = GetBackupFolderList(AppConfig.BackupPath);
                            foreach (string srcPath in copyPlanFileList) {
                                string destPath = AppConfig.BackupPath + "_tmp\\" + srcPath.Substring(AppConfig.BackupPath.Length, srcPath.Length - AppConfig.BackupPath.Length);
                                logger.Debug($"dir copy:{srcPath} => {destPath}");
                                //FileSystem.CopyDirectory(srcPath, destPath);
                            }
                        }
                    }
                    catch (Exception e) {
                        logger.Error("未処理のバックアップデータのdoc内tmpフォルダへの移行に失敗しました");
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

        /// <summary>
        /// 元ディレクトリの構造を引き継いで別名でディレクトリを作成する
        /// </summary>
        /// <param name="sourceDir">元ディレクトリ</param>
        static void CreateDestDir() {
            var orig = AppConfig.BackupPath;
            var dest = tmpPath;
            System.IO.Directory.EnumerateDirectories(orig, "*", System.IO.SearchOption.AllDirectories)
                .Select(d => d.Replace(orig, dest))
                .Where(d => !Directory.Exists(d))
                .ToList()
                .ForEach(d => Directory.CreateDirectory(d));
        }

        private static List<string> GetBackupList(string path) {
            List<string> backups = new List<string>();
            List<string> dirs = new List<string>();
            List<string> worlds = new List<string>();
            dirs = Directory.GetDirectories(tmpPath).ToList();
            logger.Info($"dirs.Count: {dirs.Count()}");
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
            return backups;
        }
    }
}
