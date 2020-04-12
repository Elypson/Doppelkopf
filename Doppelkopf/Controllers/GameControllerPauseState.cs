using System;
using System.Linq;
using Doppelkopf.Models;

namespace Doppelkopf.Controllers
{
    public partial class GameController
    {
        State handlePauseState(ClientMessage message)
        {
            // if table owner decides to start, starting works

            User sourceUser = users.FirstOrDefault(user => user.Token == message.Token);

            if(sourceUser != null)
            {
                if(message.SubType == "start")
                {
                    // number of players sufficient?
                    if(players.Where(player => !player.SittingOut).Count() >= 4)
                    {
                        // playerToActQueue should work then automatically
                        return State.Shuffling;
                    }
                    else
                    {
                        sendService.SendTo(clientControllers.Select(c => c.Socket), new ServerMessage
                        {
                            Type = Message.MessageType.Game,
                            SubType = "error",
                            Text = "tooFewPlayers"
                        });
                    }
                }
            }

            return state;
        }
    }
}
