using System;
using System.Collections.Generic;
using System.Linq;
using Doppelkopf.GameObjects;

namespace Doppelkopf.Services
{
    public static class HandService
    {
        public static HandCharacteristics GetHandCharacteristics(this List<Card> cards, RuleSet rules)
        {
            bool eightyOrMorePoints = cards.Sum(card => card.Points()) >= 80;
            bool fiveKingsOrMore = cards.Count(card => card.value == Card.Value.KING) >= 5;

            return new HandCharacteristics( 
                cards.Count(card => card.value == Card.Value.QUEEN && card.suit == Card.Suit.CLUB) == 2, // Hochzeit?
                cards.Count(card => card.suit == Card.Suit.DIAMOND || card.value == Card.Value.JACK || card.value == Card.Value.QUEEN || (card.value == Card.Value.TEN && card.suit == Card.Suit.HEART)) <= 3, // Armut?
                rules.WithReshufflingAtEightyPoints && eightyOrMorePoints || rules.WithReshufflingAtFiveKings && fiveKingsOrMore // reshuffable
                );
        }
    }
}
