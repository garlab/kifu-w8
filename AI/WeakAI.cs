using GoLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AI
{
    enum Strategy
    {
        Fuseki, Normal
    }

    public class WeakAI : IAI
    {
        private Goban _goban;
        private Colour _color;
        private Strategy _strategy;

        public WeakAI(Goban goban, Colour color)
        {
            _goban = goban;
            _color = color;
            _strategy = goban.Info.Size == 19 ? Strategy.Fuseki : Strategy.Normal;
        }

        public Stone NextStone()
        {
            Stone stone = null;

            if (_strategy == Strategy.Fuseki)
            {
                stone = BestCorner();
                if (stone == null)
                {
                    _strategy = Strategy.Normal;
                    stone = NextStone();
                }
            }
            else
            {
                return Best();
            }
            return stone;
        }

        private Stone Best()
        {
            double max = -1;
            Stone best = null;

            foreach (var point in _goban.AllLiberties().OrderBy(a => Guid.NewGuid()))
            {
                var stone = new Stone(_color, point);
                if (_goban.isMoveValid(stone))
                {
                    var value = Value(stone);
                    if (value > max)
                    {
                        max = value;
                        best = stone;
                    }
                }
            }
            return best;
        }

        private double Value(Stone stone)
        {
            double value = 0;

            int numberOfliberties = _goban.ActualLiberties(stone).Count;
            int neighborLiberties = ActualNeigborLiberties(stone);

            if (neighborLiberties != 0)
            {
                value += (double)(numberOfliberties / neighborLiberties) / 8;
            }

            int capture = 0;
            

            foreach (var group in _goban.GroupNeighbors(stone))
            {
                if (group.Color == _color)
                {
                    if (group.Liberties.Count == 1 && neighborLiberties > 1)
                    {
                        capture += group.Stones.Count;
                        value += group.Stones.Count * 2;
                    }
                    else if (group.Color == _color.OpponentColor())
                    {
                        if (group.Liberties.Count == 1)
                        {
                            value += group.Stones.Count * 2;
                        }
                        else if (numberOfliberties > 1)
                        {
                            value += (double)(group.Stones.Count * 2) / (double)(group.Liberties.Count - 1);
                        }
                    }
                }

                if (numberOfliberties == 1 && capture <= 1)
                {
                    value = capture - 1;
                }
            }
            return value;
        }

        private int ActualNeigborLiberties(Stone stone)
        {
            int actualNeigborLiberties = 0;
            foreach (var neighbor in _goban.GroupNeighbors(stone, true))
            {
                actualNeigborLiberties += neighbor.Liberties.Count;
            }
            return actualNeigborLiberties;
        }

        private Stone BestCorner()
        {
            foreach (var corner in Corner())
            {
                if (_goban.isEmpty(corner))
                {
                    var stone = new Stone(_color, corner);
                    if (_goban.Liberties(stone).Count() == 4)
                    {
                        return stone;
                    }
                }
            }
            return null;
        }

        private IEnumerable<Point> Corner()
        {
            int j = _goban.Info.Size - 3;
            yield return new Point(4, 4);
            yield return new Point(4, j);
            yield return new Point(j, 4);
            yield return new Point(j, j);
        }
    }
}
