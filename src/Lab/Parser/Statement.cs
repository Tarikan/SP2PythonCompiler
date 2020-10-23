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
	
	public class ExprStatement : Statement
	{
		public Expression expr { get; set; }
		
		public ExprStatement(int row, int col, Expression e) : base(row, col)
        {
            expr = e;
        }
	}

    public class ConditionalStatement : Statement
    {
        public Expression Condition { get; set; }

        public ConditionalStatement(int row, int col,
            Expression condition) : base(row, col)
        {
            Condition = condition;
        }
    }

    public class ConditionalElseStatement : ConditionalStatement
    {
        public ConditionalElseStatement(int row, int col, Expression condition) : base(row, col, condition)
        {
        }
    }

    class BlockStatement : Statement
    {
        public BlockStatement(int row, int col) : base(row, col)
        {
        }
    }

    public class AssignStatement : Statement
	{
		public string VarName { get; set; }
		
		public Expression VarExpr { get; set; }
		
		public AssignStatement(int row, int col, string name, Expression e) : base(row, col)
        {
            VarName = name;
            VarExpr = e;
        }
	}
    
    public class DefStatement : Statement
    {
        public List<Token> Args;
        
        
#nullable enable
        public Expression ?Return;
#nullable disable

        public DefStatement(int row, int col) : base(row, col)
        {
            Args = new List<Token>();
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
}