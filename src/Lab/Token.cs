using System;

namespace Lab
{
    public class Token
    {
        public override String ToString()
        {
            //return this.Kind.ToString();
            return $"Kind is {Kind.ToString()}\n" +
                   $"data is {data.ToString()}\n" +
                   $"row is {row.ToString()}\n" +
                   $"column is {column.ToString()}\n";
        }

        public TokenKind Kind { get; set; }

        public dynamic data { get; set; }

        public int row { get; set; }

        public int column { get; set; }
    }
}