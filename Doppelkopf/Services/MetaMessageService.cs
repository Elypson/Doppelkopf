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

        public void HandleMessage(List<User> users, List<ClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, ClientMessage message)
        {
            switch (message.SubType)
            {
                case "name":
                    handleNameSubType(users, clientConnectionControllers, message);
                    break;

                case "listUsers":
                    handleListUsersSubType(users, clientConnectionControllers, message);
                    break;

                case "listTables":
                    handleListTablesSubType(users, clientConnectionControllers, gameControllers, message);
                    break;

                case "createTable":
                    handleCreateTableSubType(users, clientConnectionControllers, gameControllers, message);
                    break;

                case "joinTable":
                    handleJoinTableSubType(users, clientConnectionControllers, gameControllers, message);
                    break;

                case "leaveTable":
                    handleLeaveTableSubType(users, clientConnectionControllers, gameControllers, message);
                    break;
            }
        }
        
        private void handleJoinTableSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, ClientMessage message)
        {
            try
            {
                var joinTableProperties = message.Text.Split(",");
                int tableID = -1;
                string password = null;
                if(joinTableProperties.Count() > 0)
                {
                    int.TryParse(joinTableProperties[0], out tableID);

                    if(joinTableProperties.Count() > 1)
                    {
                        password = joinTableProperties[1];
                    }
                }

                var gameController = gameControllers.FirstOrDefault(game => game.TableID == tableID && (String.IsNullOrEmpty(game.Password) || game.Password == password));

                if(gameController != null)
                {
                    var sender = users.FirstOrDefault(user => user.ConnectionID == message.Token);

                    if (sender != null)
                    {
                        sender.TableID = gameController.TableID;
                    }
                }
            }
            catch(JsonException)
            {
                // drop?
            }
        }

        private void handleLeaveTableSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, ClientMessage message)
        {
            var sender = users.FirstOrDefault(user => user.ConnectionID == message.Token);

            if (sender != null)
            {
                sender.TableID = User.NO_TABLE;
            }
        }

        private void handleCreateTableSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, ClientMessage message)
        {
            var numbers = Enumerable.Range(1, 10000); // limit of 10000 tables but should never be an issue
            var newTableID = numbers.FirstOrDefault(number => !gameControllers.Exists(gameController => gameController.TableID == number));

            if (newTableID != 0)
            {
                try
                {
                    var parts = message.Text.Split(",");
                    string name = "", password = null;
                    bool hidden = false;
                    if(parts.Count() > 0)
                    {
                        name = parts[0];
                        if(parts.Count() > 1)
                        {
                            password = parts[1];
                            if(parts.Count() > 2)
                            {
                                hidden = parts[2] == "true";
                            }
                        }
                    }

                    var founder = users.FirstOrDefault(user => user.ConnectionID == message.Token);

                    gameControllers.Add(new GameController(sendService, users, clientConnectionControllers, newTableID, name, password, hidden, founder));

                    founder.TableID = newTableID;

                    sendService.SendTo(clientConnectionControllers.Select(c => c.Socket), new ServerMessage
                    {
                        Type = Message.MessageType.Meta,
                        SubType = "tablesUpdated"
                    });
                }
                catch (JsonException)
                {
                    // drop?
                }
            }
        }

        private void handleNameSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers,
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
                    Type = Message.MessageType.Meta,
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
                    Type = Message.MessageType.Meta,
                    SubType = "rename",
                    Text = oldUserName,
                    Username = existingUser.Name
                };
            }

            sendService.SendTo(from clientController in clientConnectionControllers select clientController.Socket, responseToAll);
        }

        private void handleListUsersSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers, ClientMessage message)
        {
            string userList = JsonSerializer.Serialize(from user in users where user.Online select new {Username = user.Name, TableID = user.TableID});

            var response = new ServerMessage
            {
                Type = Message.MessageType.Meta,
                SubType = "listUsers",
                Text = userList
            };

            sendService.SendSyncToClient((from controller in clientConnectionControllers where controller.ConnectionID == message.Token
                   select controller.Socket).First(), response);
        }

        private class ExistingTableProperties
        {
            public int ID { set; get; }
            public string Name { set; get;}
            public bool HasPassword { set; get; }
            public bool IsHidden { set; get; }
        }

        private void handleListTablesSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, ClientMessage message)
        {
            var sender = users.FirstOrDefault(user => user.ConnectionID == message.Token);

            if (sender != null)
            {
                // get game controllers that are not hidden or where user is owner
                var visibleGameControllers = gameControllers.Where(game => !game.Hidden || game.Administrators.Contains(sender));

                var tableList = new List<ExistingTableProperties>();

                foreach (var gameController in visibleGameControllers)
                {
                    tableList.Add(new ExistingTableProperties
                    {
                        ID = gameController.TableID,
                        Name = gameController.Name,
                        HasPassword = !String.IsNullOrEmpty(gameController.Password),
                        IsHidden = gameController.Hidden
                    });
                }

                var tableListString = JsonSerializer.Serialize(tableList);

                var socket = (from client in clientConnectionControllers where client.ConnectionID == sender.ConnectionID select client.Socket).FirstOrDefault();

                if (socket != null)
                {
                    sendService.SendSyncToClient(socket, new ServerMessage
                    {
                        Type = Message.MessageType.Meta,
                        SubType = "listTables",
                        Text = tableListString
                    });
                }
            }
        }
    }
}
