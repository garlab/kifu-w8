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
            for (int i = 1; i < size - 1; ++i)
            {
                _board[0, i].stone = Stone.FAKE;
                _board[i, 0].stone = Stone.FAKE;
                _board[size + 1, i].stone = Stone.FAKE;
                _board[i, size + 1].stone = Stone.FAKE;
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

        private StoneGroup StoneGroup(Stone stone)
        {
            return _board[stone.Point.X, stone.Point.Y].stoneGroup;
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

        public HashSet<Point> ActualLiberties(Stone stone)
        {
            var liberties = new HashSet<Point>(Liberties(stone));
            foreach (var neighbor in GroupNeighbors(stone, true))
            {
                liberties.UnionWith(neighbor.Liberties);
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

        public bool isEmpty(Point p)
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

        public IEnumerable<Stone> StoneNeighbors(Stone stone, bool sameColor = false)
        {
            foreach (var point in Neighbors(stone.Point))
            {
                var neighbor = _board[point.X, point.Y].stone;
                if (neighbor != null && (!sameColor || stone.Color == neighbor.Color))
                {
                    yield return neighbor;
                }
            }
        }

        public IEnumerable<Point> Liberties(Stone stone)
        {
            foreach (var point in Neighbors(stone.Point))
            {
                if (_board[point.X, point.Y].stone == null)
                {
                    yield return point;
                }
            }
        }

        public IEnumerable<StoneGroup> GroupNeighbors(Stone stone, bool sameColor = false)
        {
            var groups = new HashSet<StoneGroup>();
            foreach (var neighbor in StoneNeighbors(stone, sameColor))
            {
                if (neighbor != Stone.FAKE)
                {
                    groups.Add(StoneGroup(neighbor));
                }
            }
            return groups;
        }

        public Move Move(Stone stone)
        {
            var move = new Move(stone, Add(stone));
            UpdateKo(move);
            _moves.Add(move);
            return move;
        }

        private HashSet<Stone> Add(Stone stone)
        {
            _board[stone.Point.X, stone.Point.Y].stone = stone;
            AddGroup(stone);
            return RemoveNeighborLiberties(stone);
        }

        private void UpdateKo(Move move)
        {
            if (StoneGroup(move.Stone).Stones.Count == 1
                && StoneGroup(move.Stone).Liberties.Count == 1
                && move.Captured.Count == 1)
            {
                move.Ko = move.Captured.First();
            }
        }

        private void AddGroup(Stone stone)
        {
            var groups = GroupNeighbors(stone, true);
            StoneGroup group;

            if (groups.Count() == 0)
            {
                group = new StoneGroup(stone, Liberties(stone));
            }
            else
            {
                group = groups.First();
                group.Stones.Add(stone);
                group.Liberties.UnionWith(Liberties(stone));
            }
            _board[stone.Point.X, stone.Point.Y].stoneGroup = group;
            foreach (var neighbor in groups)
            {
                if (neighbor != group)
                {
                    Merge(group, neighbor);
                }
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

        private HashSet<Stone> RemoveNeighborLiberties(Stone stone)
        {
            var captured = new HashSet<Stone>();
            foreach (var neighbor in GroupNeighbors(stone))
            {
                neighbor.Liberties.Remove(stone.Point);
                if (neighbor.Liberties.Count == 0 && neighbor.Color != stone.Color)
                {
                    Capture(neighbor);
                    captured.UnionWith(neighbor.Stones);
                }
            }
            return captured;
        }

        private void Capture(StoneGroup group)
        {
            foreach (var stone in group.Stones)
            {
                AddNeighborLiberties(stone);
                _board[stone.Point.X, stone.Point.Y].stone = null;
                _board[stone.Point.X, stone.Point.Y].stoneGroup = null;
            }
        }

        private void Remove(Stone stone)
        {
            var group = StoneGroup(stone);
            AddNeighborLiberties(stone);
            _board[stone.Point.X, stone.Point.Y].stone = null;
            _board[stone.Point.X, stone.Point.Y].stoneGroup = null;
            SplitGroups(group, stone);
        }

        // Eclate un groupe en plusieurs lorsqu'une pierre de jointure est enlevée du plateau.
        // Doit également etre appellé en cas de non éclatement, car force un reset des libertés
        private void SplitGroups(StoneGroup group, Stone stone)
        {
            foreach (var neighbor in StoneNeighbors(stone, true))
            {
                if (StoneGroup(neighbor) == group)
                {
                    var newGroup = new StoneGroup(neighbor, Liberties(neighbor));
                    AddNeighborStone(newGroup, neighbor);
                }
            }
        }

        // Ajoute récursivement les pierres adjacente à un groupe
        private void AddNeighborStone(StoneGroup group, Stone stone)
        {
            _board[stone.Point.X, stone.Point.Y].stoneGroup = group;
            foreach (var neighbor in StoneNeighbors(stone, true))
            {
                if (!group.Stones.Contains(neighbor))
                {
                    group.Stones.Add(neighbor);
                    group.Liberties.UnionWith(Liberties(neighbor));
                    AddNeighborStone(group, neighbor);
                }
            }
        }

        private void AddNeighborLiberties(Stone stone)
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

        // Retourne le dernier coup joué et annule celui-ci
        // Si aucun n'a été joué, ou si c'est une passe, retourne null
        public Move Undo()
        {
            if (_moves.Count == 0)
            {
                return null;
            }

            var move = _moves.Last();
            _moves.Remove(move);

            if (move.Stone == Stone.FAKE)
            {
                return null;
            }

            Remove(move.Stone);
            foreach (var captured in move.Captured)
            {
                Add(captured);
            }
            return move;
        }

        public void Pass()
        {
            _moves.Add(new Move(Stone.FAKE, null));
        }

        public IEnumerable<Point> AllLiberties()
        {
            for (int i = 1; i <= Size; ++i)
            {
                for (int j = 1; j <= Size; ++j)
                {
                    if (_board[i,j].stone == null)
                    {
                        yield return new Point(i,j);
                    }
                }
            }
        }
    }
}
