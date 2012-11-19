using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib
{
    public enum Colour
    {
        None, Black, White
    }

    public static class ColorExtension
    {
        public static Colour OpponentColor(this Colour color)
        {
            if (color == Colour.Black)
            {
                return Colour.White;
            }
            else if (color == Colour.White)
            {
                return Colour.Black;
            }
            else
            {
                return Colour.None;
            }
        }
    }

    public class Goban
    {
        private int _size;
        private Colour _first;
        private Section[,] _board;
        private List<Move> _moves;

        public Goban(int size, Colour first)
        {
            _size = size;
            _first = first;
            _board = new Section[size + 2, size + 2];
            _moves = new List<Move>();
            Sentinels(size);
        }

        private void Sentinels(int size)
        {
            var sentinel = new Stone(Colour.None, new Point(-1, -1));
            for (int i = 0; i < size - 2; ++i)
            {
                _board[0, i].stone = sentinel;
                _board[i, 0].stone = sentinel;
                _board[size + 1, i].stone = sentinel;
                _board[i, size + 1].stone = sentinel;
            }
        }

        public int Size
        {
            get { return _size; }
        }

        public int Round
        {
            get { return _moves.Count; }
        }

        public List<Move> Moves
        {
            get { return _moves; }
        }

        public Colour CurrentColour
        {
            get { return Round % 2 == 0 ? _first : _first.OpponentColor(); }
        }

        public bool isMoveValid(Stone stone)
        {
            if (isEmpty(stone.Point) && !isKo(stone))
            {
                return ActualLiberties(stone).Count > 0 || CaptureValue(stone) > 0;
            }
            else
            {
                return false;
            }
        }

        private HashSet<Point> ActualLiberties(Stone stone)
        {
            var liberties = new HashSet<Point>(Liberties(stone));
            foreach (var neighbor in GroupNeighbors(stone, true))
            {
                foreach (var liberty in neighbor.Liberties)
                {
                    liberties.Add(liberty);
                }
            }
            liberties.Remove(stone.Point);
            return liberties;
        }

        private int CaptureValue(Stone stone)
        {
            int value = 0;
            foreach (StoneGroup neighbor in GroupNeighbors(stone))
            {
                if (neighbor.Color == stone.Color.OpponentColor() && neighbor.Liberties.Count == 1)
                {
                    value += neighbor.Stones.Count;
                }
            }
            return value;
        }

        private bool isEmpty(Point p)
        {
            return _board[p.X, p.Y].stone == null;
        }

        private bool isKo(Stone stone)
        {
            return _moves.Count != 0 && stone == _moves[_moves.Count - 1].Ko;
        }

        private IEnumerable<Point> Neighbors(Point p)
        {
            yield return new Point(p.X + 1, p.Y);
            yield return new Point(p.X, p.Y + 1);
            yield return new Point(p.X - 1, p.Y);
            yield return new Point(p.X, p.Y - 1);
        }

        private IEnumerable<Stone> StoneNeighbors(Point p)
        {
            var neighbors = new HashSet<Stone>() { _board[p.X + 1, p.Y].stone, _board[p.X, p.Y + 1].stone, _board[p.X - 1, p.Y].stone, _board[p.X, p.Y - 1].stone };
            neighbors.Remove(null);
            return neighbors;
        }

        private IEnumerable<Point> Liberties(Stone stone)
        {
            foreach (var point in Neighbors(stone.Point))
            {
                if (_board[point.X, point.Y].stone == null)
                {
                    yield return point;
                }
            }
        }

        private IEnumerable<StoneGroup> GroupNeighbors(Stone stone, bool sameColor = false)
        {
            var groups = new HashSet<StoneGroup>();
            foreach (var neighbor in StoneNeighbors(stone.Point))
            {
                if (neighbor.Color != Colour.None && (!sameColor || stone.Color == neighbor.Color))
                {
                    groups.Add(_board[neighbor.Point.X, neighbor.Point.Y].stoneGroup);
                }
            }
            return groups;
        }

        public Move Move(Stone stone)
        {
            _board[stone.Point.X, stone.Point.Y].stone = stone;
            AddGroup(stone);

            var move = new Move(stone);
            RemoveNeighborLiberties(move);
            UpdateKo(move);
            _moves.Add(move);
            return move;
        }

        private void UpdateKo(Move move)
        {
            int x = move.Stone.Point.X;
            int y = move.Stone.Point.Y;
            if (_board[x, y].stoneGroup.Stones.Count == 1
                && _board[x, y].stoneGroup.Liberties.Count == 1
                && move.Captured.Count == 1)
            {
                move.Ko = move.Captured[0];
            }
        }

        // TODO: improve merge method strategy
        private void AddGroup(Stone stone)
        {
            var group = new StoneGroup(stone, Liberties(stone));
            _board[stone.Point.X, stone.Point.Y].stoneGroup = group;
            foreach (var neighbor in GroupNeighbors(stone, true))
            {
                Merge(group, neighbor);
            }
        }

        private void Merge(StoneGroup group, StoneGroup toMerge)
        {
            group.Merge(toMerge);
            foreach (var stone in toMerge.Stones)
            {
                _board[stone.Point.X, stone.Point.Y].stoneGroup = group;
            }
        }

        private void RemoveNeighborLiberties(Move move)
        {
            foreach (var neighbor in GroupNeighbors(move.Stone))
            {
                neighbor.Liberties.Remove(move.Stone.Point);
                if (neighbor.Liberties.Count == 0 && neighbor.Color != move.Stone.Color)
                {
                    Capture(neighbor, move);
                }
            }
        }

        private void Capture(StoneGroup group, Move move)
        {
            foreach (var stone in group.Stones)
            {
                addNeighborLiberties(stone);
                move.Captured.Add(stone);
                _board[stone.Point.X, stone.Point.Y].stone = null;
                _board[stone.Point.X, stone.Point.Y].stoneGroup = null;
            }
        }

        private void addNeighborLiberties(Stone stone)
        {
            foreach (var neighbor in GroupNeighbors(stone))
            {
                if (neighbor.Color == stone.Color.OpponentColor())
                {
                    neighbor.Liberties.Add(stone.Point);
                }
            }
        }

        private struct Section
        {
            public Stone stone;
            public StoneGroup stoneGroup;
        }
    }
}
