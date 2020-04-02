using System;
using System.Collections.Generic;
using System.Linq;
using DoppelkopfServer.Controllers;
using DoppelkopfServer.Interfaces;
using DoppelkopfServer.Models;

namespace DoppelkopfServer.Services
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
                    // does user exist?
                    var existingUser = users.Where(user => user.ConnectionID == message.Token).FirstOrDefault();

                    ServerMessage responseToAll;
                    if (existingUser != null)
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
                    else
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

                    sendService.SendTo(responseToAll, from clientController in clientConnectionControllers select clientController.Socket);

                    break;
            }
        }
    }
}
