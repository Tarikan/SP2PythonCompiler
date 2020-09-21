using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab
{
    public class Lexer
    {
        private const char Tab = '\t';

        private readonly string _code;
        
        private List<Token> _tokens = new List<Token>();

        private int _currentLevel = 0;

        public Lexer(string code)
        {
            _code = code;
        }

        public void GetTokens()
        {
            string[] strings = _code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
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
                
                while (_currentLevel > 0)
                {
                    _tokens.Add(new Token()
                    {
                        Kind = TokenKind.DEDENT,
                        data = "",
                        row = strings.Length + 1,
                        column = 0
                    });
                    _currentLevel--;
                }
            }
        }
        
        private bool ParseLine(string str, int row)
        {
            if (str[0].Equals('#'))
            {
                return false;
            }
            
            int tabsC = countTabs(str);
            int pos = 0;
            if (tabsC - _currentLevel > 1)
            {
                throw new CompilerException("Not expected indent");
            }
            else if (tabsC - _currentLevel == 1)
            {
                _currentLevel = tabsC;
                for (int i = 0; i < _currentLevel; i++)
                {
                    _tokens.Add(new Token()
                    {
                        Kind = TokenKind.INDENT,
                        data = "\t",
                        row = row,
                        column = 0
                    });
                }    
                
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
            pos = tabsC;

            while (pos < str.Length)
            {
                //Console.WriteLine(str[pos]);
                if (str[pos] == ' ')
                {
                    pos++;
                }
                else if (Char.IsDigit(str[pos]))
                {
                    pos += StartsWithDigit(str, row, pos);
                }
                else if (Char.IsLetter(str[pos]))
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
            
            return 0;
        }

        private int StartsWithSym(string str, int row, int col)
        {
            StringBuilder st = new StringBuilder(str.Length - col);
            int pos = col;
            while (pos < str.Length)
            {
                if (Char.IsSymbol(str[pos]))
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
                throw new CompilerException($"Unexpected token at {row}:{col}");
            }

            return 0;
        }

        private int StartsWithLetter(string str, int row, int col)
        {
            StringBuilder st = new StringBuilder(str.Length - col);
            int pos = col;
            while (Char.IsDigit(str[pos]) || Char.IsLetter(str[pos]))
            {
                st.Append(str[pos]);
                pos++;
            }

            if (Kinds.Keywords.Contains(st.ToString()))
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

        private int countTabs(string str)
        {
            int res = 0;

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
            if (str[col].Equals('"') || str[col].Equals('\''))
            {
                char quote = str[col];
                //Console.WriteLine("ParseString");
                StringBuilder st = new StringBuilder(str.Length - col);
                for (int i = 0; i < str.Length - col; i++)
                {
                    if (str[i + col + 1].Equals(quote) && !str[i + col].Equals('\\'))
                    {
                        _tokens.Add(new Token()
                        {
                            Kind = TokenKind.STRING,
                            data = st.ToString(),
                            row = row,
                            column = col
                        });
                        break;
                    }
                    else if (i == str.Length - col - 1 && !str[i + col + 1].Equals(quote))
                    {
                        throw new CompilerException($"Expected String at {row}:{str.Length}");
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
    }
}
