using System;
using System.Collections.Generic;
using System.Security;

namespace Lab
{
    public class Parser
    {
        private Dictionary<string, Ast> _defAst;
        
        private List<Token> _tokens;

        private List<Token>.Enumerator _enumerator;
        
        private readonly Ast _base; 

        public Parser(List<Token> tokens)
        {
            _defAst = new Dictionary<string, Ast>();

            _tokens = tokens;
            
            _base = new Ast(new Token());

            var en = _tokens.GetEnumerator();
            
            _enumerator = _tokens.GetEnumerator();

            while (en.MoveNext())
            {
                var token = en.Current;
                switch (token.Kind){
                    case TokenKind.DEF:
                    {
                        this.ParseDef();
                        break;
                    }
                    case TokenKind.NAME: {
                        
                        break;
                    }
                    default:
                        //Console.WriteLine(token.data + " " + token.Kind);
                        break;
                }
            }
        }

        private Statement ParseDef()
        {
            this.Match(TokenKind.DEF);
            this.Match(TokenKind.NAME);
            this.Match(TokenKind.LPAR);
            this.Match(TokenKind.RPAR);
            this.Match(TokenKind.COLON);
            this.Match(TokenKind.NEWLINE);
            this.Match(TokenKind.INDENT);
            this.MatchReturn();
            this.Match(TokenKind.NEWLINE);
            this.Match(TokenKind.DEDENT);
            
            Console.WriteLine("def parsed");
            return null;
        }

        private static List<Token> ParseArgs(List<Token>.Enumerator en)
        {
            var res = new List<Token>();
            while (en.MoveNext())
            {
                if (en.Current != null && (en.Current.Kind == TokenKind.NAME ||
                                           en.Current.Kind == TokenKind.INT || en.Current.Kind == TokenKind.FLOAT ||
                                           en.Current.Kind == TokenKind.HEXNUM || en.Current.Kind == TokenKind.BINNUM ||
                                           en.Current.Kind == TokenKind.OCTNUM || en.Current.Kind == TokenKind.STRING))
                {
                    res.Add(en.Current);
                }
                else if (en.Current != null && en.Current.Kind == TokenKind.COMMA)
                {
                    // pass
                }
                else if (en.Current != null)
                {
                    Fail(3, en.Current);
                }
                else
                {
                    Fail(-1, token:en.Current);
                }
            }

            return res;
        }
        
        private static List<Token> ParseRequiredArgs(List<Token>.Enumerator en)
        {
            var res = new List<Token>();
            while (en.MoveNext())
            {
                if (en.Current != null && en.Current.Kind == TokenKind.NAME)
                {
                    res.Add(en.Current);
                }
                else if (en.Current != null && en.Current.Kind == TokenKind.COMMA)
                {
                    // pass
                }
                else if (en.Current != null)
                {
                    Fail(3, en.Current);
                }
                else
                {
                    Fail(-1, token:en.Current);
                }
            }

            return res;
        }

        private static List<Expression> ParseExpressions(List<Token>.Enumerator en)
        {
            var res = new List<Expression>();
            // TODO: parse method
            return res;
        }
        
        private static void Fail(int errId, Token token)
        {
            string msg = "";

            switch (errId){
            case 0: {
                msg = "Incorrect tab count";
                break;
            }
            case 1: {
                msg = "Incorrect type";
                break;
            }
            case 2: {
                msg = "Cannot cast to INT";
                break;
            }
            case 3: {
                msg = "Unexpected token";
                break;
            }
            case 4: {
                msg = "Unknown method call";
                break;
            }
            default: msg = "Unknown error";
                break;
            }
        throw new CompilerException(msg + $" at {token.row}:{token.column}");
    }

        private void Match(dynamic l)
        {
            if (_enumerator.MoveNext())
            {
                if (l != _enumerator.Current.Kind)
                {
                    throw new SyntaxException();
                }
            }
            else
            {
                throw new SyntaxException();
            }
        }

        private void MatchConst()
        {
            if (_enumerator.MoveNext())
            {
                if (_enumerator.Current.Kind != TokenKind.INT &&
                    _enumerator.Current.Kind != TokenKind.FLOAT &&
                    _enumerator.Current.Kind != TokenKind.STRING)
                {
                    throw new SyntaxException();
                }
            }
            else
            {
                throw new SyntaxException();
            }
        }

        private void MatchReturn()
        {
            this.Match(TokenKind.RETURN);
            this.MatchConst();
        }

    }

    public abstract class Statement : AstNode
    {
        protected Statement(int row, int col) : base(row, col)
        {
        }
    }

    public class Expression : AstNode
    {
        public Expression(int row, int col) : base(row, col)
        {
        }
    }

    public class Operator : AstNode
    {
        public Operator(int row, int col) : base(row, col)
        {
        }
    }

    public class DefStatement : Statement
    {
        public string Name { get; set; }

        public List<Token> Args;

        public List<Expression> Expressions;
        
        #nullable enable
        public Token ?Return;
        #nullable disable

        public DefStatement(int row, int col) : base(row, col)
        {
            Expressions = new List<Expression>();
            
            Args = new List<Token>();
        }

        public void AddExpression(Expression exp)
        {
            Expressions.Add(exp);
        }

        public List<Expression> GetExpressions()
        {
            return Expressions;
        }
    }

    public class ReturnExpression : Expression
    {
        private Token ReturnValue { get; set; }
        
        public ReturnExpression(int row, int col) : base(row, col)
        {
        }
    }

}
