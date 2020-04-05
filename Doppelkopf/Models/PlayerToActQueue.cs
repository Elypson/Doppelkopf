using System;
using System.Collections.Generic;
using System.Linq;
using Doppelkopf.Services;

namespace Doppelkopf.Models
{
    // this class determines which players should act in which order, it generates new 
    public class PlayerToActQueue
    {
        private readonly List<Player> players;
        private readonly Queue<List<Player>> playersToAct = new Queue<List<Player>>();

        public PlayerToActQueue(List<Player> players)
        {
            this.players = players;
        }

        public void ResetPlayersToAct()
        {
            playersToAct.Clear();

            // create permutations based on all players that are active
            var activePlayers = players.Where(player => !player.SittingOut);

            if(activePlayers.Count() < 4)
            {
                return;
            }

            var permutations = PermutationBuilderService.GetKCombs(activePlayers, 4);

            foreach(var permutation in permutations)
            {
                playersToAct.Enqueue(permutation.ToList());
            }
        }

        public List<Player> GetPlayersToAct()
        {
            if(playersToAct.Count == 0)
            {
                ResetPlayersToAct();
            }

            return playersToAct.Dequeue();
        }
    }
}
