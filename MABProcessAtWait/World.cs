using System.IO;

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
    public class World {
        public bool WDoBackup { get; set; }
        public string WPath { get; set; }
        public string WName { get; set; }
        public string WDir { get; set; }
        public bool isAlive { get; set; }
        public World(string path) {
            WDoBackup = true;
            WPath = path;
            WName = Path.GetFileName(path);
            WDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
            isAlive = true;
        }

        public World(string path, bool doBackup, bool _isAlive) {
            WDoBackup = doBackup;
            WPath = path;
            WName = Path.GetFileName(path);
            WDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
            isAlive = _isAlive;
        }
    }
}
