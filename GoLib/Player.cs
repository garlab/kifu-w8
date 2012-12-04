namespace GoLib
{
    public class Player
    {
        private Colour _color;

        public Player(Colour colour)
        {
            _color = colour;
        }

        public Colour Color
        {
            get { return _color; }
        }

        public bool IsHuman { get; set; }
    }
}
