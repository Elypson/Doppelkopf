using System;
using System.Collections.Generic;
using System.Linq;
using Doppelkopf.Controllers;
using Doppelkopf.Interfaces;
using Doppelkopf.Models;

namespace Doppelkopf.Services
{
    public class ChatMessageService : IChatMessageService
    {
        private readonly ISendService sendService;

        public ChatMessageService(ISendService _sendService)
        {
            sendService = _sendService;
        }

        public void HandleMessage(List<User> users, List<ClientConnectionController> clientConnectionControllers, ClientMessage message)
        {
            // does user exist? only named users can send messages (if not named, client should use META "name")
            var existingUser = users.Where(user => user.ConnectionID == message.Token).FirstOrDefault();

            if (existingUser != null)
            {
                var otherClientsAtSameTableOrGlobal = clientConnectionControllers.Where(client =>
                    users.Exists(user => client.ConnectionID == user.ConnectionID &&
                    user.TableID == existingUser.TableID && user.ConnectionID != existingUser.ConnectionID));

                var serverMessage = new ServerMessage(message, existingUser.Name);

                sendService.SendTo(from otherClient in otherClientsAtSameTableOrGlobal select otherClient.Socket, serverMessage);
            }
        }
    }
}
