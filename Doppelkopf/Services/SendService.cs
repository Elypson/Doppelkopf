using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using DoppelkopfServer.Interfaces;
using DoppelkopfServer.Models;

namespace DoppelkopfServer.Services
{
    public class SendService : ISendService
    {
        public void SendTo(ServerMessage message, IEnumerable<WebSocket> recipients)
        {
            foreach (var recipient in recipients)
            {
                SendSyncToClient(recipient, message);
            }
        }

        // ===

        public void SendSyncToClient(WebSocket socket, ServerMessage message, bool final = true)
        { 
            if (socket != null && (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived))
            {
                lock (socket)
                {
                    socket?.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)),
                        WebSocketMessageType.Text, final, CancellationToken.None).Wait();
                }
            }
        }
    }

    
}
