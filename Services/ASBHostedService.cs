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
    public class ASBHostedService : IHostedService
    {
        private readonly IConfiguration _config;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ServiceBusClient _sbClient;
        private readonly TopicService _topicService;

        public ASBHostedService(IConfiguration config, IHubContext<ChatHub> ctx, ServiceBusClient sbClient)
        {
            _hubContext = ctx;
            _sbClient = sbClient;
            _config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //RegisterOnMessageHandlerAndReceiveMessages();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //var proc = _sbClient.CreateProcessor(_config.GetSection("AzureServiceBus")["TopicName"], "laltie", new ServiceBusProcessorOptions());
            //proc.StopProcessingAsync().ConfigureAwait(false);
            return Task.CompletedTask;
        }

        public void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var proc = _sbClient.CreateProcessor(_config.GetSection("AzureServiceBus")["TopicName"], "laltie", new ServiceBusProcessorOptions());
            proc.ProcessMessageAsync += MessageHandler;
            proc.ProcessErrorAsync += ErrorHandler;
            proc.StartProcessingAsync().ConfigureAwait(false);
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"{args.Message.Subject}  >  {Encoding.UTF8.GetString(args.Message.Body)}");
            // complete the message. messages is deleted from the subscription. 
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", args.Message.Subject, args.Message.Body);
            await args.CompleteMessageAsync(args.Message);
        }
    }
}
