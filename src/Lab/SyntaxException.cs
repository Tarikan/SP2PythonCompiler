using System;

namespace Lab
{
    public class SyntaxException : Exception
    {
        public SyntaxException()
        {
            
        }
        
        public SyntaxException(string str)
            : base(str)
        {
            
        }
        
        public SyntaxException(string str, int row, int col)
            : base(Program.GenerateExceptionWithCode(str, row, col))
        {
            
        }
    }
}