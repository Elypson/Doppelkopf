using System;
using System.Linq;
using Doppelkopf.GameObjects;
using Doppelkopf.Models;

namespace Doppelkopf.Controllers
{
    public partial class GameController
    {
        public State handlePreMoveState(ClientMessage message)
        {
            if(currentPlayersToAct[currentPlayerToActID].User.Token == message.Token)
            {
                if(message.Type == Message.MessageType.Game && message.SubType == "preMove")
                {
                    if(Enum.TryParse(message.Text, true /* ignoreCase */, out GameType gameType))
                    {
                        gameAnnouncements[currentPlayersToAct[currentPlayerToActID]] = gameType;

                        // 0) check if announcement is valid (only applies to Armut, Hochzeit or Schmeissen)
                        // 1) send to all that player made specific announcement
                        
                        if (++currentPlayerToActID == 4)
                        {
                            // 2) send to all that game by player xy is played now
                            currentPlayerToActID = 0;
                            return State.Move;
                        }

                        return State.Premove;
                    }
                    else
                    {
                        sendService.SendSyncToClient(clientControllers.First(c => c.Token == currentPlayersToAct[currentPlayerToActID].User.Token).Socket, new ServerMessage
                        {
                            Type = Message.MessageType.Game,
                            SubType = "error",
                            Text = "invalidGame"
                        });
                    }
                }
            }

            return state;
        }
    }
}
