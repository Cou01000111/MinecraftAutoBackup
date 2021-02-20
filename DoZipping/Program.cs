namespace Zipper {
    class Program {
        static void Main(string[] args) {
            Logger log = new Logger();
            new AppConfig();
            Process.MainProcess(args);
        }
    }
}
