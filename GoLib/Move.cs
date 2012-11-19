using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib
{
    public class Move
    {
        private Stone _stone;
        private List<Stone> _captured;
        private Stone _ko;

        public Move(Stone stone)
        {
            _stone = stone;
            _captured = new List<Stone>();
            _ko = null;
        }

        public Stone Stone
        {
            get { return _stone; }
        }

        public List<Stone> Captured
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
