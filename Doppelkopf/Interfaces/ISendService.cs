using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using Doppelkopf.Models;

namespace Doppelkopf.Interfaces
{
    public interface ISendService
    {
        // send server message to multiple recipients if socket is valid
        void SendTo(IEnumerable<WebSocket> recipients, ServerMessage message);

        // send synchronously to one specific client
        void SendSyncToClient(WebSocket socket, ServerMessage message, bool final = true);

        // send a message to a player
        void SendTo(Player player, List<IClientConnectionController> clientControllers, ServerMessage message);

        // send a message to multiple players
        void SendTo(IEnumerable<Player> players, List<IClientConnectionController> clientControllers, ServerMessage message);
    }
}
