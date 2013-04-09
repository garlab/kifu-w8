using System;
using System.Collections.Generic;
using System.Text;

namespace GoLib
{
    public class StoneGroup
    {
        public HashSet<Stone> Stones { get; private set; }
        public HashSet<Point> Liberties { get; private set; }
        public HashSet<Territory> Territories { get; private set; }
        public Colour Color { get; private set; }

        private bool _alive;

        #region Event

        public static event EventHandler Changed;

        protected void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        #endregion

        public StoneGroup(Stone stone, IEnumerable<Point> liberties)
        {
            Stones = new HashSet<Stone>();
            Liberties = new HashSet<Point>();
            Territories = new HashSet<Territory>();
            Color = stone.Color;
            _alive = true;
            Add(stone, liberties);
        }

        public void Add(Stone stone, IEnumerable<Point> liberties)
        {
            Stones.Add(stone);
            Liberties.UnionWith(liberties);
        }

        public bool Alive
        {
            get { return _alive; }
            set
            {
                if (_alive == value) return;
                _alive = value;
                var color = _alive ? Colour.None : Color.OpponentColor();
                foreach (var territory in Territories)
                {
                    territory.Mark = color;
                }
                OnChanged(EventArgs.Empty);
            }
        }

        internal void Merge(StoneGroup toMerge)
        {
            Stones.UnionWith(toMerge.Stones);
            Liberties.UnionWith(toMerge.Liberties);
        }

        public override string ToString()
        {
            var sb = new StringBuilder((Stones.Count + 1) * 4);
            foreach (var stone in Stones)
            {
                sb.Append(stone.Point);
            }
            return sb.ToString();
        }
    }
}
