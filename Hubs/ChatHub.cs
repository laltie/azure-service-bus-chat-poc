using Azure.Messaging.ServiceBus;
using AzureServiceBusChatPOC.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBusChatPOC.Hubs
{
    public class ChatHub : Hub
    {
        public const string HubUrl = "/chat";


        private readonly TopicService _topicService;

        public ChatHub(TopicService topicService, IHubContext<ChatHub> ctx)
        {
            _topicService = topicService;
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }

        public async Task JoinChatRoom(string username)
        {
            await Send(username, $"[Notice] {username} joined chat room.");
            await _topicService.RegisterMessageProcessing(username);
        }

        public async Task MessageHandler(ProcessMessageEventArgs args)
        {
            //var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            //string body = args.Message.Body.ToString();
            //Console.WriteLine($"{args.Message.Subject}  >  {Encoding.UTF8.GetString(args.Message.Body)}");
            // complete the message. messages is deleted from the subscription. 
            await Clients.All.SendAsync("ReceiveMessage", args.Message.Subject, args.Message.Body);
            await args.CompleteMessageAsync(args.Message);
        }

        public Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        public async Task Send(string username, string message)
        {
            await _topicService.SendMessage(username, message);
            await _topicService.FakeReceive(username, message);
        }

        public async Task LeaveChatRoom(string username)
        {
            await Send(username, $"[Notice] {username} leaved chat room.");
            var proc = _topicService.GetProcessor(username);
            proc.ProcessMessageAsync -= MessageHandler;
            proc.ProcessErrorAsync -= ErrorHandler;
            await proc.StopProcessingAsync();
            await _topicService.RemoveSubscription(username);
        }
    }
}
