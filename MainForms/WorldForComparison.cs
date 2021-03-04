public class WorldForComparison {
    public string WorldPath { get; set; }
    public bool IsAlive { get; set; }

    public WorldForComparison(World w) {
        WorldPath = w.WorldPath;
        IsAlive = w.IsAlive;
    }
}
