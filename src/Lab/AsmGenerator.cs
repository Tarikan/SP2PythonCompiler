using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Lab
{
    public class AsmGenerator
    {
        private readonly Ast _base;

        private List<string> _functionNames;
        
        private List<string> _functionProtoNames;

        private List<string> _functions;

        private List<string> _calls;

        private List<string> _statements;

        private const string _procTemplate = "{0} PROC\n" +
                                       "mov eax, {1}\n" +
                                       "ret\n" +
                                       "{0} ENDP\n";

        private const string _protoTemplate = "{0} PROTO\n";
        
        private string _templateMasm = ".386\n" +
                                      ".model flat,stdcall\n" +
                                      "option casemap:none\n\n" +
                                      "include     G:\\masm32\\include\\windows.inc\n" +
                                      "include     G:\\masm32\\include\\kernel32.inc\n" +
                                      "include     G:\\masm32\\include\\masm32.inc\n" +
                                      "includelib  G:\\masm32\\lib\\kernel32.lib\n" +
                                      "includelib  G:\\masm32\\lib\\masm32.lib\n\n" +
                                      "_main        PROTO\n\n" +
                                      "{0}\n" + // insert prototype of functions
                                      ".data\n" +
                                      "buff        db 11 dup(?)\n\n" +
                                      ".code\n" +
                                      "_start:\n" +
                                      "\tinvoke  _main\n" +
                                      "\tinvoke  _NumbToStr, ebx, ADDR buff\n" +
                                      "\tinvoke  StdOut,eax\n" +
                                      "\tinvoke  ExitProcess,0\n\n" +
                                      "_main PROC\n\n" +
                                      "\t{1}" + // insert code
                                      "\n\tret\n\n" +
                                      "_main ENDP\n\n" +
                                      "{2}" + // insert functions
                                      "END _start\n";

        public AsmGenerator(Ast Base)
        {
            _base = Base;
            _functions = new List<string>();
            _functionNames = new List<string>();
            _statements = new List<string>();
            _functionProtoNames = new List<string>();
        }

        public void GenerateAsm()
        {
            GetFunctions();
            GetCalls();
            
            //Console.WriteLine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName);
            
            using (FileStream fs = File.Create(
                Directory.GetParent(
                    System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName + 
                "/output.asm"))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(
                    string.Format(_templateMasm, string.Join("", _functionProtoNames.ToArray()),
                    string.Join("", _statements.ToArray()),
                    string.Join("", _functions.ToArray())));
                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }

            // Console.WriteLine(string.Format(_templateMasm, string.Join("", _functionProtoNames.ToArray()),
            //     string.Join("", _statements.ToArray()),
            //     string.Join("", _functions.ToArray())));
        }

        private void GetFunctions()
        {
            foreach (DefStatement defStatement in _base.root.GetChildren().Where(
                obj => obj.GetType() == typeof(DefStatement)))
            {
                _functionNames.Add(defStatement.Name);
                
                _functionProtoNames.Add(string.Format(_protoTemplate, defStatement.Name));
                
                _functions.Add(string.Format(_procTemplate, defStatement.Name, defStatement.Return.data));
            }
        }

        private void GetCalls()
        {
            foreach (CallStatement callStatement in _base.root.GetChildren().Where(
                obj => obj.GetType() == typeof(CallStatement)))
            {
                _statements.Add(StatementCodeFactory.GenerateCode(callStatement));
            }
        }
    }

    public static class StatementCodeFactory
    {
        private static readonly Dictionary<Type, string> TemplateDict = new Dictionary<Type, string>()
        {
            {typeof(CallStatement), "call {0}\n"}
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
