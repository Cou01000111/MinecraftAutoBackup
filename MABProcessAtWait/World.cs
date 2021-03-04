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
        public bool WorldDoBackup { get; set; }
        public string WorldPath { get; set; }
        public string WorldName { get; set; }
        public string WorldDir { get; set; }
        public bool WorldIsAlive { get; set; }
        public World(string path) {
            WorldDoBackup = true;
            WorldPath = path;
            WorldName = Path.GetFileName(path);
            WorldDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
            WorldIsAlive = true;
        }

        public World(string path, bool doBackup, bool _isAlive) {
            WorldDoBackup = doBackup;
            WorldPath = path;
            WorldName = Path.GetFileName(path);
            WorldDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
            WorldIsAlive = _isAlive;
        }
    }
}
