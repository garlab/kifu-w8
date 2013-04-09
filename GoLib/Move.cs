using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib
{
    public class Move
    {
        public Stone Stone { get; set; }
        public HashSet<Stone> Captured { get; set; }
        public Stone Ko { get; set; }
        public string Comment { get; set; }
        public string Name { get; set; }

        public Move()
            : this(null, null)
        {
        }

        public Move(Stone stone, HashSet<Stone> captured)
        {
            Stone = stone;
            Captured = captured;
            Ko = null;
        }
    }
}
