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

        public override string ToString()
        {
            if (this == Empty)
            {
                return "[]";
            }
            char x = (char)((int)'a' + _x - 1);
            char y = (char)((int)'a' + _y - 1);
            return "[" + x.ToString() + y.ToString() + "]";
        }

        public static bool operator ==(Point a, Point b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            return a._x == b._x && a._y == b._y;
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        public static Point Parse(char i, char j)
        {
            int x = Parse(i);
            int y = Parse(j);
            return new Point(x, y);
        }

        private static int Parse(char c)
        {
            if (char.IsLower(c))
                return (int)(c - 'a');
            if (char.IsUpper(c))
                return (int)(c - 'A' + 26);
            throw new FormatException();
        }

        public static Point Empty
        {
            get { return new Point(-1, -1); }
        }
    }
}
