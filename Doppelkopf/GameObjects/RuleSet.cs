using System;
namespace Doppelkopf.GameObjects
{
    public class RuleSet
    {
        public RuleSet()
        {
            UseNines = false;
            WithArmut = true;
            WithFleischlos = true;
            CountReContraBy = ReContraCounting.ADDING_TWO;
            SecondDulleTrumpsFirst = true;
            BothPigletsTrumpAll = false;
            ReContraAtHochzeitAfterFinderTrick = true;
        }

        public bool UseNines { set;  get; }
        public bool WithArmut { set; get; }
        public bool WithFleischlos { set; get; }

        public enum ReContraCounting
        {
            ADDING_TWO,
            DOUBLING
        };

        public ReContraCounting CountReContraBy { set; get; }
        public bool SecondDulleTrumpsFirst { set; get; }
        public bool BothPigletsTrumpAll { set; get; }
        public bool ReContraAtHochzeitAfterFinderTrick { set; get; }
    }
}
