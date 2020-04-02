using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using Doppelkopf.Models;

namespace Doppelkopf.Interfaces
{
    public interface ISendService
    {
        // send server message to multiple recipients if socket is valid
        public void SendTo(IEnumerable<WebSocket> recipients, ServerMessage message);

        // send synchronously to one specific client
        public void SendSyncToClient(WebSocket socket, ServerMessage message, bool final = true);
    }
}
