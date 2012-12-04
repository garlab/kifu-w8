using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib
{
    public enum Rule
    {
        Chinese, Japanese
    }

    public class Score
    {
        private Goban _goban;
        private LocalScore[] _score;

        public Score(Goban goban)
        {
            _goban = goban;
            _score = new LocalScore[2];
            ComputeScore();
        }

        public double Get(Colour color)
        {
            return _score[(int)color - 1].score;
        }

        private void ComputeScore()
        {
            if (_goban.Handicap == 0)
            {
                _score[0].komi = _goban.Rule == Rule.Japanese ? 6.5 : 7.5;
            }
            else
            {
                _score[1].komi = 0.5;
            }
            foreach (var group in _goban.Groups.Where(g => g.Alive))
            {
                _score[(int)group.Color - 1].stones += group.Stones.Count;
            }
            foreach (var group in _goban.Groups.Where(g => !g.Alive))
            {
                _score[(int)group.Color - 1].captured += group.Stones.Count;
            }
            foreach (var move in _goban.Moves.Where(m => m.Captured != null))
            {
                _score[(int)move.Stone.Color.OpponentColor() - 1].captured += move.Captured.Count;
            }
            foreach (var territory in _goban.Territories.Where(t => t.Color != Colour.Shared))
            {
                _score[(int)territory.Color - 1].territories += territory.Points.Count;
            }
            _score[0].score = Total(_score[0], _goban.Rule);
            _score[1].score = Total(_score[1], _goban.Rule);
        }

        private static double Total(LocalScore ls, Rule rule)
        {
            switch (rule)
            {
                case Rule.Chinese: return ls.territories + ls.stones + ls.komi;
                case Rule.Japanese: return ls.territories - ls.captured + ls.komi;
                default: return 0;
            }
        }

        private struct LocalScore
        {
            public double komi;
            public int territories;
            public int stones;
            public int captured;
            public double score;
        }
    }
}
