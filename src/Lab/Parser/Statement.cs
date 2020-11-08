using System.Collections.Generic;
using System.Linq;
using Lab.Interfaces;

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
    
    public class DefStatement : Statement, IVariableTableContainer
    {
        public List<string> Args;

        public Dictionary<string, int> varTable { get; set; }
        
#nullable enable
        public Expression ?Return;
#nullable disable

        public DefStatement(int row, int col) : base(row, col)
        {
            Args = new List<string>();
            this.varTable = new Dictionary<string, int>();
        }
        
        public DefStatement(int row, int col, Dictionary<string, int> varTable) : base(row, col)
        {
            Args = new List<string>();
            this.varTable = varTable;
        }
        
        public DefStatement(int row, int col, List<string> args) : base(row, col)
        {
            Args = args;
            this.varTable = new Dictionary<string, int>();
        }
        
        public bool HaveVariable(string v)
        {
            if (varTable.ContainsKey(v))
            {
                return true;
            }

            return false;
        }

        public int GetVarIndex(string s)
        {
            return varTable[s];
        }

        public int GetVarLen()
        {
            return varTable.Count;
        }

        public void AddVar(string varName)
        {
            if (!varTable.ContainsKey(varName))
            {
                MoveIndexes();
                varTable[varName] = 4;
            }
        }

        private void MoveIndexes(int value = 4)
        {
            var indexes = varTable.Keys.ToList();
            foreach (var index in indexes)
            {
                varTable[index] += 4;
            }
        }
    }
    
    public class CallStatement : Statement
    {
        public List<Expression> Args;
        public CallStatement(int row, int col) : base(row, col)
        {
            Args = new List<Expression>();
        }
    }
}