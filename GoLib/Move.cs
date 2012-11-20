using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib
{
    public class Move
    {
        private Stone _stone;
        private HashSet<Stone> _captured;
        private Stone _ko;

        public Move(Stone stone, HashSet<Stone> captured)
        {
            _stone = stone;
            _captured = captured;
            _ko = null;
        }

        public Stone Stone
        {
            get { return _stone; }
        }

        public HashSet<Stone> Captured
        {
            get { return _captured; }
        }

        public Stone Ko
        {
            get { return _ko; }
            set { _ko = value; }
        }
    }
}
