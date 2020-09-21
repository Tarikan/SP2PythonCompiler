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
    }
}