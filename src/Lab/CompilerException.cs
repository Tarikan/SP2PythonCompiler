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
    }
}