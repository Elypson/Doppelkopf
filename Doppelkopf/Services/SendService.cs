using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using Doppelkopf.Interfaces;
using Doppelkopf.Models;

namespace Doppelkopf.Services
{
    public class SendService : ISendService
    {
        public void SendTo(IEnumerable<WebSocket> recipients, ServerMessage message)
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

        // ===

        public void SendToPlayer(List<IClientConnectionController> clientControllers, Player player, ServerMessage message)
        {
            var clientController = clientControllers.FirstOrDefault(client => client.Token == player.User.Token);
            if (clientController != null)
            {
                SendTo(new []{ clientController.Socket }, message);
            }
        }

        // ===

        public void SendToPlayers(List<IClientConnectionController> clientControllers, IEnumerable<Player> players, ServerMessage message)
        {
            var playerClientSockets = clientControllers.Where(client => players.Any(player => player.User.Token == client.Token)).Select(client => client.Socket);
            if (playerClientSockets.Count() > 0)
            {
                SendTo(playerClientSockets, message);
            }
        }
    }
}
