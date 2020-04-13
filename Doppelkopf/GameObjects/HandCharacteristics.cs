using System;
namespace Doppelkopf.GameObjects
{
    // which characteristics apply to a given hand, is basis for solos
    public class HandCharacteristics
    {
        public HandCharacteristics(bool hochzeit, bool armut, bool reshufflePossible)
        {
            Hochzeit = hochzeit;
            Armut = armut;
            ReshufflePossible = reshufflePossible;
        }

        public bool Hochzeit { get; }
        public bool Armut { get; }
        public bool ReshufflePossible { get; }
    }
}
