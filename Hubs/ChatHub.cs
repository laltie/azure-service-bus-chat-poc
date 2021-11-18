using AzureServiceBusChatPOC.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureServiceBusChatPOC.Hubs
{
    public class ChatHub : Hub
    {
        public const string HubUrl = "/chat";
        private readonly TopicService _topicService;

        public ChatHub(TopicService topicService)
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


        public async Task Start()
        { 
        }

        public async Task Send(string username, string message)
        {
            await _topicService.SendMessage(username, message);
        }
    }
}
