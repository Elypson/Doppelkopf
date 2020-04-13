using System;
namespace Doppelkopf.GameObjects
{
    public struct Card
    {
        public enum Value
        {
            ACE,
            KING,
            QUEEN,
            JACK,
            TEN,
            NINE               
        };

        public enum Suit
        {
            DIAMOND,
            HEART,
            SPADE,
            CLUB
        };

        public Card(Value value, Suit suit)
        {
            this.value = value;
            this.suit = suit;
        }

        // how many points does it provide in a trick
        public int Points()
        {
            switch(value)
            {
                case Value.ACE: return 11;
                case Value.TEN: return 10;
                case Value.KING: return 4;
                case Value.QUEEN: return 3;
                case Value.JACK: return 2;
                default: return 0;
            }
        }

        public override string ToString()
        {
            string result = "";
            switch(value)
            {
                case Value.ACE: result = "A"; break;
                case Value.KING: result = "K"; break;
                case Value.QUEEN: result = "Q"; break;
                case Value.JACK: result = "J"; break;
                case Value.TEN: result = "T"; break;
                case Value.NINE: result = "9"; break;
            }

            switch(suit)
            {
                case Suit.DIAMOND: result += "d"; break;
                case Suit.HEART: result += "h"; break;
                case Suit.SPADE: result += "s"; break;
                case Suit.CLUB: result += "c"; break;
            }

            return result;
        }

        public readonly Value value;
        public readonly Suit suit;
    }
}
