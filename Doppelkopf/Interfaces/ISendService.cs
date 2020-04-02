using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using DoppelkopfServer.Models;

namespace DoppelkopfServer.Interfaces
{
    public interface ISendService
    {
        // send server message to multiple recipients if socket is valid
        public void SendTo(ServerMessage message, IEnumerable<WebSocket> recipients);

        // send synchronously to one specific client
        public void SendSyncToClient(WebSocket socket, ServerMessage message, bool final = true);
    }
}
