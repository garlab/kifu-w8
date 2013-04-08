
using System.Globalization;
namespace GoLib
{
    public class GameInfo
    {
        private int _handicap = 0;
        private Rules _rule = Rules.Japanese;

        public GameInfo()
        {
            Size = 19;
            Komi = 0;
            Players = new Player[2] { new Player(Colour.Black), new Player(Colour.White) };
        }

        public int Size { get; set; }

        public double Komi { get; set; }

        public Player[] Players { get; set; }

        public int Handicap
        {
            get { return _handicap; }
            set
            {
                _handicap = value;
                UpdateKomi();
            }
        }

        public Rules Rule
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
            double v = _rule == Rules.Japanese ? 6.5 : 7.5;
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
