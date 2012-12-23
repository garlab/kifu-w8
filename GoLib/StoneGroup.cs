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
        private HashSet<Territory> _territories;
        private Colour _color;
        private bool _alive;

        public static event EventHandler Changed;

        protected void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        public StoneGroup(Stone stone, IEnumerable<Point> liberties)
        {
            _stones = new HashSet<Stone>();
            _liberties = new HashSet<Point>();
            _territories = new HashSet<Territory>();
            _color = stone.Color;
            _alive = true;
            Add(stone, liberties);
        }

        public void Add(Stone stone, IEnumerable<Point> liberties)
        {
            _stones.Add(stone);
            _liberties.UnionWith(liberties);
        }

        public bool Alive
        {
            get { return _alive; }
            set
            {
                if (_alive == value) return;
                _alive = value;
                var color = _alive ? Colour.None : _color.OpponentColor();
                foreach (var territory in _territories)
                {
                    territory.Mark = color;
                }
                OnChanged(EventArgs.Empty);
            }
        }

        public HashSet<Point> Liberties
        {
            get { return _liberties; }
        }

        public HashSet<Stone> Stones
        {
            get { return _stones; }
        }

        public HashSet<Territory> Territories
        {
            get { return _territories; }
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

        public override string ToString()
        {
            var sb = new StringBuilder((_stones.Count + 1) * 4);
            foreach (var stone in _stones)
            {
                sb.Append(stone.Point);
            }
            return sb.ToString();
        }
    }
}
