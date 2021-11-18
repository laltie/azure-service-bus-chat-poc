using Azure.Messaging.ServiceBus;
using AzureServiceBusChatPOC.Interface;
using AzureServiceBusChatPOC.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBusChatPOC.Hubs
{
    public class ChatHub : Hub<IChatHub>
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

        public async Task Send(string username, string message)
        {
            await _topicService.SendMessage(username, message);
            await _topicService.FakeReceive(username, message);
        }

        public async Task LeaveChatRoom(string username)
        {
            await Send(username, $"[Notice] {username} leaved chat room.");
            var proc = _topicService.GetProcessor(username);
            await proc.StopProcessingAsync();
            await _topicService.RemoveSubscription(username);
        }
    }
}
