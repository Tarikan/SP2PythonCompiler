using System.Collections.Generic;

namespace Lab
{
    public abstract class AstNode
    {
        private int _row { get; set; }
        
        private int _column { get; set; }

        protected AstNode(int row, int col)
        {
            _row = row;
            _column = col;
        }
    }
}
