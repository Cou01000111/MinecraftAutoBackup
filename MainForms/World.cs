using System.IO;

public class World {
    public bool WorldDoBackup { get; set; }
    public string WorldPath { get; set; }
    public string WorldName { get; set; }
    public string WorldDir { get; set; }
    public bool IsAlive { get; set; }
    public World(string path) {
        WorldDoBackup = true;
        WorldPath = path;
        WorldName = Path.GetFileName(path);
        WorldDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
        IsAlive = true;
    }

    public World(string path, bool doBackup, bool _isAlive) {
        WorldDoBackup = doBackup;
        WorldPath = path;
        WorldName = Path.GetFileName(path);
        WorldDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
        IsAlive = _isAlive;
    }

}
