public class WorldForComparison {
    public string path { get; set; }
    public bool isAlive { get; set; }

    public WorldForComparison(World w) {
        path = w.WPath;
        isAlive = w.isAlive;
    }
}
