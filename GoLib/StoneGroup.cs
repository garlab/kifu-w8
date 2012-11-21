using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib
{
    public class StoneGroup
    {
        private HashSet<Stone> _stones;
        private HashSet<Point> _liberties;
        private Colour _color;

        public StoneGroup(Stone stone, IEnumerable<Point> liberties)
        {
            _stones = new HashSet<Stone>();
            _liberties = new HashSet<Point>();
            _color = stone.Color;
            Add(stone, liberties);
        }

        public void Add(Stone stone, IEnumerable<Point> liberties)
        {
            _stones.Add(stone);
            _liberties.UnionWith(liberties);
        }

        public HashSet<Point> Liberties
        {
            get { return _liberties; }
        }

        public HashSet<Stone> Stones
        {
            get { return _stones; }
        }

        public Colour Color
        {
            get { return _color; }
        }

        internal void Merge(StoneGroup toMerge)
        {
            _stones.UnionWith(toMerge._stones);
            _liberties.UnionWith(toMerge._liberties);
        }
    }
}
