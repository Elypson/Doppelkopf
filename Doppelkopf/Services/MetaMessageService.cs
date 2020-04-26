using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Doppelkopf.Controllers;
using Doppelkopf.GameObjects;
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

        public void HandleMessage(List<User> users, List<IClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, IUserPermissionService userPermissionService, ClientMessage message)
        {
            switch (message.SubType)
            {
                case "getToken":
                    handleGetTokenSubType(users, clientConnectionControllers, message);
                    break;

                case "reclaim":
                    handleReclaimSubType(users, clientConnectionControllers, message);
                    break;

                case "name":
                    handleNameSubType(users, clientConnectionControllers, userPermissionService, message);
                    break;

                case "listUsers":
                    handleListUsersSubType(users, clientConnectionControllers, userPermissionService, message);
                    break;

                case "listTables":
                    handleListTablesSubType(users, clientConnectionControllers, gameControllers, userPermissionService, message);
                    break;

                case "createTable":
                    handleCreateTableSubType(users, clientConnectionControllers, gameControllers, userPermissionService, message);
                    break;

                case "joinTable":
                    handleJoinTableSubType(users, clientConnectionControllers, gameControllers, userPermissionService, message);
                    break;

                case "leaveTable":
                    handleLeaveTableSubType(users, clientConnectionControllers, gameControllers, userPermissionService, message);
                    break;
            }
        }

        private void handleGetTokenSubType(List<User> users, List<IClientConnectionController> clientConnectionControllers, ClientMessage message)
        {
            // set token to GUID and let user know
            var connection = clientConnectionControllers.FirstOrDefault(controller => controller.Token == message.Token);

            if (connection != null)
            {
                string newToken = connection.CreateToken();

                sendService.SendSyncToClient(connection.Socket, new ServerMessage
                {
                    Type = Message.MessageType.Meta,
                    SubType = "token",
                    Text = newToken
                });
            }
        }

        private void handleReclaimSubType(List<User> users, List<IClientConnectionController> clientConnectionControllers, ClientMessage message)
        {
            // if some user with such GUID is found, token is valid and we let client know;
            // otherwise, new GUID is assigned
            var connection = clientConnectionControllers.FirstOrDefault(controller => controller.Token == message.Token);
            
            if (connection != null)
            {
                var user = users.FirstOrDefault(user => user.Token == message.Text);

                if (user == null)
                {
                    handleGetTokenSubType(users, clientConnectionControllers, message);
                }
                else
                {
                    connection.ResetToken(message.Text);

                    user.Online = true;

                    sendService.SendSyncToClient(connection.Socket, new ServerMessage
                    {
                        Type = Message.MessageType.Meta,
                        SubType = "accept"
                    });
                }
            }
        }

        private void handleJoinTableSubType(List<User> users, List<IClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, IUserPermissionService userPermissionService, ClientMessage message)
        {
            if(!userPermissionService.IsMessageFromNamedUser(users, message))
            {
                // drop?
                return;
            }

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
                    var sender = users.FirstOrDefault(user => user.Token == message.Token);

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

        private void handleLeaveTableSubType(List<User> users, List<IClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, IUserPermissionService userPermissionService, ClientMessage message)
        {
            if (!userPermissionService.IsMessageFromNamedUser(users, message))
            {
                // drop?
                return;
            }

            var sender = users.FirstOrDefault(user => user.Token == message.Token);

            if (sender != null)
            {
                sender.TableID = User.NO_TABLE;
            }
        }

        private void handleCreateTableSubType(List<User> users, List<IClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, IUserPermissionService userPermissionService, ClientMessage message)
        {
            if(!userPermissionService.IsMessageFromNamedUser(users, message))
            {
                // drop?
                return;
            }

            var numbers = Enumerable.Range(1, 10000); // limit of 10000 tables but should never be an issue
            var newTableID = numbers.FirstOrDefault(number => !gameControllers.Exists(gameController => gameController.TableID == number));

            if (newTableID != 0)
            {
                try
                {
                    var parts = message.Text.Split(",");
                    string name = "", password = null;
                    bool hidden = false;
                    RuleSet ruleSet = new RuleSet();

                    if (parts.Count() > 0)
                    {
                        name = parts[0];
                        if(parts.Count() > 1)
                        {
                            password = parts[1];
                            if(parts.Count() > 2)
                            {
                                hidden = parts[2] == "true";

                                if(parts.Count() > 3)
                                {
                                    // split all parts to retrieve rules
                                    for(int partID = 3; partID < parts.Count(); ++partID)
                                    {
                                        var keyValue = parts[partID].Split(":");
                                        if(keyValue.Count() != 2)
                                        {
                                            continue;
                                        }

                                        switch(keyValue[0])
                                        {
                                            case "useNines": ruleSet.UseNines = keyValue[1] == "true"; break;
                                            case "withArmut": ruleSet.WithArmut = keyValue[1] == "true"; break;
                                            case "withFleischlos": ruleSet.WithFleischlos = keyValue[1] == "true"; break;
                                            case "countingReContra": ruleSet.CountReContraBy = keyValue[1] == "+2" ? RuleSet.ReContraCounting.ADDING_TWO : RuleSet.ReContraCounting.DOUBLING; break;
                                            case "secondDulleTrumpsFirst": ruleSet.SecondDulleTrumpsFirst = keyValue[1] == "true"; break;
                                            case "bothPigletsTrumpAll": ruleSet.BothPigletsTrumpAll = keyValue[1] == "true"; break;
                                            case "reContraAtHochzeitAfterFinderTrick": ruleSet.ReContraAtHochzeitAfterFinderTrick = keyValue[1] == "true"; break;
                                            case "withReshufflingAtFiveKings": ruleSet.WithReshufflingAtFiveKings = keyValue[1] == "true"; break;
                                            case "withReshufflingAtEightyPoints": ruleSet.WithReshufflingAtEightyPoints = keyValue[1] == "true"; break;
                                            case "soloPlayerFirstToAct": ruleSet.SoloPlayerFirstToAct = keyValue[1] == "true"; break;
                                            case "numberOfNestedBuckRounds":
                                                if (int.TryParse(keyValue[1], out int numberOfNestedBuckRounds))
                                                {
                                                    ruleSet.NumberOfNestedBuckRounds = numberOfNestedBuckRounds;
                                                }
                                                break;
                                            case "addBuckRoundAtLostSolo": ruleSet.AddBuckRoundAtLostSolo = keyValue[1] == "true"; break;
                                            case "addBuckRoundAtFullHeartTrick": ruleSet.AddBuckRoundAtFullHeartTrick = keyValue[1] == "true"; break;
                                            case "addBuckRoundAtLostContra": ruleSet.AddBuckRoundAtLostContra = keyValue[1] == "true"; break;
                                            case "addBuckRoundAtZeroGame": ruleSet.AddBuckRoundAtZeroGame = keyValue[1] == "true"; break;

                                        }
                                    }
                                    
                                }
                            }
                        }
                    }

                    var founder = users.FirstOrDefault(user => user.Token == message.Token);

                    gameControllers.Add(new GameController(sendService, users, clientConnectionControllers, newTableID, name, password, hidden, founder, ruleSet));

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

        private void handleNameSubType(List<User> users, List<IClientConnectionController> clientConnectionControllers, IUserPermissionService userPermissionService, ClientMessage message)
        {
            if(!userPermissionService.IsMessageFromTokenizedUser(clientConnectionControllers, message))
            {
                // drop?
                return;
            }

            var existingUser = users.FirstOrDefault(user => user.Token == message.Token);

            ServerMessage responseToAll;
            if (existingUser == null)
            {
                // client connection controllers needs to be initialized by having a new or reclaimed token
                var initialized = clientConnectionControllers.FirstOrDefault(controller => controller.Token == message.Token)?.Initialized;
                
                if(initialized == null || initialized == false)
                {
                    // drop?
                    return;
                }

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

        private void handleListUsersSubType(List<User> users, List<IClientConnectionController> clientConnectionControllers, IUserPermissionService userPermissionService, ClientMessage message)
        {
            if(!userPermissionService.IsMessageFromNamedUser(users, message))
            {
                // drop?
                return;
            }

            string userList = JsonSerializer.Serialize(from user in users where user.Online select new {Username = user.Name, TableID = user.TableID});

            var response = new ServerMessage
            {
                Type = Message.MessageType.Meta,
                SubType = "listUsers",
                Text = userList
            };

            sendService.SendSyncToClient((from controller in clientConnectionControllers where controller.Token == message.Token
                   select controller.Socket).First(), response);
        }

        private class ExistingTableProperties
        {
            public int ID { set; get; }
            public string Name { set; get;}
            public bool HasPassword { set; get; }
            public bool IsHidden { set; get; }
        }

        private void handleListTablesSubType(List<User> users, List<IClientConnectionController> clientConnectionControllers, List<IGameController> gameControllers, IUserPermissionService userPermissionService, ClientMessage message)
        {
            if (!userPermissionService.IsMessageFromNamedUser(users, message))
            {
                // drop?
                return;
            }

            var sender = users.FirstOrDefault(user => user.Token == message.Token);

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

                var socket = (from client in clientConnectionControllers where client.Token == sender.Token select client.Socket).FirstOrDefault();

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
