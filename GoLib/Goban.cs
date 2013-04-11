using GoLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

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
                return Colour.White;
            else if (color == Colour.White)
                return Colour.Black;
            return Colour.None;
        }
    }

    public class Goban
    {
        private Section[,] _board;
        private List<Move> _moves;

        public GameInfo Info { get; private set; }
        public Score Score { get; private set; }
        public Colour First { get; private set; }
        public int[] Captured { get; private set; }
        public Tree<Move> Root { get; set; }

        public Goban(GameInfo info)
        {
            Info = info;
            Score = new Score(this);
            First = info.Handicap == 0 ? Colour.Black : Colour.White;
            _board = new Section[info.Size + 2, info.Size + 2];
            _moves = new List<Move>();
            Captured = new int[2]; // TODO: merge with score
            Init();
        }

        private void Init()
        {
            Sentinels();
            Handicap();
        }

        private void Handicap()
        {
            foreach (var h in Handicaps)
            {
                var stone = new Stone(Colour.Black, h);
                Add(stone);
            }
        }

        private void Sentinels()
        {
            int j = Info.Size + 1;
            for (int i = 1; i < j; ++i)
            {
                _board[0, i].stone =
                    _board[i, 0].stone =
                    _board[j, i].stone =
                    _board[i, j].stone = Stone.FAKE;
            }
        }

        public void Clear()
        {
            Captured[0] = Captured[1] = 0;
            Array.Clear(_board, 0, _board.Length);
            _moves.Clear();
            Score.Clear();
            Init();
        }

        #region Utils

        public IEnumerable<Point> Hoshis
        {
            get
            {
                if (Info.Size < 9)
                {
                    yield break;
                }
                int low = Info.Size == 9 ? 3 : 4;
                int high = Info.Size - low + 1;
                yield return new Point(low, low);
                yield return new Point(low, high);
                yield return new Point(high, low);
                yield return new Point(high, high);
                if (Info.Size % 2 == 1)
                {
                    int middle = (Info.Size / 2 + 1);
                    yield return new Point(middle, middle);
                    yield return new Point(low, middle);
                    yield return new Point(middle, low);
                    yield return new Point(high, middle);
                    yield return new Point(middle, high);
                }
            }
        }

        public IEnumerable<Point> Handicaps
        {
            get
            {
                var hoshis = new List<Point>(Hoshis);
                if (Info.Handicap >= 1) yield return hoshis[0];
                if (Info.Handicap >= 2) yield return hoshis[3];
                if (Info.Handicap >= 3) yield return hoshis[1];
                if (Info.Handicap >= 4) yield return hoshis[2];
                if (Info.Handicap >= 6) { yield return hoshis[5]; yield return hoshis[7]; }
                if (Info.Handicap >= 8) { yield return hoshis[6]; yield return hoshis[8]; }
                if (Info.Handicap > 4 && Info.Handicap % 2 == 1) yield return hoshis[4];
            }
        }

        #endregion

        public int Round
        {
            get { return _moves.Count; } // TODO: risque de perdre en fiabilité
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
            get { return Round % 2 == 0 ? First : First.OpponentColor(); }
        }

        public Player CurrentPlayer
        {
            get { return Info.Players[0].Color == CurrentColour ? Info.Players[0] : Info.Players[1]; }
        }

        public Stone Top
        {
            get { return Moves.Count == 0 ? null : Moves[Moves.Count - 1].Stone; }
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

        public bool IsMoveValid(Stone stone)
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
            for (int i = 1; i < Info.Size + 1; ++i)
            {
                for (int j = 1; j < Info.Size + 1; ++j)
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
                group.Territories.Clear();
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
                        _board[x, y].territory = new Territory(liberty);
                    }
                    else
                    {
                        _board[x, y].territory = _board[x, y - 1].territory;
                        _board[x, y].territory.Points.Add(liberty);
                    }
                }
                else
                {
                    if (_board[x, y - 1].territory == null)
                    {
                        _board[x, y].territory = _board[x - 1, y].territory;
                        _board[x, y].territory.Points.Add(liberty);
                    }
                    else
                    {
                        _board[x, y].territory = _board[x, y - 1].territory;
                        _board[x, y].territory.Points.Add(liberty);
                        Merge(_board[x, y].territory, _board[x - 1, y].territory);
                    }
                }
                AddOwner(_board[x, y].territory, liberty);
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
            for (int i = 1; i <= Info.Size; ++i)
            {
                for (int j = 1; j <= Info.Size; ++j)
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

            if (move.Stone.IsPass)
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
            _moves.Add(new Move(new Stone(CurrentColour, Point.Empty), null));
        }
    }
}
