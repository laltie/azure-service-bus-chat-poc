using Azure.Messaging.ServiceBus;
using AzureServiceBusChatPOC.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureServiceBusChatPOC.Services
{
    public class Worker : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ServiceBusClient _sbClient;
        private readonly TopicService _topicService;

        public Worker(IConfiguration config, IHubContext<ChatHub> ctx, ServiceBusClient sbClient)
        {
            _hubContext = ctx;
            _sbClient = sbClient;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Worker", DateTime.Now.ToLongTimeString());
                await Task.Delay(5000);
            }
            await Task.CompletedTask;
        }

    }
}
