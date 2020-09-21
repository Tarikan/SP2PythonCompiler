using System;
using System.Linq;

namespace Lab
{
    class Program
    {
        static void Main(string[] args)
        {
            string code = "def main():\n" +
                          $"\treturn 0b01001";
            
            Console.WriteLine(code);

            Lexer l = new Lexer(code);
            
            l.GetTokens();
            
            Parser p = new Parser(l.GetTokensList());
            
            /*
            foreach (var t in l.GetTokensList())
            {
                Console.WriteLine(t.ToString());
            }*/
        }
    }
}
