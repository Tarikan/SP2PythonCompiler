using System.Collections.Generic;
using System.Linq;

namespace Lab
{
    public class Ast
    {
        public readonly RootNode root;
        public readonly Dictionary<string, int> varTable = new Dictionary<string, int>();
        
        public Ast(RootNode root)
        {
            this.root = root;
        }

        public Ast()
        {
            root = new RootNode();
        }

        public RootNode GetRoot()
        {
            return root;
        }

        public bool IfVarExist(string v)
        {
            if (varTable.ContainsKey(v))
            {
                return true;
            }

            return false;
        }
		
		public Dictionary<string, int> GetTable()
		{
			return varTable;
		}

        public int GetIndex(string s)
        {
            return varTable[s];
        }

        public int GetVarLen()
        {
            return varTable.Count;
        }
		
		public void AddVar(string VarName)
		{
			if(!varTable.Keys.Contains(VarName))
			{
				varTable[VarName] = (varTable.Count + 1) * 4;
			}
		}
    }

    public class RootNode
    {
        private List<AstNode> _childrenNodes;

        public RootNode()
        {
            _childrenNodes = new List<AstNode>();
        }
        
        public void AddChild(AstNode child)
        {
            _childrenNodes.Add(child);
        }

        public List<AstNode> GetChildren()
        {
            return _childrenNodes;
        }
    }
}