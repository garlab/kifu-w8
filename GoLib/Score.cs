using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GoLib
{
    public enum Rules
    {
        Chinese, Japanese
    }

    public class Score
    {
        private Goban _goban;
        private LocalScore[] _score;
        private Colour _winner;
        private string _result;

        public Score(Goban goban)
        {
            _goban = goban;
            _score = new LocalScore[2];
        }

        public void Clear()
        {
            _score = new LocalScore[2];
            _winner = Colour.None;
            _result = null;
        }

        public Colour Winner
        {
            get { return _winner; }
        }

        public string Result
        {
            get { return _result; }
        }

        public double Get(Colour color)
        {
            return _score[(int)color - 1].score;
        }

        public void ComputeScore()
        {
            _score[1].komi = _goban.Info.Komi;
            foreach (var group in _goban.Groups.Where(g => g.Alive))
            {
                _score[(int)group.Color - 1].stones += group.Stones.Count;
            }
            foreach (var group in _goban.Groups.Where(g => !g.Alive))
            {
                _score[(int)group.Color - 1].captured += group.Stones.Count;
                _score[(int)group.Color.OpponentColor() - 1].territories += group.Stones.Count;
            }
            foreach (var move in _goban.Moves.Where(m => m.Captured != null))
            {
                _score[(int)move.Stone.Color.OpponentColor() - 1].captured += move.Captured.Count;
            }
            foreach (var territory in _goban.Territories.Where(t => t.Color != Colour.Shared && t.Color != Colour.None)) // TODO: remplacer ce workaround par un test en amont
            {
                _score[(int)territory.Color - 1].territories += territory.Points.Count;
            }
            _score[0].score = Total(_score[0], _goban.Info.Rule);
            _score[1].score = Total(_score[1], _goban.Info.Rule);
            _winner = Get(Colour.Black) > Get(Colour.White) ? Colour.Black : Colour.White;
            _result = _winner.ToString()[0] + "+" + Math.Abs(_score[0].score - _score[1].score).ToString(new CultureInfo("en-US"));
        }

        public void GiveUp(Colour winner)
        {
            _winner = winner;
            _result = _winner.ToString()[0] + "+R";
        }

        public override string ToString()
        {
            return _result == null ? "" : "RE[" + _result + "]";
        }

        private static double Total(LocalScore ls, Rules rule)
        {
            switch (rule)
            {
                case Rules.Chinese: return ls.territories + ls.stones + ls.komi;
                case Rules.Japanese: return ls.territories - ls.captured + ls.komi;
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
