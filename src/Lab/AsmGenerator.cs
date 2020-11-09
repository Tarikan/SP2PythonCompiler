using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lab.Interfaces;
using Lab.Parser;

namespace Lab
{
    public class AsmGenerator
    {
        private readonly Ast _base;

        private IVariableTableContainer _currentNameSpace;

        private List<string> _functionProtoNames;
    
        private List<string> _functions;

        private List<string> _statements;

        private string _currModule = "MyModule";
        private int _currentFreeId = 0;
        
        private static readonly Dictionary<Type, string> TemplateDict = new Dictionary<Type, string>()
        {
            {typeof(CallStatement), "call {0}\n"},
            {typeof(AssignStatement), "{0}\n\tpop eax\n\tmov dword ptr[ebp{1}], eax\n"},
            {typeof(ExprStatement), "{0}\n"},
            {typeof(ConditionalElseStatement), "{0}" +
                                           "pop eax\n" +
                                           "cmp eax, 0\n" +
                                           "je {1}else\n" +
                                           "{2}" +
                                           "jmp {1}final\n" +
                                           "{1}else:\n" +
                                           "{3}" +
                                           "{1}final:\n"},
            {typeof(ConditionalStatement), "{0}" +
                                           "pop eax\n" +
                                           "cmp eax, 0\n" +
                                           "je {1}else\n" +
                                           "{2}" +
                                           "{1}else:\n"}
            
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
                                      "push ebp\n" + 
                                      "mov ebp, esp\n" +
                                      "sub ebp, {3}\n"+
                                      "invoke  _main\n" +
                                      "add ebp, {3}\n"+
                                      "mov esp, ebp\n" +
                                      "pop ebp\n" +
                                      "ret\n" +
                                      "_main PROC\n\n" +
                                      "\n" +
                                      "{1}" + // insert code
                                      "\n" +
                                      "\nret\n\n" +
                                      "_main ENDP\n\n" +
                                      "{2}" + // insert functions
                                      "END _start\n";

        public AsmGenerator(Ast Base)
        {
            _base = Base;
            _functions = new List<string>();
            _statements = new List<string>();
            _functionProtoNames = new List<string>();
            _currentNameSpace = Base;
        }

        public void GenerateAsm()
        {
            foreach (var child in _base.root.GetChildren())
            {
                _statements.Add(GenerateCode(child));
            }

            using (FileStream fs = File.Create(
                "5-8-CSHARP-IO-81-Ivanyshyn.asm"))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(
                    string.Format(_templateMasm, string.Join("", _functionProtoNames.ToArray()),
                    string.Join("", _statements.ToArray()),
                    string.Join("", _functions.ToArray()),
                    (_currentNameSpace.GetVarLen() * 4).ToString()));
                fs.Write(info, 0, info.Length);
            }
        }

        private string GenerateFunction(DefStatement defStatement)
        {
            var oldNameSpace = _currentNameSpace;
            _currentNameSpace = defStatement;
            var bodystatements = new StringBuilder();
            if (defStatement.VarCounter != 0)
            {
                bodystatements.Append($"push ebp\nmov ebp, esp\nsub ebp, {defStatement.VarCounter * 4}\n");
            }
            
            foreach (var statement in defStatement.GetChildren())
            {
                //Console.WriteLine(statement.GetType() + statement.Row.ToString() + ':' + statement.Column.ToString());
                bodystatements.Append(GenerateCode(statement));
                bodystatements.Append('\n');
            }

            if (defStatement.Return != null)
            {
                bodystatements.Append(GenerateReturn(defStatement.Return));
            }
            if (defStatement.VarCounter != 0)
            {
                bodystatements.Append($"add ebp, {defStatement.VarCounter * 4}\nmov esp, ebp\npop ebp\n");
            }

            bodystatements.Append($"ret {defStatement.Args.Count * 4}\n");
            
            _currentNameSpace = oldNameSpace;
            _functionProtoNames.Add(string.Format(ProtoTemplate, defStatement.Name));
                
            _functions.Add(string.Format(ProcTemplate, defStatement.Name, bodystatements.ToString()));
            return "\n";
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
                code = $"{b}\n{a}\npop eax\npop ecx\nimul ecx\npush eax\n";
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
                code = $"{b}\n{a}\npop eax\npop ecx\ncmp ecx, eax\nmov eax, 0\nsetl al\npush eax\n";
            }
            else if (e.Op == TokenKind.LESS)
            {
                code = $"{b}\n{a}\npop eax\npop ecx\ncmp eax, ecx\nmov eax, 0\nsetl al\npush eax\n";
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
            return $"\tpush {e.Data}\n";
        }

        private string GenerateVarExpr(VarExpression e)
        {
            return $"mov eax, dword ptr[ebp{GetVarOffset(e.varName)}] ; {e.varName}\n" +
                   $"push eax\n";
        }

        private string GenerateReturn(Expression ret)
        {
            return $"{GenerateExpr(ret)}\npop eax\n";
        }

        private string GenerateCallExpression(CallExpression e)
        {
            var st = new StringBuilder();
            e.Args.Reverse();
            if (e.Args.Count > 0)
            {
                foreach (var arg in e.Args)
                {
                    st.Append(GenerateExpr(arg));
                }
            }

            st.Append($"invoke {e.name}\n push eax\n");
            return st.ToString();
        }

        private string GenerateConditionalExpression(ConditionalExpression e)
        {
            var currId = GenerateId();
            if (e.elseBody != null)
            {
                return $"{GenerateExpr(e.condition)}\npop eax\ncmp eax, 0\nje {currId}else\n" +
                       $"{GenerateExpr(e.body)}\njmp {currId}final\n" +
                       $"{currId}else:\n{GenerateExpr(e.elseBody)}\n" +
                       $"{currId}final:\n";
            }
            return $"{GenerateExpr(e.condition)}\npop eax\ncmp eax, 0\nje {currId}final\n" +
                   $"{GenerateExpr(e.body)}\n" +
                   $"{currId}final:\n";
            
        }

        private string GenerateExpr(Expression e)
        {
            return e switch
            {
                BinOp binop => GenerateBinExpr(binop),
                UnOp unop => GenerateUnExpr(unop),
                ConstExpression constExpression => GenerateConstExpr(constExpression),
                VarExpression varExpression => GenerateVarExpr(varExpression),
                CallExpression callExpression => GenerateCallExpression(callExpression),
                ConditionalExpression conditionalExpression => GenerateConditionalExpression(conditionalExpression),
                _ => throw new CompilerException(e.GetType().ToString(),  e.Row, e.Column)
            };
        }

        private string GenerateId()
        {
            return $"{_currModule}{_currentFreeId++}";
        }

        private string GetVarOffset(string var)
        {
            if (_currentNameSpace.GetVarIndex(var) < 0)
            {
                return $"+{-_currentNameSpace.GetVarIndex(var)}";
            }

            return $"-{_currentNameSpace.GetVarIndex(var)}";
        }

        private string TrimPush(string s)
        {
            if (s.EndsWith("push eax\n"))
            {
                return s.Substring(0, s.IndexOf("push eax\n", StringComparison.Ordinal));
            }

            return s;
        }

        private string GenerateCode(AstNode st)
        {
            //Console.WriteLine(st.GetType());
            return st switch
            {
                CallStatement callStatement => string.Format(TemplateDict[st.GetType()],
                    callStatement.Name),
                BlockStatement blockStatement =>
                    string.Join('\n',
                        blockStatement.GetChildren()
                            .Select(c => GenerateCode(c) + '\n').ToArray()),
                //blockStatement.GetChildren().S,
                AssignStatement assignStatement =>
                    string.Format(TemplateDict[assignStatement.GetType()],
                        GenerateExpr(assignStatement.VarExpr),
                        GetVarOffset(assignStatement.VarName)),
                ExprStatement exprStatement =>
                    string.Format(TemplateDict[exprStatement.GetType()],
                        TrimPush(GenerateExpr(exprStatement.expr))),
                ConditionalElseStatement conditionalElseStatement =>
                    string.Format(TemplateDict[conditionalElseStatement.GetType()],
                        GenerateExpr(conditionalElseStatement.Condition),
                        GenerateId(),
                        GenerateCode(conditionalElseStatement.GetChildren()[0]),
                        GenerateCode(conditionalElseStatement.GetChildren()[1])
                    ),
                ConditionalStatement conditionalStatement =>
                    string.Format(TemplateDict[conditionalStatement.GetType()],
                        GenerateExpr(conditionalStatement.Condition),
                        GenerateId(),
                        GenerateCode(conditionalStatement.GetChildren()[0])),
                DefStatement defStatement =>
                    GenerateFunction(defStatement),
                _ => throw new CompilerException(
                    $"Ooops, unknown type, seems like this feature is in development {st.GetType()}" +
                    $" {st.Row + 1}:{st.Column + 1}")
            } ?? throw new Exception();
        }
    }
}
