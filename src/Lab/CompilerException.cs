using System;

namespace Lab
{
    public class CompilerException : Exception
    {
        public CompilerException()
        {
            
        }
        
        public CompilerException(string str)
            : base(str)
        {
            
        }

        public CompilerException(string str, int row, int col)
            : base(Program.GenerateExceptionWithCode(str, row, col))
        {
            
        }
    }
}