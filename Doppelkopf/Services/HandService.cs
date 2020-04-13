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
            return new HandCharacteristics( 
                cards.Count(card => card.value == Card.Value.QUEEN && card.suit == Card.Suit.CLUB) == 2, // Hochzeit?
                true,
                false
                );
        }
    }
}
