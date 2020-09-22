using System;
using System.Linq;

namespace Lab
{
    class Program
    {
        static void Main(string[] args)
        {
            string code = "def main():\n" +
                          $"\treturn 0b01001\n" +
                          $"main()";
            
            Console.WriteLine(code);

            Lexer l = new Lexer(code);
            
            l.GetTokens();
            
            
            
            //l.PrintTokens();
            
            Parser p = new Parser(l.GetTokensList());

            AsmGenerator gen = new AsmGenerator(p.GetAst());
            
            gen.GenerateAsm();
        }
    }
}
