using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Lab.Parser
{
    public class Parser
    {
        private Dictionary<string, AstNode> _defAst;

        private readonly List<Token> _tokens;

        private readonly TwoWayEnum<Token> _enumerator;
        
        private readonly Ast _base; 

        public Parser(List<Token> tokens)
        {
            _defAst = new Dictionary<string, AstNode>();

            _tokens = tokens;
            
            _base = new Ast();

            _enumerator = new TwoWayEnum<Token>(_tokens.GetEnumerator());

            //this.Parse();
            ParseUntil(_base.root);
        }

        /*private void Parse()
        {
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
        }*/

        private void ParseUntil(RootNode baseNode, TokenKind? cancellationToken = null)
        {
            while (_enumerator.MoveNext())
            {
                var token = _enumerator.Current;
                if (token.Kind == cancellationToken)
                {
                    break;
                }
                switch (token.Kind)
                {
                    case TokenKind.DEF:
                    {
                        var temp = ParseDef();
                        baseNode.AddChild(temp);
                        break;
                    }
                    case TokenKind.NAME: {
                        //Console.WriteLine(_enumerator.Current.data);
                        if (_enumerator.MoveNext())
                        {
                            if (_enumerator.Current.Kind == TokenKind.EQUAL)
                            {
                                _enumerator.MovePrevious();
                                var name = _enumerator.Current.data;
                                _enumerator.MoveNext();
                                _enumerator.MoveNext();
                                var expr = ParseExpr();
                                baseNode.AddChild(new AssignStatement(
                                    _enumerator.Current.row,
                                    _enumerator.Current.column,
                                    name, expr));
                                if (!_base.IfVarExist(name))
                                {
                                    _base.AddVar(name);
                                }
                            }
                            else if (_enumerator.Current.Kind == TokenKind.LPAR)
                            {
                                _enumerator.MovePrevious();
                                var temp = ParseName();
                                baseNode.AddChild(temp);
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
                            else
                            {
                                _enumerator.MovePrevious();
                                baseNode.AddChild(new ExprStatement(
                                    _enumerator.Current.row,
                                    _enumerator.Current.column,
                                    ParseExpr()));
                                ;
                            }
                        }
                        break;
                    }
                    case TokenKind.IF:
                    {
                        var temp = ParseConditional();
                        baseNode.AddChild(temp);
                        _enumerator.MovePrevious();
                        break;
                    }
                    case TokenKind.INT:
                    case TokenKind.MINUS:
                    case TokenKind.TILDE:
                    case TokenKind.EXCLAMINATION:
                    case TokenKind.LPAR:
                    {
                        var temp = new ExprStatement(_enumerator.Current.row,
                            _enumerator.Current.column,
                            ParseExpr());
                        //Console.WriteLine(temp.ToString());
                        baseNode.AddChild(temp);
                        MatchIndentationCurrent();
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

            //MatchDefBody(def);
            if (MatchBool(TokenKind.NEWLINE))
            {
                ParseUntil(def, TokenKind.RETURN);
                _enumerator.MovePrevious();
                //_enumerator.MovePrevious();
                def.Return = this.MatchReturn();
                this.MatchCurrent(TokenKind.NEWLINE);
                this.Match(TokenKind.DEDENT);
            }
            else
            {
                this.MatchCurrent(TokenKind.RETURN);
                def.Return = ParseExpr();
                MatchCurrent(TokenKind.NEWLINE);
            }
            return def;
        }

        /*private void MatchDefBody(DefStatement def)
        {
            if (this.MatchBool(TokenKind.NEWLINE))
            {
                this.Match(TokenKind.INDENT);
                var move = true;
				while (_enumerator.MoveNext() && move)
				{
					var token = _enumerator.Current;
					switch (token.Kind){
						case TokenKind.NAME: {
                            if (_enumerator.MoveNext())
                            {
                                if (_enumerator.Current.Kind == TokenKind.EQUAL)
                                {
                                    _enumerator.MovePrevious();
                                    var name = _enumerator.Current.data;
                                    _enumerator.MoveNext();
                                    _enumerator.MoveNext();
                                    var expr = ParseExpr();
                                    def.AddStatement(new AssignStatement(
                                        _enumerator.Current.row,
                                        _enumerator.Current.column,
                                        name, expr));
                                    if (!_base.IfVarExist(name))
                                    {
                                        _base.AddVar(name);
                                    }
                                }
                                else
                                {
                                    _enumerator.MovePrevious();
                                    def.AddStatement(new ExprStatement(
                                        _enumerator.Current.row,
                                        _enumerator.Current.column,
                                        ParseExpr()));
                                   ;
                                }
                            }
							break;
                        }
                        case TokenKind.RETURN:
                        {
                            _enumerator.MovePrevious();
                            _enumerator.MovePrevious();
                            move = false;
                            break;
                        }
                    }
				}
                def.Return = this.MatchReturn();
                this.MatchCurrent(TokenKind.NEWLINE);
                this.Match(TokenKind.DEDENT);
            }
            else
            {
                this.MatchCurrent(TokenKind.RETURN);
                def.Return = ParseExpr();
            }
        }*/
        
        private Statement ParseName()
        {
            var res = new CallStatement(_enumerator.Current.row, _enumerator.Current.column);
            res.Name = _enumerator.Current.data;

            res.Args = this.MatchArgs();

            return res;
        }

        private Statement ParseConditional()
        {
            var rowCol = new
            {
                _enumerator.Current.row,
                _enumerator.Current.column
            };
            if (!_enumerator.MoveNext()) {throw new SyntaxException("Token expected",
                rowCol.row, rowCol.column);}
            var condition = ParseExpr();
            MatchCurrent(TokenKind.COLON);
            var body = new BlockStatement(_enumerator.Current.row,
                _enumerator.Current.column);
            
            if (!_enumerator.MoveNext()) throw new SyntaxException("Token expected",
                _enumerator.Current.row, _enumerator.Current.column);
            ParseUntil(body,
                _enumerator.Current.Kind == TokenKind.NEWLINE ? TokenKind.DEDENT : TokenKind.NEWLINE);

            if (MatchBool(TokenKind.ELSE))
            {
                var conditionalElseStatement = new ConditionalElseStatement(rowCol.row,
                    rowCol.column,
                    condition
                );
                var elseBody = new BlockStatement(_enumerator.Current.row,
                    _enumerator.Current.column);
                if (!_enumerator.MoveNext()) throw new SyntaxException("Token expected",
                    _enumerator.Current.row, _enumerator.Current.column);
                _enumerator.MoveNext();
                ParseUntil(elseBody,
                    _enumerator.Current.Kind == TokenKind.NEWLINE ? TokenKind.DEDENT : TokenKind.NEWLINE);
                conditionalElseStatement.AddChild(body);
                conditionalElseStatement.AddChild(elseBody);
                return conditionalElseStatement;
            }
            var conditionalStatement = new ConditionalStatement(rowCol.row,
                rowCol.column,
                condition
            );
            conditionalStatement.AddChild(body);
            return conditionalStatement;
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
            
            if (MatchCurrentBool(TokenKind.IF) && _enumerator.MoveNext())
            {
                var condition = ParseExpr();
                
                if (MatchCurrentBool(TokenKind.ELSE))
                {
                    _enumerator.MoveNext();
                    var elseExpression = ParseExpr();
                    return new ConditionalExpression(first.Row,
                        first.Column,
                        first,
                        condition,
                        elseExpression);
                }
            }

            if (MatchCurrentBool(TokenKind.LESS) ||
                MatchCurrentBool(TokenKind.GREATER) ||
                MatchCurrentBool(TokenKind.EQEQUAL) ||
                MatchCurrentBool(TokenKind.GREATEREQUAL) ||
                MatchCurrentBool(TokenKind.LESSEQUAL))
            {
                var op = _enumerator.Current.Kind;
                if (_enumerator.MoveNext())
                {
                    var third = ParseExpr();
                    first = new BinOp(first.Row,
                        first.Column,
                        op,
                        first,
                        third
                    );
                }
            }

            //first.PrintOp(0);
            
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
                var errRow = _enumerator.Current.row;
                var errCol = _enumerator.Current.column;
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
                    throw new SyntaxException($"Expected token at {errRow}:{errCol}", errRow, errCol);
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
			
			if (MatchCurrentBool(TokenKind.NAME))
            {
                var name = _enumerator.Current.data;
                if (_enumerator.MoveNext() &&
                    _enumerator.Current.Kind == TokenKind.LPAR)
                {
                    Match(TokenKind.RPAR);
                    //_enumerator.MovePrevious();
                    //_enumerator.MovePrevious();
                    return new CallExpression(_enumerator.Current.row,
                        _enumerator.Current.column,
                        name);
                }

                _enumerator.MovePrevious();
				if (_base.IfVarExist(_enumerator.Current.data))
				{
					return new VarExpression(_enumerator.Current.row,
						_enumerator.Current.column,
						name);
				}
                throw new SyntaxException($"Variable used before assignment " +
                                          $"{_enumerator.Current.Kind.ToString()} " +
                                          $"at {_enumerator.Current.row}:{_enumerator.Current.column}",
                    _enumerator.Current.row, _enumerator.Current.column);
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
