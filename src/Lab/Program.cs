using System;
using System.Linq;

namespace Lab
{
    class Program
    {
        static void Main(string[] args)
        {
            string code = "def main():\n" +
                          $"\treturn \"fuck yeah\"\n";
            
            Console.WriteLine(code);

            Lexer l = new Lexer(code);
            
            l.GetTokens();

            foreach (var t in l.GetTokensList())
            {
                Console.WriteLine(t.ToString());
            }
        }
    }
}
