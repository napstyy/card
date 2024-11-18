namespace CardGame
{
    public class Card
    {
        public enum Ranks
        {
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6,
            Seven = 7,
            Eight = 8,
            Nine = 9,
            Ten = 10,
            Jack = 11,
            Queen = 12,
            King = 13,
            Ace = 14
        }

        public enum Suits
        {
            Spades,
            Hearts,
            Diamonds,
            Clubs
        }


        public Ranks rank;
        public Suits suit;

        public Card(Ranks ranks, Suits suit)
        {
            this.rank = ranks;
            this.suit = suit;
        }
    }
}