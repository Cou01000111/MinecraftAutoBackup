using System.IO;

public class World {
    public bool WDoBackup { get; set; }
    public string WPath { get; set; }
    public string WName { get; set; }
    public string WDir { get; set; }
    public bool isAlive { get; set; }
    public World(string path) {
        //if (!Directory.Exists(path)) {
        //    Console.WriteLine($"info:不正なpath[{path}]が渡されました");
        //    return;
        //}
        WDoBackup = true;
        WPath = path;
        WName = Path.GetFileName(path);
        WDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
        isAlive = true;
    }

    public World(string path, bool doBackup, bool _isAlive) {
        //if (!Directory.Exists(path)) {
        //    Console.WriteLine($"info:不正なpath[{path}]が渡されました");
        //    return;
        //}
        WDoBackup = doBackup;
        WPath = path;
        WName = Path.GetFileName(path);
        WDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));
        isAlive = _isAlive;
    }

}
