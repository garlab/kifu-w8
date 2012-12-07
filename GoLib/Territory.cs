using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib
{
    public class Territory
    {
        private HashSet<Point> _points;
        private HashSet<StoneGroup> _groups;
        private Colour _color;
        private Colour _marked;

        public static event EventHandler Changed;

        protected void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        public Territory(Point point)
        {
            _points = new HashSet<Point>();
            _groups = new HashSet<StoneGroup>();
            _points.Add(point);
            _color = Colour.None;
            _marked = Colour.None;
        }

        public HashSet<Point> Points
        {
            get { return _points; }
        }

        public Colour Color
        {
            get { return _marked == Colour.None ? _color : _marked; }
            set { _color |= value; }
        }

        public Colour Mark
        {
            get { return _marked; }
            set
            {
                if (_marked == value) return;
                _marked = value;
                foreach (var group in _groups) // TODO: check ça
                {
                    group.Alive = value == Colour.None || group.Color == value;
                }
                OnChanged(EventArgs.Empty);
            }
        }

        public void Add(StoneGroup group)
        {
            _groups.Add(group);
            group.Territories.Add(this);
            Color = group.Color;
        }

        public void Merge(Territory toMerge)
        {
            foreach (var group in toMerge._groups)
            {
                group.Territories.Remove(toMerge);
                group.Territories.Add(this);
            }
            _points.UnionWith(toMerge.Points);
            _groups.UnionWith(toMerge._groups);
            Color = toMerge.Color;
        }
    }
}
