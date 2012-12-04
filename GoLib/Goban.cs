using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib
{
    public enum Colour
    {
        None = 0, Black = 1, White = 2, Shared = 3
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
        private GameInfo _info;
        private Colour _first;
        private Section[,] _board;
        private List<Move> _moves;

        public Goban(GameInfo info)
        {
            _info = info;
            _first = info.Handicap == 0 ? Colour.Black : Colour.White;
            _board = new Section[info.Size + 2, info.Size + 2];
            _moves = new List<Move>();
            Sentinels();
        }

        private void Sentinels()
        {
            int j = _info.Size + 1;
            for (int i = 1; i < j; ++i)
            {
                _board[0, i].stone = Stone.FAKE;
                _board[i, 0].stone = Stone.FAKE;
                _board[j, i].stone = Stone.FAKE;
                _board[i, j].stone = Stone.FAKE;
            }
        }

        public void Clear()
        {
            Array.Clear(_board, 0, _board.Length);
            _moves.Clear();
            Sentinels();
        }

        public GameInfo Info
        {
            get { return _info; }
        }

        public int Size
        {
            get { return _info.Size; }
        }

        public Rule Rule
        {
            get;
            set;
        }

        public int Handicap { get; set; }

        public Colour First
        {
            get { return _first; }
        }

        public IEnumerable<Point> Hoshis
        {
            get
            {
                if (_info.Size < 9)
                {
                    yield break;
                }
                int low = _info.Size == 9 ? 3 : 4;
                int high = _info.Size - low + 1;
                yield return new Point(low, low);
                yield return new Point(low, high);
                yield return new Point(high, low);
                yield return new Point(high, high);
                if (_info.Size % 2 == 1)
                {
                    int middle = (_info.Size / 2 + 1);
                    yield return new Point(middle, middle);
                    yield return new Point(low, middle);
                    yield return new Point(middle, low);
                    yield return new Point(high, middle);
                    yield return new Point(middle, high);
                }
            }
        }

        public int Round
        {
            get { return _moves.Count; }
        }

        public List<Move> Moves
        {
            get { return _moves; }
        }

        public IEnumerable<Territory> Territories
        {
            get
            {
                var territories = new HashSet<Territory>();
                foreach (var section in _board)
                {
                    if (section.territory != null)
                    {
                        territories.Add(section.territory);
                    }
                }
                return territories;
            }
        }

        public IEnumerable<StoneGroup> Groups
        {
            get
            {
                var groups = new HashSet<StoneGroup>();
                foreach (var section in _board)
                {
                    if (section.stoneGroup != null)
                    {
                        groups.Add(section.stoneGroup);
                    }
                }
                return groups;
            }
        }

        public Colour CurrentColour
        {
            get { return Round % 2 == 0 ? _first : _first.OpponentColor(); }
        }

        public Player CurrentPlayer
        {
            get { return _info.Players[0].Color == CurrentColour ? _info.Players[0] : _info.Players[1]; }
        }

        public Stone GetStone(Point point)
        {
            return _board[point.X, point.Y].stone;
        }

        public StoneGroup StoneGroup(Point point)
        {
            return _board[point.X, point.Y].stoneGroup;
        }

        public StoneGroup StoneGroup(Stone stone)
        {
            return _board[stone.Point.X, stone.Point.Y].stoneGroup;
        }

        public bool isMoveValid(Stone stone)
        {
            if (isEmpty(stone.Point) && !isKo(stone))
            {
                // Liberties(stone).Count > 0 || GroupNeighbors(stone, true).Any(n => n.Liberties.Count > 1) || GroupNeighbors(stone).Any(n => n.Color == stone.Color.OpponentColor() && n.Liberties.Count == 1)
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
            var v = from neighbor in GroupNeighbors(stone)
                    where neighbor.Color == stone.Color.OpponentColor() && neighbor.Liberties.Count == 1
                    select neighbor.Stones.Count;
            return v.Sum();
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

        private IEnumerable<Stone> StoneNeighbors(Stone stone, bool sameColor = false)
        {
            var stoneNeighbors = from point in Neighbors(stone.Point)
                                 let neighbor = _board[point.X, point.Y].stone
                                 where neighbor != null && (!sameColor || stone.Color == neighbor.Color)
                                 select neighbor;
            return stoneNeighbors;
        }

        public IEnumerable<Point> Liberties(Stone stone)
        {
            var liberties = from point in Neighbors(stone.Point)
                            where _board[point.X, point.Y].stone == null
                            select point;
            return liberties;
        }

        public IEnumerable<StoneGroup> GroupNeighbors(Stone stone, bool sameColor = false)
        {
            var groups = from neighbor in StoneNeighbors(stone, sameColor)
                         where neighbor != Stone.FAKE
                         select StoneGroup(neighbor);
            return groups.Distinct();
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
            var color = stone.Color.OpponentColor();
            foreach (var neighbor in GroupNeighbors(stone).Where(n => n.Color == color))
            {
                neighbor.Liberties.Add(stone.Point);
            }
        }

        #region Territories

        public void EraseTerritories()
        {
            for (int i = 0; i < _info.Size + 2; ++i)
            {
                for (int j = 0; j < _info.Size + 2; ++j)
                {
                    _board[i, j].territory = null;
                }
            }
            AllGroupsAlive();
        }

        private void AllGroupsAlive()
        {
            foreach (var group in Groups)
            {
                group.Alive = true;
            }
        }

        // Must be call only once
        public void ComputeTerritories()
        {
            foreach (var liberty in AllLiberties())
            {
                int x = liberty.X;
                int y = liberty.Y;
                if (_board[x - 1, y].territory == null)
                {
                    if (_board[x, y - 1].territory == null)
                    {
                        var territory = new Territory(liberty);
                        _board[x, y].territory = territory;
                        AddOwner(territory, liberty);
                    }
                    else
                    {
                        _board[x, y].territory = _board[x, y - 1].territory;
                        _board[x, y].territory.Points.Add(liberty);
                        AddOwner(_board[x, y].territory, liberty);
                    }
                }
                else
                {
                    if (_board[x, y - 1].territory == null)
                    {
                        _board[x, y].territory = _board[x - 1, y].territory;
                        _board[x, y].territory.Points.Add(liberty);
                        AddOwner(_board[x, y].territory, liberty);
                    }
                    else
                    {
                        _board[x, y].territory = _board[x, y - 1].territory;
                        _board[x, y].territory.Points.Add(liberty);
                        AddOwner(_board[x, y].territory, liberty);
                        Merge(_board[x, y].territory, _board[x - 1, y].territory);
                    }
                }
            }
        }

        private void Merge(Territory territory, Territory toMerge)
        {
            if (territory != toMerge)
            {
                territory.Merge(toMerge);
                foreach (var point in toMerge.Points)
                {
                    _board[point.X, point.Y].territory = territory;
                }
            }
        }

        private void AddOwner(Territory territory, Point liberty)
        {
            foreach (var neighbor in StoneNeighbors(new Stone(Colour.Black, liberty)))
            {
                if (neighbor != Stone.FAKE)
                {
                    territory.Add(StoneGroup(neighbor));
                }
            }
        }

        #endregion

        public void MarkDead(Point point)
        {
            var group = _board[point.X, point.Y].stoneGroup;
            if (group != null)
            {
                group.Alive = !group.Alive;
            }
        }

        public IEnumerable<Point> AllLiberties()
        {
            for (int i = 1; i <= Size; ++i)
            {
                for (int j = 1; j <= Size; ++j)
                {
                    if (_board[i, j].stone == null)
                    {
                        yield return new Point(i, j);
                    }
                }
            }
        }

        private struct Section
        {
            public Stone stone;
            public StoneGroup stoneGroup;
            public Territory territory;
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
    }
}
