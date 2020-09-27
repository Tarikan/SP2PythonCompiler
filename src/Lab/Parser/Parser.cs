using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab.Parser
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
                    case TokenKind.INT:
                    case TokenKind.MINUS:
                    case TokenKind.TILDE:
                    case TokenKind.EXCLAMINATION:
                    case TokenKind.LPAR:
                    {
                        var temp = ParseExpr();
                        Console.WriteLine(temp.ToString());
                        _base.root.AddChild(temp);
                        this.MatchIndentationCurrent();
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
            if (this.MatchBool(TokenKind.NEWLINE))
            {
                this.Match(TokenKind.INDENT);
                def.Return = this.MatchReturn();
                this.MatchCurrent(TokenKind.NEWLINE);
                this.Match(TokenKind.DEDENT);
            }
            else
            {
                this.MatchCurrent(TokenKind.RETURN);
                def.Return = ParseExpr();
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
                    //Console.WriteLine(_enumerator.Current.Kind.ToString());
                    throw new SyntaxException("Got " + _enumerator.Current.Kind.ToString() + $", {l.ToString()} expected" +
                                              $" at {_enumerator.Current.row}:{_enumerator.Current.column}",
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

        private bool MatchBool(TokenKind l)
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
        
        private bool MatchCurrentBool(TokenKind l)
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
                if (!MatchCurrentBool(TokenKind.NEWLINE) &&
                    !MatchCurrentBool(TokenKind.SEMI)&&
                    !MatchCurrentBool(TokenKind.DEDENT))
                {
                    Console.WriteLine(_enumerator.Current.ToString());
                    throw new SyntaxException("Expected new line or semicolon");
                }
            }
        }

        private void MatchIndentationCurrent()
        {
            if (!MatchCurrentBool(TokenKind.NEWLINE) &&
                !MatchCurrentBool(TokenKind.SEMI)&&
                !MatchCurrentBool(TokenKind.DEDENT))
            {
                Console.WriteLine(_enumerator.Current.ToString());
                throw new SyntaxException("Expected new line or semicolon");
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

        private Expression MatchReturn()
        {
            this.Match(TokenKind.RETURN);
            var ErrRow = _enumerator.Current.row;
            var ErrCol = _enumerator.Current.column;
            if (_enumerator.MoveNext())
            {
                var returnExpr = this.ParseExpr();
                return returnExpr;
            }
            throw new SyntaxException($"Expected token at {ErrRow}:{ErrCol}",
                ErrRow, ErrCol);
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

        private Expression ParseExpr()
        {
            //Console.WriteLine(_enumerator.Current.Kind.ToString() + "Expr");
            var first = ParseTerm();
            while (
                MatchCurrentBool(TokenKind.PLUS) || 
                MatchCurrentBool(TokenKind.MINUS))
            {
                var op = _enumerator.Current.Kind;
                var ErrRow = _enumerator.Current.row;
                var ErrCol = _enumerator.Current.column;
                if (_enumerator.MoveNext())
                {
                    var second = ParseTerm();
                    first = new BinOp(first.Row,
                        first.Column,
                        op,
                        first,
                        second
                    );
                }
            }
            first.PrintOp(0);
            
            return first;
        }

        private Expression ParseTerm()
        {
            //Console.WriteLine(_enumerator.Current.Kind.ToString() + "Term");
            var first = ParseFactor();
            while (_enumerator.MoveNext() &&
                   (MatchCurrentBool(TokenKind.STAR) || 
                   MatchCurrentBool(TokenKind.SLASH)))
            {
                var op = _enumerator.Current.Kind;
                var ErrRow = _enumerator.Current.row;
                var ErrCol = _enumerator.Current.column;
                if (_enumerator.MoveNext())
                {
                    var second = ParseFactor();
                    first = new BinOp(first.Row,
                        first.Column,
                        op,
                        first,
                        second
                    );
                }
                else
                {
                    throw new SyntaxException($"Expected token at {ErrRow}:{ErrCol}", ErrRow, ErrCol);
                }
            }

            //Console.WriteLine(first.GetType().ToString());

            return first;
        }
        
        private Expression ParseFactor()
        {
            //Console.WriteLine(_enumerator.Current.Kind.ToString() + " Factor");
            if (MatchCurrentBool(TokenKind.LPAR))
            {
                if (_enumerator.MoveNext())
                {
                    var expr = ParseExpr();
                    MatchCurrent(TokenKind.RPAR);
                    return expr;
                }
            }

            if (MatchCurrentBool(TokenKind.MINUS) ||
                MatchCurrentBool(TokenKind.TILDE) ||
                MatchCurrentBool(TokenKind.EXCLAMINATION))
            {
                var row = _enumerator.Current.row;
                var col = _enumerator.Current.column;
                var op = _enumerator.Current.Kind;
                if (_enumerator.MoveNext())
                {
                    return new UnOp(row,
                        col,
                        op,
                        ParseFactor());
                }
                else
                {
                    throw new SyntaxException($"Expected token at {row}:{col}", row, col);
                }
            }

            if (MatchCurrentBool(TokenKind.INT))
            {
                return new ConstExpression(_enumerator.Current.row,
                    _enumerator.Current.column,
                    _enumerator.Current.data);
            }
            throw new SyntaxException($"Unexpected token {_enumerator.Current.Kind.ToString()} at {_enumerator.Current.row}:{_enumerator.Current.column}",
                _enumerator.Current.row, _enumerator.Current.column);
        }

        public Ast GetAst()
        {
            return _base;
        }

    }
}
