using System;

namespace ConsoleApp1 {
    class Program {
        static void Main(string[] args) {
            int a = 0;
            try{
                Console.WriteLine(a);
                if (a == 0) {
                    throw new Exception();
                }
            }
            catch {
                Console.WriteLine(a);
            }
            Console.WriteLine("えらーなし");
            Console.Read();
        }
    }
}
