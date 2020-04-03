using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Doppelkopf.Controllers;
using Doppelkopf.Interfaces;
using Doppelkopf.Models;

namespace Doppelkopf.Services
{
    public class MetaMessageService : IMetaMessageService
    {
        private readonly ISendService sendService;

        public MetaMessageService(ISendService _sendService)
        {
            sendService = _sendService;
        }

        public void HandleMessage(List<User> users, List<ClientConnectionController> clientConnectionControllers, ClientMessage message)
        {
            switch (message.SubType)
            {
                case "name":
                    HandleNameSubType(users, clientConnectionControllers, message);
                    break;

                case "userlist":
                    HandleUserlistSubType(users, clientConnectionControllers, message);
                    break;
            }
        }

        public void HandleNameSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers,
            ClientMessage message)
        {
            // does user exist?
            var existingUser = users.FirstOrDefault(user => user.ConnectionID == message.Token);

            ServerMessage responseToAll;
            if (existingUser == null)
            {
                User user = new User(message.Token, message.Text);
                users.Add(user);

                responseToAll = new ServerMessage
                {
                    Type = Message.MessageType.META,
                    SubType = "join",
                    Text = user.Name
                };
            }
            else
            {
                string oldUserName = existingUser.Name;
                existingUser.Name = message.Text;
                responseToAll = new ServerMessage
                {
                    Type = Message.MessageType.META,
                    SubType = "rename",
                    Text = oldUserName,
                    Username = existingUser.Name
                };
            }

            sendService.SendTo(from clientController in clientConnectionControllers select clientController.Socket, responseToAll);
        }

        public void HandleUserlistSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers,
            ClientMessage message)
        {
            string userList = JsonSerializer.Serialize(from user in users where user.Online select new {Username = user.Name, TableID = user.TableID});

            var response = new ServerMessage
            {
                Type = Message.MessageType.META,
                SubType = "userlist",
                Text = userList
            };

            sendService.SendSyncToClient((from controller in clientConnectionControllers where controller.ConnectionID == message.Token
                   select controller.Socket).First(), response);
        }
    }
}
