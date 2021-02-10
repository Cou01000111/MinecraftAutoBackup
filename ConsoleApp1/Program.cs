using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace ConsoleApp1 {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("hello");
            Console.WriteLine(Directory.GetFiles(@"D:\Documents\MinecraftAutoBackup\.minecraft_forge1.12.2\新規ワールド-").ToList().Count());
            Console.WriteLine(Directory.GetDirectories(@"D:\Documents\MinecraftAutoBackup\.minecraft_forge1.12.2\新規ワールド-").ToList().Count());
        }
    }
}
