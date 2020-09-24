using System;
using System.Diagnostics;
using System.Text;

namespace Lab
{
    class Program
    {

        public const string PublishPath = "";
        
        public static readonly string Code = System.IO.File.ReadAllText(PublishPath + 
                                                                        "input.txt");

        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("C# compiler for python by Taras Ivanyshyn\n" +
                              "Lab1");


            Lexer l = new Lexer(Code);
            
            l.GetTokens();
            
            
            Parser p = new Parser(l.GetTokensList());

            AsmGenerator gen = new AsmGenerator(p.GetAst());
            
            gen.GenerateAsm();
            
            stopwatch.Stop();
            
            Console.WriteLine($"Elapsed in {stopwatch.ElapsedMilliseconds} ms");
            
            Console.WriteLine("Press enter");
            Console.Read();
            
        }
        public static string GenerateExceptionWithCode(string str, int row, int col)
        {
            StringBuilder s = new StringBuilder(
                Code.Split('\n')[row].Length);
            for (int i = 0; i < col - 1; i++)
            {
                s.Append(' ');
            }

            s.Append('^');
            return str +
                   '\n' +
                   Code.Split('\n')[row] +
                   '\n' +
                   s.ToString();
        }
    }
}
