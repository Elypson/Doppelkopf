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
            WithReshufflingAtFiveKings = true;
            WithReshufflingAtEightyPoints = true;
            SoloPlayerFirstToAct = false;
            NumberOfNestedBuckRounds = 1;
            AddBuckRoundAtLostSolo = true;
            AddBuckRoundAtFullHeartTrick = !UseNines;
            AddBuckRoundAtLostContra = false;
            AddBuckRoundAtZeroGame = true;
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
        public bool WithReshufflingAtFiveKings { set; get; }
        public bool WithReshufflingAtEightyPoints { set; get; }
        public bool SoloPlayerFirstToAct { set; get; }
        public int NumberOfNestedBuckRounds { set; get; } // 0 to disable buck rounds altogether
        public bool AddBuckRoundAtLostSolo { set; get; }
        public bool AddBuckRoundAtFullHeartTrick { set; get; } // makes little sense if nines are allowed
        public bool AddBuckRoundAtLostContra { set; get; }
        public bool AddBuckRoundAtZeroGame { set; get; }
    }
}
