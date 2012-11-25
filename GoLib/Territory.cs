using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib
{
    public class Territory
    {
        private HashSet<Point> _points;
        private Colour _color;

        public Territory(Point point)
        {
            _points = new HashSet<Point>();
            _points.Add(point);
            _color = Colour.None;
        }

        public HashSet<Point> Points
        {
            get { return _points; }
        }

        public Colour Color
        {
            get { return _color; }
        }

        public void Add(Colour color)
        {
            if (_color == Colour.Shared || color == Colour.None)
            {
                return;
            }

            if (_color == Colour.None)
            {
                _color = color;
            }
            else if (_color != color)
            {
                _color = Colour.Shared;
            }
        }
    }
}
