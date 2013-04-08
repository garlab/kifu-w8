namespace GoLib
{
    public class Player
    {
        public Player(Colour colour)
        {
            Color = colour;
            Name = colour.ToString();
        }

        public Colour Color { get; private set; }

        public string Name { get; set; }

        public bool IsHuman { get; set; }
    }
}
