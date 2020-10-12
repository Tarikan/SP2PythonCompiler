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
        
        private static readonly Dictionary<Type, string> TemplateDict = new Dictionary<Type, string>()
        {
            {typeof(CallStatement), "\tcall {0}\n"},
            {typeof(AssignStatement), "{0}\n\tpop eax\n\tmov dword ptr[ebp-{1}], eax\n"},
            {typeof(ExprStatement), "\t{0}\n"}
        };

        private const string ProcTemplate = "{0} PROC\n" +
                                       "{1}\n" +
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
                                      "\tpush ebp\n" + 
                                      "\tmov ebp, esp\n" +
                                      "\tsub ebp, {3}\n"+
                                      "{1}" + // insert code
                                      "\tadd ebp, {3}\n"+
                                      "\tmov esp, ebp\n" +
                                      "\tpop ebp\n" +
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
                "3-8-CSHARP-IO-81-Ivanyshyn.asm"))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(
                    string.Format(_templateMasm, string.Join("", _functionProtoNames.ToArray()),
                    string.Join("", _statements.ToArray()),
                    string.Join("", _functions.ToArray()),
                    (_base.GetVarLen() * 4).ToString()));
                fs.Write(info, 0, info.Length);
            }
        }

        private void GetFunctions()
        {
            foreach (DefStatement defStatement in _base.root.GetChildren().Where(
                obj => obj.GetType() == typeof(DefStatement)))
            {
                var Bodystatements = new StringBuilder();
                foreach (var statement in defStatement.Statements)
                {
                    Bodystatements.Append(GenerateCode(statement));
                    Bodystatements.Append('\n');
                }
                
                //var ret = GenerateReturn(defStatement.Return);

                Bodystatements.Append(GenerateReturn(defStatement.Return));

                //Console.WriteLine(ret);
                
                _functionProtoNames.Add(string.Format(ProtoTemplate, defStatement.Name));
                
                _functions.Add(string.Format(ProcTemplate, defStatement.Name, Bodystatements.ToString()));
            }
        }

        private string GenerateBinExpr(BinOp e)
        {
            string code;
            var a = GenerateExpr(e.LeftExpression);
            var b = GenerateExpr(e.RightExpression);
            if (e.Op == TokenKind.PLUS)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\nadd eax, ecx\npush eax\n";
            }
            else if (e.Op == TokenKind.MINUS)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\nsub eax, ecx\npush eax\n";
            }
            else if (e.Op == TokenKind.STAR)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\nimul ecx\npush edx\n";
            }
            else if (e.Op == TokenKind.SLASH)
            {
                code = $"{b}\n{a}\npop eax\npop ebx\nxor edx, edx\ndiv ebx\npush eax\n";
            }
            else if (e.Op == TokenKind.EQEQUAL)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\ncmp eax, ecx\nmov eax, 0\nsete al\npush eax\n";
            }
            else if (e.Op == TokenKind.NOTEQUAL)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\ncmp eax, ecx\nmov eax, 0\nsetne al\npush eax\n";
            }
            else if (e.Op == TokenKind.GREATER)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\ncmp ecx, eax\nmov eax, 0\nset al\npush eax\n";
            }
            else if (e.Op == TokenKind.LESS)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\ncmp eax, ecx\nmov eax, 0\nset al\npush eax\n";
            }
            else if (e.Op == TokenKind.GREATEREQUAL)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\ncmp eax, ecx\nmov eax, 0\nsetge al\npush eax\n";
            }
            else if (e.Op == TokenKind.LESSEQUAL)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\ncmp ecx, eax\nmov eax, 0\nsetge al\npush eax\n";
            }
            else
            {
                throw new CompilerException($"Sorry, but {e.Op.ToString()} not implemented yet");
            }

            //Console.WriteLine(code);
            return code;
        }

        private string GenerateUnExpr(UnOp e)
        {
            string code = "";
            var expr = GenerateExpr(e.Expression);
            if (e.Op == TokenKind.MINUS)
            {
                code = expr + $"\npop eax\nneg eax\npush eax\n";
            }
            else
            {
                throw new CompilerException($"Sorry, but {e.Op.ToString()} not implemented yet");
            }
            return code;
        }

        private string GenerateConstExpr(ConstExpression e)
        {
            return $"\tpush {e.Data}";
        }

        private string GenerateVarExpr(VarExpression e)
        {
            return $"mov eax, dword ptr[ebp - {_base.GetIndex(e.varName)}]\n" +
                   $"push eax";
        }

        private string GenerateReturn(Expression ret)
        {
            return $"{GenerateExpr(ret)}\npop eax\nret";
        }

        private string GenerateExpr(Expression e)
        {
            return e switch
            {
                BinOp binop => GenerateBinExpr(binop),
                UnOp unop => GenerateUnExpr(unop),
                ConstExpression constExpression => GenerateConstExpr(constExpression),
                VarExpression varExpression => GenerateVarExpr(varExpression),
                _ => throw new CompilerException(e.GetType().ToString(),  e.Row, e.Column)
            };
        }

        private void GetCalls()
        {
            foreach (CallStatement callStatement in _base.root.GetChildren().Where(
                obj => obj.GetType() == typeof(CallStatement)))
            {
                _statements.Add(GenerateCode(callStatement));
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

        private string GenerateCode(Statement st)
        {
            return st switch
            {
                CallStatement callStatement => string.Format(TemplateDict[st.GetType()],
                    callStatement.Name),
                AssignStatement assignStatement =>
                    string.Format(TemplateDict[assignStatement.GetType()],
                        GenerateExpr(assignStatement.VarExpr),
                        (_base.GetIndex(assignStatement.VarName)).ToString()),
                ExprStatement exprStatement =>
                    string.Format(TemplateDict[exprStatement.GetType()],
                        GenerateExpr(exprStatement.expr)),
                _ => throw new CompilerException(
                    $"Ooops, unknown type, seems like this feature is in development {st.GetType()}")
            };
        }
    }
}
