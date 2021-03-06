namespace Zipper {
    class Program {
        static void Main(string[] args) {
            Logger.Info(">--------------------<");
            new AppConfig();
            Process.MainProcess(args);
            Logger.Info(">--------------------<");
        }
    }
}
