using System.Collections.Generic;

namespace Lab.Parser
{
    public abstract class Statement : AstNode
    {
        public string Name { get; set; }

        protected Statement(int row, int col) : base(row, col)
        {
        }
    }
    
    public class DefStatement : Statement
    {
        public List<Token> Args;

        public List<Expression> Expressions;
        
#nullable enable
        public Expression ?Return;
#nullable disable

        public DefStatement(int row, int col) : base(row, col)
        {
            Expressions = new List<Expression>();
            
            Args = new List<Token>();
        }

        public void AddExpression(Expression exp)
        {
            Expressions.Add(exp);
        }

        public List<Expression> GetExpressions()
        {
            return Expressions;
        }
    }
    
    public class CallStatement : Statement
    {
        public List<Token> Args;
        public CallStatement(int row, int col) : base(row, col)
        {
            Args = new List<Token>();
        }
    }
    
    public class ReturnStatement : Statement
    {
        private Token ReturnValue { get; set; }
        
        public ReturnStatement(int row, int col) : base(row, col)
        {
        }
    }
}