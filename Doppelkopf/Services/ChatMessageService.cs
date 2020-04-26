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

        public void HandleMessage(List<User> users, List<IClientConnectionController> clientConnectionControllers, IUserPermissionService userPermissionService, ClientMessage message)
        {
            if (userPermissionService.IsMessageFromNamedUser(users, message))
            {
                var existingUser = users.FirstOrDefault(user => user.Token == message.Token);

                var otherClientsAtSameTableOrGlobal = clientConnectionControllers.Where(client =>
                    users.Exists(user => client.Token == user.Token &&
                    user.TableID == existingUser.TableID && user.Token != existingUser.Token));

                var serverMessage = new ServerMessage(message, existingUser.Name);

                sendService.SendTo(from otherClient in otherClientsAtSameTableOrGlobal select otherClient.Socket, serverMessage);
            }
        }
    }
}
