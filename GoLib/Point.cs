using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib
{
    public class Point
    {
        private readonly int _x;
        private readonly int _y;

        public Point()
        {
            _x = -1;
            _y = -1;
        }

        public Point(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public int X { get { return _x; } }
        public int Y { get { return _y; } }

        public override bool Equals(object obj)
        {
            Point p = obj as Point;
            return this == p;
        }

        public override int GetHashCode()
        {
            return _x * 67 + _y;
        }

        public static bool operator ==(Point a, Point b)
        {
            if (Object.ReferenceEquals(a,b)) {
                return true;
            }
            
            if ((object)a == null || (object)b == null) {
                return false;
            }
            
            return a._x == b._x && a._y == b._y;    
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }
    }
}
