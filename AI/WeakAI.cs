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

        #region tmp

        public static event EventHandler CleanNow;
        public static event EventHandler Changed;

        protected void OnChanged(VStone sender, EventArgs e)
        {
            if (Changed != null)
                Changed(sender, e);
        }

        protected void OnClean()
        {
            if (CleanNow != null)
                CleanNow(this, EventArgs.Empty);
        }

        public class VStone
        {
            public VStone(double v, Point p)
            {
                Value = v;
                Point = p;
            }

            public double Value { get; set; }
            public Point Point { get; set; }
        }

        #endregion

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

            OnClean();
            foreach (var point in _goban.AllLiberties().OrderBy(a => Guid.NewGuid()))
            {
                var stone = new Stone(_color, point);
                if (_goban.IsMoveValid(stone))
                {
                    var value = Value(stone);
                    OnChanged(new VStone(value, stone.Point), EventArgs.Empty);
                    if (value > max)
                    {
                        max = value;
                        best = stone;
                    }
                }
            }
            return best;
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
