
using System.Globalization;
namespace GoLib
{
    public class GameInfo
    {
        private int _handicap;
        private Rule _rule;
        private Player[] _players;

        public GameInfo()
        {
            _players = new Player[2];
            _players[0] = new Player(Colour.Black);
            _players[1] = new Player(Colour.White);
        }

        public int Size { get; set; }

        public double Komi { get; set; }

        public Player[] Players
        {
            get { return _players; }
        }

        public int Handicap
        {
            get { return _handicap; }
            set
            {
                _handicap = value;
                UpdateKomi();
            }
        }

        public Rule Rule
        {
            get { return _rule; }
            set
            {
                _rule = value;
                UpdateKomi();
            }
        }

        private void UpdateKomi()
        {
            double v = _rule == Rule.Japanese ? 6.5 : 7.5;
            Komi = _handicap == 0 ? v : 0.5;
        }

        public override string ToString()
        {
            return "SZ[" + Size.ToString() + "]"
                + "HA[" + Handicap.ToString() + "]"
                + "KM[" + Komi.ToString("F", new CultureInfo("en-US")) + "]"
                + "RU[" + Rule.ToString() + "]";
        }
    }
}
