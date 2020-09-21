using System.Collections.Generic;

namespace Lab
{
    public enum TokenKind {
        NAME,
        INT,
        FLOAT,
        HEXNUM,
        BINNUM,
        OCTNUM,
        STRING,
        NEWLINE,
        INDENT,
        DEDENT,
        LPAR,
        RPAR,
        LSQB,
        RSQB,
        COLON,
        COMMA,
        SEMI,
        PLUS,
        MINUS,
        STAR,
        SLASH,
        VBAR,
        AMPER,
        LESS,
        GREATER,
        EQUAL,
        DOT,
        PERCENT,
        LBRACE,
        RBRACE,
        EQEQUAL,
        NOTEQUAL,
        LESSEQUAL,
        GREATEREQUAL,
        TILDE,
        CIRCUMFLEX,
        LEFTSHIFT,
        RIGHTSHIFT,
        DOUBLESTAR,
        PLUSEQUAL,
        MINEQUAL,
        STAREQUAL,
        SLASHEQUAL,
        PERCENTEQUAL,
        AMPEREQUAL,
        VBAREQUAL,
        CIRCUMFLEXEQUAL,
        LEFTSHIFTEQUAL,
        RIGHTSHIFTEQUAL,
        DOUBLESTAREQUAL,
        DOUBLESLASH,
        DOUBLESLASHEQUAL,
        AT,
        ATEQUAL,
        RARROW,
        ELLIPSIS,
        COLONEQUAL,
        OP,
        KEYWORD
    }

    public class Kinds
    {
        public static List<string> Keywords = new List<string>()
        {
            "False",
            "await",
            "else",
            "import",
            "pass",
            "None",
            "break",
            "except",
            "in",
            "raise",
            "True",
            "class",
            "finally",
            "is",
            "return",
            "and",
            "continue",
            "for",
            "lambda",
            "try",
            "as",
            "def",
            "from",
            "nonlocal",
            "while",
            "assert",
            "del",
            "global",
            "not",
            "with",
            "async",
            "elif",
            "if",
            "or",
            "yield"
        };
        
        public static List<char> Symbols = new List<char>()
        {
            '%', '&', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '=', '<', '>',
            '@', '[', ']', '^', '{', '}', '~', '|'
        };
        
        public static TokenKind LexerOneChar(int c1)
        {
            switch (c1) {
                case '%': return TokenKind.PERCENT;
                case '&': return TokenKind.AMPER;
                case '(': return TokenKind.LPAR;
                case ')': return TokenKind.RPAR;
                case '*': return TokenKind.STAR;
                case '+': return TokenKind.PLUS;
                case ',': return TokenKind.COMMA;
                case '-': return TokenKind.MINUS;
                case '.': return TokenKind.DOT;
                case '/': return TokenKind.SLASH;
                case ':': return TokenKind.COLON;
                case ';': return TokenKind.SEMI;
                case '<': return TokenKind.LESS;
                case '=': return TokenKind.EQUAL;
                case '>': return TokenKind.GREATER;
                case '@': return TokenKind.AT;
                case '[': return TokenKind.LSQB;
                case ']': return TokenKind.RSQB;
                case '^': return TokenKind.CIRCUMFLEX;
                case '{': return TokenKind.LBRACE;
                case '|': return TokenKind.VBAR;
                case '}': return TokenKind.RBRACE;
                case '~': return TokenKind.TILDE;
            }
            return TokenKind.OP;
        }
        
        public static TokenKind LexerTwoChars(int c1, int c2)
        {
            switch (c1) {
            case '!':
                switch (c2) {
                case '=': return TokenKind.NOTEQUAL;
                }
                break;
            case '%':
                switch (c2) {
                case '=': return TokenKind.PERCENTEQUAL;
                }
                break;
            case '&':
                switch (c2) {
                case '=': return TokenKind.AMPEREQUAL;
                }
                break;
            case '*':
                switch (c2) {
                case '*': return TokenKind.DOUBLESTAR;
                case '=': return TokenKind.STAREQUAL;
                }
                break;
            case '+':
                switch (c2) {
                case '=': return TokenKind.PLUSEQUAL;
                }
                break;
            case '-':
                switch (c2) {
                case '=': return TokenKind.MINEQUAL;
                case '>': return TokenKind.RARROW;
                }
                break;
            case '/':
                switch (c2) {
                case '/': return TokenKind.DOUBLESLASH;
                case '=': return TokenKind.SLASHEQUAL;
                }
                break;
            case ':':
                switch (c2) {
                case '=': return TokenKind.COLONEQUAL;
                }
                break;
            case '<':
                switch (c2) {
                case '<': return TokenKind.LEFTSHIFT;
                case '=': return TokenKind.LESSEQUAL;
                case '>': return TokenKind.NOTEQUAL;
                }
                break;
            case '=':
                switch (c2) {
                case '=': return TokenKind.EQEQUAL;
                }
                break;
            case '>':
                switch (c2) {
                case '=': return TokenKind.GREATEREQUAL;
                case '>': return TokenKind.RIGHTSHIFT;
                }
                break;
            case '@':
                switch (c2) {
                case '=': return TokenKind.ATEQUAL;
                }
                break;
            case '^':
                switch (c2) {
                case '=': return TokenKind.CIRCUMFLEXEQUAL;
                }
                break;
            case '|':
                switch (c2) {
                case '=': return TokenKind.VBAREQUAL;
                }
                break;
            }
            return TokenKind.OP;
        }

        public static TokenKind LexerThreeChars(int c1, int c2, int c3)
        {
            switch (c1) {
            case '*':
                switch (c2) {
                case '*':
                    switch (c3) {
                    case '=': return TokenKind.DOUBLESTAREQUAL;
                    }
                    break;
                }
                break;
            case '.':
                switch (c2) {
                case '.':
                    switch (c3) {
                    case '.': return TokenKind.ELLIPSIS;
                    }
                    break;
                }
                break;
            case '/':
                switch (c2) {
                case '/':
                    switch (c3) {
                    case '=': return TokenKind.DOUBLESLASHEQUAL;
                    }
                    break;
                }
                break;
            case '<':
                switch (c2) {
                case '<':
                    switch (c3) {
                    case '=': return TokenKind.LEFTSHIFTEQUAL;
                    }
                    break;
                }
                break;
            case '>':
                switch (c2) {
                case '>':
                    switch (c3) {
                    case '=': return TokenKind.RIGHTSHIFTEQUAL;
                    }
                    break;
                }
                break;
            }
            return TokenKind.OP;
        }
    }
    
}