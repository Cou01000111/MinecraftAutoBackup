using System;

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
    public class BackupTimes {
        public string WorldPath;
        public DateTime NextBackupTime;
        public BackupTimes(string path, string time) {
            WorldPath = path;
            NextBackupTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(time)).DateTime;
        }
    }
}
