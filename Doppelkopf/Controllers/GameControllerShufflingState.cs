using System;
using System.Collections.Generic;
using System.Linq;
using Doppelkopf.GameObjects;
using Doppelkopf.Models;
using Doppelkopf.Services;

namespace Doppelkopf.Controllers
{
    partial class GameController
    {
        private State handleShufflingState(ClientMessage message)
        {
            var rand = new Random();

            assignedHands.Clear();

            foreach(var card in allCards)
            {
                assignedHands[currentPlayersToAct[rand.Next(4)]].Add(card);
            }

            foreach(var playerCards in assignedHands)
            {
                handCharacteristics[playerCards.Key] = playerCards.Value.GetHandCharacteristics(ruleSet);

                sendService.SendTo(new[]{ clientControllers.First(c => c.Token == playerCards.Key.User.Token).Socket}, new ServerMessage
                {
                    Type = Message.MessageType.Game,
                    SubType = "cards",
                    Text = string.Join(",", playerCards.Value)
                });
            }

            gameAnnouncements.Clear();
            
            return State.Premove;
        }
    }
}
