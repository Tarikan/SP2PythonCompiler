namespace Lab
{
    public class Ast
    {
        private readonly AstNode _root;
        
        public Ast(AstNode root)
        {
            _root = root;
        }

        public Ast(Token token)
        {
           // _root = new AstNode(token);
        }

        public AstNode GetRoot()
        {
            return _root;
        }
    }
}
