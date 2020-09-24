using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab
{
    public class Parser
    {
        private Dictionary<string, AstNode> _defAst;

        private readonly List<Token> _tokens;

        private List<Token>.Enumerator _enumerator;
        
        private readonly Ast _base; 

        public Parser(List<Token> tokens)
        {
            _defAst = new Dictionary<string, AstNode>();

            _tokens = tokens;
            
            _base = new Ast();

            using var en = _tokens.GetEnumerator();
            
            _enumerator = _tokens.GetEnumerator();

            while (_enumerator.MoveNext())
            {
                var token = _enumerator.Current;
                switch (token.Kind){
                    case TokenKind.DEF:
                    {
                        var temp = this.ParseDef();
                        _base.root.AddChild(temp);
                        break;
                    }
                    case TokenKind.NAME: {
                        var temp = this.ParseName();
                        _base.root.AddChild(temp);
                        if (!_base.root.GetChildren()
                            .Any(def =>
                                def is DefStatement d &&
                                d.Name == temp.Name))
                        {
                            throw new SyntaxException(
                                $"Name {temp.Name} is not defined at {temp.Row + 1}:{temp.Column}",
                                temp.Row, temp.Column);
                        }
                        this.MatchIndentation();
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
                
                
            }
        }

        private DefStatement ParseDef()
        {
            var def = new DefStatement(_enumerator.Current.row, _enumerator.Current.column)
            {
                Name = this.Match(TokenKind.NAME).data, Args = this.MatchArgs()
            };

            foreach (var arg in def.Args)
            {
                Console.WriteLine(arg);
            }
            this.Match(TokenKind.COLON);

            MatchDefBody(def);
            return def;
        }

        private void MatchDefBody(DefStatement def)
        {
            if (this.MatchTrue(TokenKind.NEWLINE))
            {
                this.Match(TokenKind.INDENT);
                def.Return = this.MatchReturn();
                this.Match(TokenKind.NEWLINE);
                this.Match(TokenKind.DEDENT);
            }
            else
            {
                this.MatchCurrent(TokenKind.RETURN);
                def.Return = this.MatchConst();
                // TODO: add full support for strings
                if (def.Return != null &&
                    def.Return.Kind == TokenKind.STRING &&
                    def.Return.data != null &&
                    def.Return.data.Length > 6)
                {
                    throw new CompilerException("Sorry, but strings, that contains more than 4 chars currently not supported");
                }
            }
        }

        private CallStatement ParseName()
        {
            CallStatement res = new CallStatement(_enumerator.Current.row, _enumerator.Current.column);
            res.Name = _enumerator.Current.data;

            res.Args = this.MatchArgs();

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
        throw new CompilerException(msg + $" at {token.row + 1}:{token.column}",
            token.row, token.column);
    }

        private Token Match(TokenKind l)
        {
            if (_enumerator.MoveNext())
            {
                //Console.WriteLine(_enumerator.Current.Kind.ToString());
                if (l != _enumerator.Current.Kind)
                {
                    throw new SyntaxException("Got " + _enumerator.Current.Kind.ToString() + $", {l.ToString()} expected" +
                                              $" at {_enumerator.Current.row + 1}:{_enumerator.Current.column}",
                        _enumerator.Current.row, _enumerator.Current.column);
                }
                else
                {
                    //Console.WriteLine(_enumerator.Current.ToString());
                    return _enumerator.Current;
                }
            }
            else
            {
                throw new SyntaxException();
            }
        }

        private Token MatchCurrent(TokenKind l)
        {
            if (l != _enumerator.Current.Kind)
            {
                throw new SyntaxException("Got " + _enumerator.Current.Kind.ToString() + $", {l.ToString()} expected" +
                                          $" at {_enumerator.Current.row + 1}:{_enumerator.Current.column}",
                    _enumerator.Current.row, _enumerator.Current.column);
            }
            else
            {
                //Console.WriteLine(_enumerator.Current.ToString());
                return _enumerator.Current;
            }
        }

        private bool MatchTrue(TokenKind l)
        {
            if (_enumerator.MoveNext())
            {
                if (l != _enumerator.Current.Kind)
                {
                    //Console.WriteLine(_enumerator.Current.Kind.ToString());
                    return false;
                }
                else
                {
                    //Console.WriteLine(_enumerator.Current.ToString());
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        
        private bool MatchCurrentTrue(TokenKind l)
        {
            if (l != _enumerator.Current.Kind)
            {
                return false;
            }
            else
            {
                //Console.WriteLine(_enumerator.Current.ToString());
                return true;
            }
        }

        private void MatchIndentation()
        {
            if (_enumerator.MoveNext())
            {
                if (!MatchCurrentTrue(TokenKind.NEWLINE) &&
                    !MatchCurrentTrue(TokenKind.SEMI)&&
                    !MatchCurrentTrue(TokenKind.DEDENT))
                {
                    throw new SyntaxException("Expected new line or semicolon");
                }
            }
        }

        private Token MatchConst()
        {
            if (_enumerator.MoveNext())
            {
                if (_enumerator.Current.Kind != TokenKind.INT &&
                    _enumerator.Current.Kind != TokenKind.FLOAT &&
                    _enumerator.Current.Kind != TokenKind.STRING)
                {
                    throw new SyntaxException();
                }
                else
                {
                    //Console.WriteLine(_enumerator.Current.ToString());
                    return _enumerator.Current;
                }
            }
            else
            {
                throw new SyntaxException();
            }
        }

        private Token MatchReturn()
        {
            this.Match(TokenKind.RETURN);
            var returnToken = this.MatchConst();
            return _enumerator.Current;
        }

        private List<Token> MatchArgs()
        {
            var res = new List<Token>();
            this.Match(TokenKind.LPAR);
            while (_enumerator.MoveNext())
            {
                switch (_enumerator.Current.Kind)
                {
                    case TokenKind.NAME:
                        res.Add(_enumerator.Current);
                        _enumerator.MoveNext();
                        switch (_enumerator.Current.Kind)
                        {
                            case TokenKind.COMMA:
                                break;
                            case TokenKind.RPAR:
                                return res;
                            default:
                                throw new SyntaxException(
                                    $"Unexpected token at {_enumerator.Current.row + 1}:{_enumerator.Current.column}",
                                    _enumerator.Current.row, _enumerator.Current.column
                                    );
                        }
                        break;
                    case TokenKind.RPAR:
                        return res;
                    default:
                        throw new SyntaxException(
                            $"Unexpected token at {_enumerator.Current.row + 1}:{_enumerator.Current.column}",
                            _enumerator.Current.row, _enumerator.Current.column
                        );
                }
            }

            return res;
        }
        

        public Ast GetAst()
        {
            return _base;
        }

    }

    public abstract class Statement : AstNode
    {
        public string Name { get; set; }

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

    public class CallStatement : Statement
    {
        public List<Token> Args;
        public CallStatement(int row, int col) : base(row, col)
        {
            Args = new List<Token>();
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
