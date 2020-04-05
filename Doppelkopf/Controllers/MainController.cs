using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using Doppelkopf.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using Doppelkopf.Services;
using Doppelkopf.Interfaces;

namespace Doppelkopf.Controllers
{
    public class MainController : IMainController
    {
        private readonly List<ClientConnectionController> clientControllers = new List<ClientConnectionController>();
        private readonly ConcurrentQueue<ClientMessage> clientMessages = new ConcurrentQueue<ClientMessage>();
        private readonly Thread serverThread = new Thread(MainController.RunServer);
        private readonly List<User> users = new List<User>();
        private readonly ISendService sendService;
        private readonly IChatMessageService chatMessageService;
        private readonly IMetaMessageService metaMessageService;

        // TableID maps to game controller
        List<IGameController> gameControllers = new List<IGameController>();    

        public MainController(ISendService _sendService, IMetaMessageService _metaMessageService, IChatMessageService _chatMessageService)
        {
            sendService = _sendService;
            chatMessageService = _chatMessageService;
            metaMessageService = _metaMessageService;
            serverThread.Start(this);
        }

        public async Task ManageWebSocketRequestAsync(HttpContext context)
        {
            var newController = new ClientConnectionController();
            newController.MessageReceived += (Object source, EventArgs rawArgs) =>
            {
                var args = (IClientConnectionController.MessageReceivedArgs)rawArgs;
                clientMessages.Enqueue(args.Message);
            };          

            clientControllers.Add(newController);

            // socket needs to be initialized before we can continue
            newController.InitializeAsync(context).Wait();

            // let client know their token
            sendService.SendSyncToClient(newController.Socket, new ServerMessage
            {
                Type = Message.MessageType.Meta,
                SubType = "token",
                Text = context.Connection.Id
            });

            await newController.HandleAsync(context);
            
            clientControllers.Remove(newController);

            // find user that has left but keep him in case that reclaim is demanded
            var leftUser = users.Find(user => user.ConnectionID == newController.ConnectionID);

            leftUser.Online = false;

            sendService.SendTo(from clientController in clientControllers select clientController.Socket, new ServerMessage { Type = Message.MessageType.Meta, SubType = "quit", Text = leftUser.Name });
        }

        void handleGameMessage(ClientMessage message)
        {
            var user = users.FirstOrDefault(user => user.ConnectionID == message.Token);

            if (user == null || user.TableID == User.NO_TABLE)
                return;

            IGameController gameController = gameControllers.FirstOrDefault(controller => controller.TableID == user.TableID);

            // game needs to exist because someone needs to start it
            if (gameControllers != null)
            {
                gameController.HandleMessage(message);
            }
            else
            {
                // drop?
            }
        }

        public static void RunServer(object parameter)
        {
            var mainController = (MainController) parameter;

            while (true)
            {
                Thread.Sleep(1);

                while(mainController.clientMessages.TryDequeue(out var message))
                {
                    switch(message.Type)
                    {
                        case Message.MessageType.Meta:
                            mainController.metaMessageService.HandleMessage(mainController.users, mainController.clientControllers, mainController.gameControllers, message);
                            break;
                        case Message.MessageType.Chat:
                            mainController.chatMessageService.HandleMessage(mainController.users, mainController.clientControllers, message);
                            break;
                        case Message.MessageType.Game:
                            mainController.handleGameMessage(message);
                            break;
                    }
                }
            }
        }
    }
}
