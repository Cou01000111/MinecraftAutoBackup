using System;

namespace ConsoleApp1 {
    class Program {
        static void Main(string[] args) {
            try {
                throw new Exception();
            }
            catch {
                Console.WriteLine("error");
            }
            Console.WriteLine("no error");
        }
    }
}
