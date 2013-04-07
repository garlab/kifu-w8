using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GoLib
{
    public static class SgfWriter
    {
        private const int max = 5000;
        private const string header = "(;GM[1]FF[4]CA[UTF-8]AP[Kifu-Windows8:1]ST[2]\n";

        public static String ToSGF(Goban goban)
        {
            var sgf = new StringBuilder(header, max);
            AddInfos(sgf, goban.Info);
            AddScore(sgf, goban.Score);
            AddHandicap(sgf, goban);
            AddMoves(sgf, goban.Moves);
            AddTerritories(sgf, goban);
            return sgf.Append(")").ToString();
        }

        static void AddInfos(StringBuilder sgf, GameInfo info)
        {
            sgf.Append(info);
        }

        private static void AddScore(StringBuilder sgf, Score score)
        {
            sgf.Append(score);
        }

        static void AddHandicap(StringBuilder sgf, Goban goban)
        {
            if (goban.Info.Handicap > 0)
            {
                sgf.Append("AB");
                foreach (var handicap in goban.Handicaps)
                {
                    sgf.Append(handicap);
                }
            }
        }

        static void AddMoves(StringBuilder sgf, List<Move> moves)
        {
            foreach (var move in moves)
            {
                sgf.Append("\n;" + move.Stone);
            }
        }

        // TODO: regrouper les TW/TB entre eux
        private static void AddTerritories(StringBuilder sgf, Goban goban)
        {
            foreach (var territory in goban.Territories.Where(t => t.Color != Colour.None && t.Color != Colour.Shared))
            {
                sgf.Append("T" + territory.Color.ToString()[0] + territory.ToString());
            }
            foreach (var group in goban.Groups.Where(g => !g.Alive))
            {
                sgf.Append("T" + group.Color.OpponentColor().ToString()[0] + group.ToString());
            }
        }
    }
}
