using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lab.Parser;

namespace Lab
{
    public class AsmGenerator
    {
        private readonly Ast _base;

        private List<string> _functionProtoNames;
    
        private List<string> _functions;

        private List<string> _statements;

        private const string ProcTemplate = "{0} PROC\n" +
                                       "mov eax, {1}\n" +
                                       "ret\n" +
                                       "{0} ENDP\n";

        private const string ProtoTemplate = "{0} PROTO\n";
        
        private string _templateMasm = ".386\n" +
                                      ".model flat,stdcall\n" +
                                      "option casemap:none\n\n" +
                                      "_main        PROTO\n\n" +
                                      "{0}\n" + // insert prototype of functions
                                      ".data\n" +
                                      ".code\n" +
                                      "_start:\n" +
                                      "\tinvoke  _main\n" +
                                      "_main PROC\n\n" +
                                      "{1}" + // insert code
                                      "\n\tret\n\n" +
                                      "_main ENDP\n\n" +
                                      "{2}" + // insert functions
                                      "END _start\n";

        public AsmGenerator(Ast Base)
        {
            _base = Base;
            _functions = new List<string>();
            _statements = new List<string>();
            _functionProtoNames = new List<string>();
        }

        public void GenerateAsm()
        {
            GetFunctions();
            GetCalls();

            using (FileStream fs = File.Create(
                "output.asm"))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(
                    string.Format(_templateMasm, string.Join("", _functionProtoNames.ToArray()),
                    string.Join("", _statements.ToArray()),
                    string.Join("", _functions.ToArray())));
                fs.Write(info, 0, info.Length);
            }
        }

        private void GetFunctions()
        {
            foreach (DefStatement defStatement in _base.root.GetChildren().Where(
                obj => obj.GetType() == typeof(DefStatement)))
            {
                var ret = GenerateReturn(defStatement.Return);
                
                _functionProtoNames.Add(string.Format(ProtoTemplate, defStatement.Name));
                
                _functions.Add(string.Format(ProcTemplate, defStatement.Name, ret));
            }
        }

        private string GenerateBinExpr(BinOp e)
        {
            string code;
            var a = GenerateExpr(e.LeftExpression);
            var b = GenerateExpr(e.RightExpression);
            if (e.Op == TokenKind.PLUS)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\nadd eax, ecx\npush eax";
            }
            else if (e.Op == TokenKind.MINUS)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\nsub eax, ecx\npush eax";
            }
            else
            {
                throw new CompilerException($"Sorry, but {e.Op.ToString()} not implemented yet");
            }

            Console.WriteLine(code);
            return code;
        }

        private string GenerateUnExpr(UnOp e)
        {
            string code = "";
            var expr = GenerateExpr(e.Expression);
            if (e.Op == TokenKind.MINUS)
            {
                code = expr + $"\npop eax\nneg eax\npush eax";
            }
            return code;
        }

        private string GenerateConstExpr(ConstExpression e)
        {
            return $"\npush {e.Data}";
        }

        private string GenerateReturn(Expression ret)
        {
            return $"\n{GenerateExpr(ret)}\npop eax\nret";
        }

        private string GenerateExpr(Expression e)
        {
            return e switch
            {
                BinOp binop => GenerateBinExpr((BinOp) e),
                UnOp unop => GenerateUnExpr((UnOp) e),
                ConstExpression constExpression => GenerateConstExpr((ConstExpression) e),
                _ => throw new CompilerException(e.GetType().ToString())
            };
        }

        private void GetCalls()
        {
            foreach (CallStatement callStatement in _base.root.GetChildren().Where(
                obj => obj.GetType() == typeof(CallStatement)))
            {
                _statements.Add(StatementCodeFactory.GenerateCode(callStatement));
            }
        }

        private void GetExpr()
        {
            foreach (Expression expression in _base.root.GetChildren().Where(
                obj => obj.GetType() == typeof(Expression)))
            {
                //Console.WriteLine("aaaaa");
            }
        }
    }

    public static class StatementCodeFactory
    {
        private static readonly Dictionary<Type, string> TemplateDict = new Dictionary<Type, string>()
        {
            {typeof(CallStatement), "\tcall {0}\n"}
        };

        public static string GenerateCode(Statement st)
        {
            return st switch
            {
                CallStatement callStatement => string.Format(TemplateDict[st.GetType()], st.Name),
                _ => throw new CompilerException("Ooops, unknown type, seems like this feature is in development")
            };
        }
    }
}
