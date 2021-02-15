using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp2 {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("console app 2");
            string path = ".\\test.txt";
            string t = "testtest\n";
            for (int i = 0;i < 10;i++) {
                File.AppendAllText(path,t); 
            }
            List<string> texts = new List<string>();
            using ( var s = new StreamReader(path)) {
                string _texts = s.ReadToEnd();
                texts = _texts.Split('\n').ToList();
            }
            foreach(string s in texts) {
                Console.WriteLine("info:" + s);
            }
            Console.ReadLine();
        }
    }
}
