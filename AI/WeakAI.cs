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

        private double ValueOld(Stone stone)
        {
            double value = 0;

            int numberOfliberties = _goban.ActualLiberties(stone).Count;
            int neighborLiberties = ActualNeigborLiberties(stone).Count;

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

        public double Value(Stone stone)
        {
            double saveValue = 0;
            double captureValue = 0;
            double libertyValue = 0;
            double territoryValue = 0;
            double libertyReduced = 0;

            int actualNumberOfLiberties = _goban.ActualLiberties(stone).Count;
            int actualNeighborLiberties = ActualNeigborLiberties(stone).Count;

            libertyValue = (double)(actualNumberOfLiberties - actualNeighborLiberties) / 8;

            foreach (StoneGroup groupNeighbor in _goban.GroupNeighbors(stone))
            {
                if (groupNeighbor.Color == stone.Color)
                {
                    if (groupNeighbor.Liberties.Count == 1 && actualNumberOfLiberties > 1)
                    {
                        saveValue += groupNeighbor.Stones.Count;
                        territoryValue += groupNeighbor.Stones.Count;
                    }
                }
                else if (groupNeighbor.Color == stone.Color.OpponentColor())
                {
                    if (groupNeighbor.Liberties.Count == 1)
                    {
                        captureValue += (double)(groupNeighbor.Stones.Count);
                        territoryValue += groupNeighbor.Stones.Count;
                    }
                    else
                    {
                        if (actualNumberOfLiberties > 1)
                        {
                            libertyReduced = (double)((double)(groupNeighbor.Stones.Count * 2) / (double)(groupNeighbor.Liberties.Count - 1));
                        }
                    }
                }
            }

            if (actualNumberOfLiberties == 1 && captureValue <= 0)
            {
                saveValue = 0;
                territoryValue = 0;
                libertyReduced = 0;
            }

            return saveValue + captureValue + libertyValue + territoryValue + libertyReduced;
        }

        private HashSet<Point> ActualNeigborLiberties(Stone stone)
        {
            var liberties = new HashSet<Point>();
            foreach (var neighbor in _goban.GroupNeighbors(stone, true))
            {
                liberties.UnionWith(neighbor.Liberties);
            }
            return liberties;
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
