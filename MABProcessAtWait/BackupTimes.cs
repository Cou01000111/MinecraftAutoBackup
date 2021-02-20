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
        public string worldPath;
        public DateTime nextBackupTime;
        public BackupTimes(string path, string time) {
            worldPath = path;
            nextBackupTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(time)).DateTime;
        }
    }
}
