using System;

namespace Lab.Parser
{
    public abstract class Expression : AstNode
    {
        protected Expression(int row, int col) : base(row, col)
        {
        }
        public override string ToString()
        {
            return this.GetType().ToString();
        }

        public virtual void PrintOp(int depth)
        {
            for (int i = 0; i < depth; i++)
            {
                Console.Write('\t');
            }
            Console.WriteLine(this.GetType().ToString());
        }
    }
    
    public class ConstExpression : Expression
    {
        public readonly dynamic Data;
        public ConstExpression(int row, int col, dynamic data) : base(row, col)
        {
            Data = data;
        }

        public override void PrintOp(int depth)
        {
            base.PrintOp(depth);
            for (int i = 0; i <= depth; i++)
            {
                Console.Write('\t');
            }
            Console.WriteLine(Data);
        }
    }

    public class UnOp : Expression
    {
        public readonly TokenKind Op;

        public readonly Expression Expression;
        public UnOp(int row, int col, TokenKind op, Expression e) : base(row, col)
        {
            Op = op;
            Expression = e;
        }

        public override void PrintOp(int depth)
        {
            base.PrintOp(depth);
            for (var i = 0; i <= depth; i++)
            {
                Console.Write('\t');
            }

            Console.WriteLine(Op.ToString());
            Expression.PrintOp(depth+1);
        }
    }

    public class BinOp : Expression
    {
        public readonly TokenKind Op;

        public readonly Expression LeftExpression;

        public readonly Expression RightExpression;
        public BinOp(int row, int col, TokenKind op, Expression le, Expression re) : base(row, col)
        {
            Op = op;
            LeftExpression = le;
            RightExpression = re;
        }

        public override void PrintOp(int depth)
        {
            base.PrintOp(depth);
            //Console.WriteLine(this.ToString());
            LeftExpression.PrintOp(depth + 1);
            Console.WriteLine("\t" + Op.ToString());
            RightExpression.PrintOp(depth + 1);
        }
    }
}