using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using DoppelkopfServer.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using DoppelkopfServer.Services;
using DoppelkopfServer.Interfaces;

namespace DoppelkopfServer.Controllers
{
    public class MainController : IMainController
    {
        private List<ClientConnectionController> clientControllers = new List<ClientConnectionController>();
        private ConcurrentQueue<ClientMessage> clientMessages = new ConcurrentQueue<ClientMessage>();
        private Thread serverThread = new Thread(MainController.RunServer);
        private List<User> users = new List<User>();
        private readonly ISendService sendService;
        private readonly IMetaMessageService metaMessageService;

        public MainController(ISendService _sendService, IMetaMessageService _metaMessageService)
        {
            sendService = _sendService;
            metaMessageService = _metaMessageService;
            serverThread.Start(this);
        }

        public async Task ManageWebSocketRequest(HttpContext context)
        {
            var newController = new ClientConnectionController();
            newController.MessageReceived += new EventHandler((Object source, EventArgs rawArgs) =>
            {
                var args = (ClientConnectionController.MessageReceivedArgs)rawArgs;
                clientMessages.Enqueue(args.Message);
            });            

            clientControllers.Add(newController);

            // socket needs to be initialized before we can continue
            newController.Initialize(context).Wait();

            // let client know their token
            sendService.SendSyncToClient(newController.Socket, new ServerMessage
            {
                Type = Message.MessageType.META,
                SubType = "token",
                Text = context.Connection.Id
            });

            await newController.Handle(context);

            clientControllers.Remove(newController);

            // find user that has left but keep him in case that reclaim is demanded
            var leftUser = users.Find(user => user.ConnectionID == newController.ConnectionID);

            sendService.SendTo(new ServerMessage { Type = Message.MessageType.META, SubType = "quit", Text = leftUser?.Name },
                from clientController in clientControllers select clientController.Socket);
        }

        public static void RunServer(object parameter)
        {
            var mainController = (MainController) parameter;

            while (true)
            {
                Thread.Sleep(1);

                ClientMessage message;

                while(mainController.clientMessages.TryDequeue(out message))
                {
                    switch(message.Type)
                    {
                        case Message.MessageType.META:
                            mainController.metaMessageService.HandleMessage(mainController.users, mainController.clientControllers, message);
                            break;

                        case Message.MessageType.CHAT:
                            mainController.chatMessageService.HandleMessage()
                            break;

                        case Message.MessageType.GAME:
                            mainController.HandleGameMessage(message);
                            break;
                    }
                }
                
            }
        }        

        private void HandleChatMessage(ClientMessage message)
        {
            // does user exist? only named users can send messages (if not named, client should use META "name")
            var existingUser = users.Where(user => user.ConnectionID == message.Token).FirstOrDefault();
            
            if(existingUser != null)
            {
                var otherClientsAtSameTableOrGlobal = clientControllers.Where(client =>
                    users.Exists(user => client.ConnectionID == user.ConnectionID &&
                    user.TableID == existingUser.TableID && user.ConnectionID != existingUser.ConnectionID));

                var serverMessage = new ServerMessage(message, existingUser.Name);

                sendService.SendTo(serverMessage, from otherClient in otherClientsAtSameTableOrGlobal select otherClient.Socket);
            }
        }
    }
}
