using System.Collections.Generic;
using System.Linq;

namespace Lab
{
    public class Ast
    {
        public readonly RootNode root;
        
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