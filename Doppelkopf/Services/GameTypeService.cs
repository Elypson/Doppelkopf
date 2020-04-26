using System;
using System.Collections.Generic;
using System.Linq;
using Doppelkopf.GameObjects;

namespace Doppelkopf.Services
{
    public static class GameTypeService
    {
        // return first index of dominant game type
        public static int GetDominantGameTypeIndex(this List<GameType> gameTypes)
        {
            // game types are already ordered by dominance!
            var allGameTypes = Enum.GetValues(typeof(GameType)).Cast<GameType>().ToList();

            // get an array of game type indices first
            var gameTypeIndices = (from gameType in gameTypes select allGameTypes.IndexOf(gameType)).ToArray();

            // get index of smallest index
            return Array.IndexOf(gameTypeIndices, gameTypeIndices.Min());
        }
    }
}
