using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib
{
    public class Stone
    {
        private readonly Colour _color;
        private readonly Point _point;

        public static Stone FAKE = new Stone(Colour.None, new Point(-1, -1));

        public Stone(Colour color, Point point)
        {
            _color = color;
            _point = point;
        }

        public Point Point { get { return _point; } }
        public Colour Color { get { return _color; } }

        public override bool Equals(object obj)
        {
            Stone s = obj as Stone;
            return this == s;
        }

        public override int GetHashCode()
        {
            return _point.GetHashCode() + (int)_color;
        }

        public static bool operator ==(Stone a, Stone b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            return a._point == b._point && a._color == b._color;
        }

        public static bool operator !=(Stone a, Stone b)
        {
            return !(a == b);
        }
    }
}
