namespace GoLib
{
    public class Player
    {
        public Colour Color { get; private set; }
        public string Name { get; set; }
        public bool IsHuman { get; set; }

        public Player(Colour colour)
        {
            Color = colour;
            Name = colour.ToString();
        }
    }
}
