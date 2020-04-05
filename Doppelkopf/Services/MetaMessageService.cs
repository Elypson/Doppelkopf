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

        private struct NewTableProperties
        {
            public string name;
            public string password;
            public bool hidden;
        }

        private struct JoinTableProperties
        {
            public int tableID;
            public string password;
        }

        private void handleJoinTableSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, ClientMessage message)
        {
            try
            {
                var joinTableProperties = JsonSerializer.Deserialize<JoinTableProperties>(message.Text);

                var gameController = gameControllers.FirstOrDefault(game => game.TableID == joinTableProperties.tableID && (String.IsNullOrEmpty(game.Password) || game.Password == joinTableProperties.password));

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

        private void handleLeaveTableSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers, List<GameController> gameControllers, ClientMessage message)
        {
            var sender = users.FirstOrDefault(user => user.ConnectionID == message.Token);

            if (sender != null)
            {
                sender.TableID = User.NO_TABLE;
            }
        }

        private void handleCreateTableSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers, List<GameController> gameControllers, ClientMessage message)
        {
            var numbers = Enumerable.Range(1, 10000); // limit of 10000 tables but should never be an issue
            var newTableID = numbers.FirstOrDefault(number => !gameControllers.Exists(gameController => gameController.TableID == number));

            if (newTableID != 0)
            {
                try
                {
                    NewTableProperties newTableProperties = JsonSerializer.Deserialize<NewTableProperties>(message.Text);

                    gameControllers.Add(new GameController(sendService, users, clientConnectionControllers, newTableID, newTableProperties.name, newTableProperties.password, newTableProperties.hidden, users.FirstOrDefault(user => user.ConnectionID == message.Token)));

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
                SubType = "userlist",
                Text = userList
            };

            sendService.SendSyncToClient((from controller in clientConnectionControllers where controller.ConnectionID == message.Token
                   select controller.Socket).First(), response);
        }

        private struct ExistingTableProperties
        {
            public string name;
            public bool hasPassword;
            public bool isHidden; // if user is owner
        }

        private void handleListTablesSubType(List<User> users, List<ClientConnectionController> clientConnectionControllers, List<GameController> gameControllers, ClientMessage message)
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
                        name = gameController.Name,
                        hasPassword = String.IsNullOrEmpty(gameController.Password),
                        isHidden = gameController.Hidden
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
