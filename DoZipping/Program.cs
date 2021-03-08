using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Zipper {
    class Program {
        static void Main(string[] args) {
            try{
                Logger.Info(">------Zipper--------<");
                new AppConfig();
                Task.Factory.StartNew(() => Process.MainProcess(args));
                WaitDialog wait = new WaitDialog();
                wait.Show();
                Console.ReadLine();
                Logger.Info(">--------------------<");
            }
            catch (Exception e){
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
            }
        }
    }
}
