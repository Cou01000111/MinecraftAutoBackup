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
        public static int successCount = 0;
        public static int errorCount = 0;
        public static int skipCount = 0;
        public static void MainProcess(string[] args) {
            //zipperがほかに動いてた場合は他zipperを強制終了してそのまま終了
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName("Zipper.exe");
            foreach (System.Diagnostics.Process p in ps) {
                Logger.Info($"{p.ProcessName}を検知しました");
                p.Kill();
            }

            if (args.ToList().Count() == 0) {
                Logger.Error("argsが存在しません");
                EndTimeProcess(false);
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
                EndTimeProcess(false);
                return;
            }

        }

        private static void TmpProcess() {
            //前回のtmpファイルが残っている場合は削除
            tmpPath = $"{Path.GetTempPath()}MABtmp";
            if (Directory.Exists(tmpPath)) {
                try {
                    Directory.Delete(tmpPath);
                }
                catch {
                    Logger.Error("前回の残存tmpファイルが削除できませんでした");
                    EndTimeProcess(false);
                }
            }
            Logger.Info("tmpファイルを作成します");
            
            try{ 
                FileSystem.CreateDirectory(tmpPath);
            }
            catch (Exception e){
                Logger.Error("tmpファイルの作成に失敗しました");
                Logger.Error(e.StackTrace);
                EndTimeProcess(false);
            }

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
                        FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
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
            Logger.Info("=========Decompression=========");

            Logger.Info($"{pasess.Count()}件のバックアップを検討します");
            foreach (var path in pasess) {
                Logger.Info($"-----{path} の検討をします-----");
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
                    catch (Exception e) {
                        Logger.Error(e.GetType().ToString());
                        Logger.Error(e.Message);
                        Logger.Error(e.StackTrace);
                        errorCount++;
                        continue;
                    }
                    Logger.Info($"[{path}]削除完了");
                    successCount++;
                }
                else {
                    skipCount++;
                }
            }
            try {
                FileSystem.DeleteDirectory(AppConfig.BackupPath,UIOption.OnlyErrorDialogs,RecycleOption.DeletePermanently);
                FileSystem.CopyDirectory(tmpPath, AppConfig.BackupPath);
            }
            catch (Exception e) {
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
                Logger.Error($"バックアップフォルダ{AppConfig.BackupPath}の削除ができなかったため、処理が完了できませんでした");
                System.Windows.Forms.MessageBox.Show("圧縮/解凍作業ができませんでした","Minecraft Auto Backup");
                EndTimeProcess(false);
                return;
            }
            EndTimeProcess(true);
            
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
        public static void EndTimeProcess(bool normalTermination) {
            if (normalTermination) {
                Logger.Info($"{successCount}件圧縮/解凍済み,{skipCount}件のスルー,{errorCount}件のエラーが発生しました");
                //tmpファイルの内容をバックアップフォルダに移す
                try {
                    Directory.Delete(AppConfig.BackupPath, true);
                    FileSystem.CopyDirectory(tmpPath, AppConfig.BackupPath);
                }
                catch (Exception e){
                    Logger.Error(e.Message);
                    Logger.Error(e.StackTrace);
                }
            }
            
            Logger.Info("Exit Process");
            try {
                Directory.Delete(tmpPath,true);
            }
            catch (DirectoryNotFoundException) {
                Logger.Warn("削除予定のtmpフォルダが見つかりませんでした");
            }
            catch (Exception e){
                Logger.Error("EndTimeProcess内のtmpフォルダ削除で例外が発生しました");
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
            }
        }
    }
}
