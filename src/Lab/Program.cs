using System;
using System.Diagnostics;
using System.Text;
using Lab.Parser;

namespace Lab
{
    class Program
    {

        public const string PublishPath = "";
        
        public static readonly string Code = System.IO.File.ReadAllText(PublishPath + 
                                                                        "6-8-CSHARP-IO-81-Ivanyshyn.txt");

        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("C# compiler for python by Taras Ivanyshyn\n" +
                              "Lab6");

            //Console.WriteLine(Code);

            
            Lexer l = new Lexer(Code);
            
            l.GetTokens();
            
            //l.PrintTokens();
            
            var p = new Parser.Parser(l.GetTokensList());

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
            foreach (var ch in Code.Split('\n')[row])
            {
                if (ch == '\t')
                {
                    col = col + 8;
                }
                else
                {
                    break;
                }
            }
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
