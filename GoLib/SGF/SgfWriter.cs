using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GoLib
{
    public static class SgfWriter
    {
        private const int MAX = 5000;

        public static String ToSGF(Goban goban)
        {
            var sgf = new StringBuilder("(;GM[1]FF[4]CA[UTF-8]AP[Kifu-Windows8]ST[2]", MAX);
            AddInfos(sgf, goban.Info);
            AddMoves(sgf, goban.Moves);
            sgf.Append(")");
            return sgf.ToString();
        }

        static void AddInfos(StringBuilder sgf, GameInfo info)
        {
            sgf.Append("SZ[" + info.Size.ToString() + "]");
            sgf.Append("KM[" + info.Komi.ToString("F", new CultureInfo("en-US")) + "]");
            sgf.Append("RU[" + info.Rule.ToString() + "]");
        }

        static void AddMoves(StringBuilder sgf, List<Move> moves)
        {
            foreach (var move in moves)
            {
                sgf.Append(";" + Convert(move.Stone));
            }
        }

        public static string Convert(Stone stone)
        {
            return stone.Color.ToString()[0] + "[" + Convert(stone.Point) + "]";
        }

        public static string Convert(Point point)
        {
            if (point.X != -1)
            {
                char x = (char)((int)'a' + point.X - 1);
                char y = (char)((int)'a' + point.Y - 1);
                return x.ToString() + y.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}
