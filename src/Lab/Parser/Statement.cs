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

        public List<Statement> Statements;
        
#nullable enable
        public Expression ?Return;
#nullable disable

        public DefStatement(int row, int col) : base(row, col)
        {
            Statements = new List<Statement>();
            
            Args = new List<Token>();
        }

        public void AddStatement(Statement st)
        {
            Statements.Add(st);
        }

        public List<Statement> GetStatements()
        {
            return Statements;
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