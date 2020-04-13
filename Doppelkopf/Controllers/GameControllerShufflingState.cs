using System;
using System.Collections.Generic;
using System.Linq;
using Doppelkopf.GameObjects;
using Doppelkopf.Models;

namespace Doppelkopf.Controllers
{
    partial class GameController
    {
        private State handleShufflingState(ClientMessage message)
        {
            var rand = new Random();

            assignedCards.Clear();

            foreach(var card in allCards)
            {
                assignedCards[currentPlayersToAct[rand.Next(4)]].Add(card);
            }

            foreach(var playerCards in assignedCards)
            {
                sendService.SendTo(new[]{ clientControllers.First(c => c.Token == playerCards.Key.User.Token).Socket}, new ServerMessage
                {
                    Type = Message.MessageType.Game,
                    SubType = "cards",
                    Text = string.Join(",", playerCards.Value)
                });
            }
            

            return State.Premove;
        }
    }
}
