using System;
using System.Linq;
using Doppelkopf.GameObjects;
using Doppelkopf.Models;
using Doppelkopf.Services;

namespace Doppelkopf.Controllers
{
    public partial class GameController
    {
        public State handlePreMoveState(ClientMessage message)
        {
            if(currentPlayersToAct[currentPlayerToActID].User.Token == message.Token)
            {
                var player = currentPlayersToAct[currentPlayerToActID];
                if(message.Type == Message.MessageType.Game && message.SubType == "preMove")
                {
                    if(Enum.TryParse(message.Text, true /* ignoreCase */, out GameType gameType))
                    {
                        bool gameTypeValid = true;
                        switch(gameType)
                        {
                            case GameType.ARMUT:
                                gameTypeValid = handCharacteristics[player].Armut;
                                break;
                            case GameType.HOCHZEIT:
                                gameTypeValid = handCharacteristics[player].Hochzeit;
                                break;
                            case GameType.RESHUFFLING:
                                gameTypeValid = handCharacteristics[player].ReshufflePossible;
                                break;
                        }

                        if (gameTypeValid)
                        {
                            gameAnnouncements[currentPlayersToAct[currentPlayerToActID]] = gameType;

                            sendService.SendTo(currentPlayersToAct.Except(new []{ player}), clientControllers, new ServerMessage
                            {
                                Type = Message.MessageType.Game,
                                SubType = "premove",
                                Username = player.User.Name,
                                Text = gameType.ToString()
                            });

                            if (++currentPlayerToActID == 4)
                            {
                                int playerToSetGameTypeID = gameAnnouncements.Values.ToList().GetDominantGameTypeIndex();
                                currentGameType = gameAnnouncements[currentPlayersToAct[playerToSetGameTypeID]];

                                sendService.SendTo(currentPlayersToAct, clientControllers, new ServerMessage
                                {
                                    Type = Message.MessageType.Game,
                                    SubType = "gameType",
                                    Username = currentPlayersToAct[playerToSetGameTypeID].User.Name,
                                    Text = currentGameType.ToString()
                                });

                                currentPlayerToActID = 0;
                                return State.Move;
                            }
                        }
                        else
                        {
                            sendService.SendTo(player, clientControllers, new ServerMessage
                            {
                                Type = Message.MessageType.Game,
                                SubType = "error",
                                Text = "unplayableGame"
                            });
                        }                        

                        return State.Premove;
                    }
                    else
                    {
                        sendService.SendSyncToClient(clientControllers.First(c => c.Token == player.User.Token).Socket, new ServerMessage
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
