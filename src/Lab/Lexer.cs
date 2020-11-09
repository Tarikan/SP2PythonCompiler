using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Lab
{
    public class Lexer
    {
        private const char Tab = '\t';

        private readonly string _code;
        
        private List<Token> _tokens = new List<Token>();

        private int _currentLevel;

        public Lexer(string code)
        {
            _code = code;
            _currentLevel = 0;
        }

        public void GetTokens()
        {
            string[] strings = _code.Split(Environment.NewLine);
            for (int i = 0; i < strings.Length; i++)
            {
                if (ParseLine(strings[i], i))
                {
                    _tokens.Add(new Token()
                    {
                        Kind = TokenKind.NEWLINE,
                        data = @"\n",
                        row = i,
                        column = strings[i].Length
                    });
                }

                // while (_currentLevel > 0)
                // {
                //     _tokens.Add(new Token()
                //     {
                //         Kind = TokenKind.DEDENT,
                //         data = "",
                //         row = strings.Length + 1,
                //         column = 0
                //     });
                //     _currentLevel--;
                // }
            }
        }
        
        private bool ParseLine(string str, int row)
        {
            if (str.Length != 0 &&
                str[0].Equals('#') ||
                str.All(s => s == '\t'))
            {
                return false;
            }
            
            var tabsC = CountTabs(str);
            //Console.WriteLine(_currentLevel);
            if (Math.Abs(tabsC - _currentLevel) > 1)
            {
                throw new CompilerException($"Not expected indent at {row + 1}");
                //_currentLevel++;
            }
            if (tabsC - _currentLevel == 1)
            {
                _currentLevel = tabsC;
                _tokens.Add(new Token()
                {
                    Kind = TokenKind.INDENT,
                    data = "\t",
                    row = row,
                    column = 0
                });
                //Console.WriteLine(_currentLevel.ToString() + ' ' + row.ToString());
                // for (int i = 0; i < _currentLevel; i++)
                // {
                //     _tokens.Add(new Token()
                //     {
                //         Kind = TokenKind.INDENT,
                //         data = "\t",
                //         row = row,
                //         column = 0
                //     });
                // }
            }
            else if (_currentLevel - tabsC > 0)
            {
                for (int i = 0; i < _currentLevel - tabsC; i++)
                {
                    _tokens.Add(new Token()
                    {
                        Kind = TokenKind.DEDENT,
                        data = null,
                        row = row - 1
                    });
                }

                _currentLevel = tabsC;
            }

            //str = str.Remove(0, tabsC);

            //int toDel = 0;
            var pos = tabsC;

            while (pos < str.Length)
            {
                //Console.WriteLine(str[pos]);
                if (str[pos] == ' ')
                {
                    pos++;
                }
                else if (str[pos] == '#')
                {
                    return true;
                }
                else if (char.IsDigit(str[pos]))
                {
                    pos += StartsWithDigit(str, row, pos);
                }
                else if (char.IsLetter(str[pos]))
                {
                    pos += StartsWithLetter(str, row, pos);
                }
                else if (Kinds.Symbols.Contains(str[pos]))
                {
                    pos += StartsWithSym(str, row, pos);
                }
                else if (str[pos].Equals('"') || str[pos].Equals('\''))
                {
                    pos += ParseString(str, row, pos);
                }
            }

            return true;
        }

        private int StartsWithDigit(string str, int row, int col)
        {
            var pos = col;
            var type = TokenKind.INT;
            var st = new StringBuilder(str.Length - col);
            while (pos < str.Length)
            {
                if (char.IsDigit(str[pos]))
                {
                    st.Append(str[pos]);
                }
                else if (str[pos].Equals('x') ||
                         str[pos].Equals('b') ||
                         str[pos].Equals('o') ||
                         str[pos].Equals('.'))
                {
                    type = str[pos] switch
                    {
                        '.' => TokenKind.FLOAT,
                        'x' => TokenKind.HEXNUM,
                        'b' => TokenKind.BINNUM,
                        'o' => TokenKind.OCTNUM,
                        _ => type
                    };
                    st.Append(str[pos]);
                }
                else if (type == TokenKind.HEXNUM &&
                         new List<char>()
                             {
                                 'A', 'B', 'C', 'D', 'E', 'F', 'a', 'b', 'c', 'd', 'e', 'f'
                             }
                             .Contains(str[pos]))
                {
                    st.Append(str[pos]);
                }
                else
                {
                    break;
                }
                pos++;
            }

            if (type == TokenKind.INT)
            {
                int res = 0;
                //Console.WriteLine(st.ToString());
                if (int.TryParse(st.ToString(), out res))
                {
                    _tokens.Add(new Token()
                    {
                        Kind = TokenKind.INT,
                        data = res,
                        row = row,
                        column = col
                    });
                    return st.Length;
                }
            }
            else if (type == TokenKind.FLOAT)
            {
                float res = 0;
                if (float.TryParse(st.ToString(), out res))
                {
                    // _tokens.Add(new Token()
                    // {
                    //     Kind = TokenKind.FLOAT,
                    //     data = res,
                    //     row = row,
                    //     column = col
                    // });
                    _tokens.Add(new Token()
                    {
                        Kind = TokenKind.INT,
                        data = Convert.ToInt32(res),
                        row = row,
                        column = col
                    });
                    return st.Length;
                }
            }
            else if (type == TokenKind.BINNUM)
            {
                if (st[0].Equals('0') &&
                    st.ToString().Substring(2)
                        .All(num => num.Equals('0') ||
                                    num.Equals('1')) &&
                    st[1].Equals('b'))
                {
                    _tokens.Add(new Token()
                    {
                        Kind = TokenKind.INT,
                        data = Convert.ToInt32(st.ToString().Substring(2), 2),
                        row = row,
                        column = col
                    });
                    return st.Length;
                }
            }
            else if (type == TokenKind.HEXNUM)
            {
                var range = new List<char>()
                {
                    '0', '1', '2','3','4','5','6','7','8','9','A','B','C','D','E','F','a','b','c','d','e','f'
                };
                if (st[0].Equals('0') &&
                    st.ToString().Substring(2)
                        .All(num => range.Contains(num)) &&
                    st[1].Equals('x'))
                {
                    _tokens.Add(new Token()
                    {
                        Kind = TokenKind.INT,
                        data = Convert.ToInt32(st.ToString(), 16),
                        row = row,
                        column = col
                    });
                    return st.Length;
                }
            }
            else if (type == TokenKind.OCTNUM)
            {
                var range = new List<char>()
                {
                    '0', '1', '2', '3','4','5','6','7'
                };
                if (st[0].Equals('0') &&
                    st.ToString()
                        .Substring(2)
                        .All(num => range.Contains(num)) &&
                    st[1].Equals('o'))
                {
                    _tokens.Add(new Token()
                    {
                        Kind = TokenKind.INT,
                        data = Convert.ToInt32(st.ToString().Substring(2), 8),
                        row = row,
                        column = col
                    });
                    return st.Length;
                }
            }

            throw new CompilerException($"invalid syntax at {row + 1}:{col}", row, col);
        }

        private int StartsWithSym(string str, int row, int col)
        {
            var st = new StringBuilder(str.Length - col);
            var pos = col;
            while (pos < str.Length)
            {
                if (Kinds.Symbols.Contains(str[pos]))
                {
                    st.Append(str[pos]);
                    pos++;
                }
                else
                {
                    break;
                }
            }

            if (st.Length == 3)
            {
                if (Kinds.LexerThreeChars(str[col], str[col + 1], str[col + 2]) != TokenKind.OP)
                {
                    _tokens.Add(new Token()
                    {
                        Kind = Kinds.LexerThreeChars(str[col], str[col + 1], str[col + 2]),
                        data = Kinds.LexerThreeChars(str[col], str[col + 1], str[col + 2]).ToString(),
                        row = row,
                        column = col
                    });
                    return 3;
                }

                else if (Kinds.LexerTwoChars(str[col], str[col + 1]) != TokenKind.OP)
                {
                    _tokens.Add(new Token()
                    {
                        Kind = Kinds.LexerTwoChars(str[col], str[col + 1]),
                        data = Kinds.LexerTwoChars(str[col], str[col + 1]).ToString(),
                        row = row,
                        column = col
                    });
                    return 2;
                }
                else if (Kinds.LexerOneChar(str[col]) != TokenKind.OP)
                {
                    _tokens.Add(new Token()
                    {
                        Kind = Kinds.LexerOneChar(str[col]),
                        data = Kinds.LexerOneChar(str[col]).ToString(),
                        row = row,
                        column = col
                    });
                    //Console.WriteLine(Kinds.LexerOneChar(str[col]).ToString());
                    return 1;
                }
            }
            else if (st.Length == 2)
            {
                if (Kinds.LexerTwoChars(str[col], str[col + 1]) != TokenKind.OP)
                {
                    _tokens.Add(new Token()
                    {
                        Kind = Kinds.LexerTwoChars(str[col], str[col + 1]),
                        data = Kinds.LexerTwoChars(str[col], str[col + 1]).ToString(),
                        row = row,
                        column = col
                    });
                    return 2;
                }
                else if (Kinds.LexerOneChar(str[col]) != TokenKind.OP)
                {
                    _tokens.Add(new Token()
                    {
                        Kind = Kinds.LexerOneChar(str[col]),
                        data = Kinds.LexerOneChar(str[col]).ToString(),
                        row = row,
                        column = col
                    });
                    //Console.WriteLine(Kinds.LexerOneChar(str[col]).ToString());
                    return 1;
                }
            }
            else if (Kinds.LexerOneChar(str[col]) != TokenKind.OP)
            {
                _tokens.Add(new Token()
                {
                    Kind = Kinds.LexerOneChar(str[col]),
                    data = Kinds.LexerOneChar(str[col]).ToString(),
                    row = row,
                    column = col
                });
                //Console.WriteLine(Kinds.LexerOneChar(str[col]).ToString());
                return 1;
            }
            else
            {
                throw new CompilerException($"Unexpected token at {row + 1}:{col}", row, col);
            }

            return 0;
        }

        private int StartsWithLetter(string str, int row, int col)
        {
            var st = new StringBuilder(str.Length - col);
            var pos = col;
            while (pos < str.Length &&
                   (char.IsDigit(str[pos]) ||
                    char.IsLetter(str[pos]) ||
                    str[pos] == '_'))
            {
                st.Append(str[pos]);
                pos++;
            }

            if (st.ToString().Equals("def"))
            {
                _tokens.Add(new Token()
                {
                    Kind = TokenKind.DEF,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
            }
            else if (st.ToString().Equals("for"))
            {
                _tokens.Add(new Token()
                {
                    Kind = TokenKind.FOR,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
            }
            else if (st.ToString().Equals("return"))
            {
                _tokens.Add(new Token()
                {
                    Kind = TokenKind.RETURN,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
            }
            else if (st.ToString().Equals("if"))
            {
                _tokens.Add(new Token()
                {
                    Kind = TokenKind.IF,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
            }
            else if (st.ToString().Equals("else"))
            {
                _tokens.Add(new Token()
                {
                    Kind = TokenKind.ELSE,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
            }
            else if (Kinds.Keywords.Contains(st.ToString()))
            {
                _tokens.Add(new Token()
                {
                    Kind = TokenKind.KEYWORD,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
                //Console.WriteLine(_tokens[^1].ToString());
                
            }
            else
            {
                _tokens.Add(new Token()
                {
                    Kind = TokenKind.NAME,
                    data = st.ToString(),
                    row = row,
                    column = col
                });
                //Console.WriteLine(_tokens[^1].ToString());
            }
            return st.ToString().Length;
        }

        private static int CountTabs(string str)
        {
            var res = 0;

            foreach (var ch in str)
            {
                if (ch == Tab)
                {
                    res++;
                }
                else
                {
                    break;
                }
            }
            return res;
        }

        private int ParseString(string str, int row, int col)
        {
            if (str[col].Equals('"') ||
                str[col].Equals('\''))
            {
                char quote = str[col];
                //Console.WriteLine("ParseString");
                StringBuilder st = new StringBuilder(str.Length - col);
                for (int i = 0; i < str.Length - col; i++)
                {
                    if (str[i + col + 1].Equals(quote) && 
                        !str[i + col].Equals('\\'))
                    {
                        _tokens.Add(new Token()
                        {
                            Kind = TokenKind.STRING,
                            data = quote + st.ToString() + quote,
                            row = row,
                            column = col
                        });
                        break;
                    }
                    else if (i == str.Length - col - 1 &&
                             !str[i + col + 1].Equals(quote))
                    {
                        throw new CompilerException($"Expected String at {row + 1}:{str.Length}", row, str.Length);
                    }
                    else
                    {
                        st.Append(str[i + col + 1]);
                    }
                    
                }
                
                //Console.WriteLine(_tokens[^1].ToString());
                
                return st.Length + 2;
            }

            return 0;
        }

        public List<Token> GetTokensList()
        {
            return _tokens;
        }

        public void PrintTokens()
        {
            foreach (var token in _tokens)
            {
                Console.WriteLine(token.ToString());
            }
        }
    }
}
