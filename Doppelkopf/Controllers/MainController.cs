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
        private readonly Dictionary<Message.MessageType, IMessageService> messageServices = new Dictionary<Message.MessageType, IMessageService>();

        public MainController(ISendService _sendService, IMetaMessageService metaMessageService, IChatMessageService chatMessageService,
            IGameMessageService gameMessageService)
        {
            sendService = _sendService;
            messageServices.Add(Message.MessageType.META, metaMessageService);
            messageServices.Add(Message.MessageType.CHAT, chatMessageService);
            messageServices.Add(Message.MessageType.GAME, gameMessageService);
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
                Type = Message.MessageType.META,
                SubType = "token",
                Text = context.Connection.Id
            });

            await newController.HandleAsync(context);
            
            clientControllers.Remove(newController);

            // find user that has left but keep him in case that reclaim is demanded
            var leftUser = users.Find(user => user.ConnectionID == newController.ConnectionID);

            leftUser.Online = false;

            sendService.SendTo(from clientController in clientControllers select clientController.Socket, new ServerMessage { Type = Message.MessageType.META, SubType = "quit", Text = leftUser.Name });
        }

        public static void RunServer(object parameter)
        {
            var mainController = (MainController) parameter;

            while (true)
            {
                Thread.Sleep(1);

                while(mainController.clientMessages.TryDequeue(out var message))
                {
                    mainController.messageServices[message.Type].HandleMessage(
                        mainController.users, mainController.clientControllers, message);
                }

            }
        }
    }
}
