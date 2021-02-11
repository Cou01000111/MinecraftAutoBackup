using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace Zipper {
    class Program {
        static void Main(string[] args) {

            // © DOBON!.
            //Mutex関係
            // https://dobon.net/vb/dotnet/process/checkprevinstance.html
            //Mutex名を決める（必ずアプリケーション固有の文字列に変更すること！）
            string mutexName = "ZippingProcess";
            //Mutexオブジェクトを作成する
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, mutexName);

            bool hasHandle = false;
            try {
                try {
                    hasHandle = mutex.WaitOne(0, false);
                }
                //.NET Framework 2.0以降の場合
                catch (System.Threading.AbandonedMutexException) {
                    hasHandle = true;
                }
                if (hasHandle == false) {
                    return;
                }
                Process.MainProcess(args);
            }
            finally {
                if (hasHandle) {
                    mutex.ReleaseMutex();
                }
                mutex.Close();
            }
        }
    }

    class Process {
        public static void MainProcess(string[] args) {
            Logger log = new Logger(3);
            int zippingCount = 0;
            int errorCount = 0;
            int skipCount = 0;

            if (args.ToList().Count() == 0) {
                log.Error("argsが存在しません");
                return;
            }

            if (args[0] == "0") {
                //0番ならzippingMode
                //try {
                List<string> pasess = args.ToList();
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
                            FileSystem.DeleteDirectory(path,UIOption.OnlyErrorDialogs,RecycleOption.DeletePermanently);
                        }
                        //catch (IOException) {
                        //    log.Warn($"{path}が使用中だったため10秒後再試行します");
                        //    Task.Delay(10000);
                        //    try { File.Delete($"{path}"); }
                        //    catch {
                        //        log.Error($"{path}が使用中のためスルーします");
                        //        errorCount++;
                        //        continue;
                        //    }
                        //    //Console.ReadLine();
                        //}
                        //catch (UnauthorizedAccessException) {
                        //    log.Warn($"{path}が使用中だったため10秒後再試行します");
                        //    Task.Delay(10000);
                        //    try { File.Delete($"{path}"); }
                        //    catch {
                        //        log.Error($"{path}が使用中のためスルーします");
                        //        errorCount++;
                        //        continue;
                        //    }
                        //}
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

            } else if(args[1] == "1") {
                //1番ならdecompressionMode
                //try {
                List<string> pasess = args.ToList();
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
                //}
                //catch (Exception) {
                //    log.Info("error");
                //}
            }else {
                log.Error("Args Error");
            }

        }
    }

    class Logger {
        private string logPath = ".\\logs\\Zipper.txt";
        private int outputLevel;


        public Logger(int level) {
            outputLevel = level;
        }

        public void Base(int level, string message) {
            string logLevelStr;
            switch (level) {
                case 3:
                    logLevelStr = "DEBUG";
                    break;
                case 2:
                    logLevelStr = "INFO ";
                    break;
                case 1:
                    logLevelStr = "WARN ";
                    break;
                case 0:
                    logLevelStr = "ERROR";
                    break;
                default:
                    logLevelStr = "NONE";
                    break;
            }
            if (level <= outputLevel) {
                string logMessage = $"{DateTime.Now.ToString($"yyyy/MM/dd-HH:mm:ss")} [{logLevelStr}]:{message}\n";
                Console.WriteLine(logMessage);
                File.AppendAllText(logPath, logMessage);
            }
        }
        public void Debug(string message) {
            Base(3, message);
        }
        public void Info(string message) {
            Base(2, message);
        }
        public void Warn(string message) {
            Base(1, message);
        }
        public void Error(string message) {
            Base(0, message);
        }
    }
}
