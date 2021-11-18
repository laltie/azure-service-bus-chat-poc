using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using AzureServiceBusChatPOC.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBusChatPOC.Services
{
    public class TopicService
    {
        private readonly IConfiguration _config;
        private readonly ServiceBusClient _sbClient;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly ServiceBusSender _topicClient;
        private readonly string _topicName;
        private readonly IHubContext<ChatHub> _hubContext;

        public TopicService(IConfiguration config, ServiceBusClient sbClient,
            IHubContext<ChatHub> ctx)
        {
            _hubContext = ctx;
            _sbClient = sbClient;
            _config = config;
            _adminClient = new ServiceBusAdministrationClient(_config.GetConnectionString("ServiceBus"));
            _topicName = _config.GetSection("AzureServiceBus")["TopicName"];
            if (!_adminClient.TopicExistsAsync(_topicName).Result)
            {
                _adminClient.CreateTopicAsync(_topicName);
            }
            _topicClient = _sbClient.CreateSender(_topicName);
        }

        public ServiceBusProcessor GetProcessor(string subname)
        {
            return _sbClient.CreateProcessor(_topicName, subname, new ServiceBusProcessorOptions());
        }

        public async Task CreateSubscription(string subName)
        {
            if (!await _adminClient.SubscriptionExistsAsync(_topicName, subName))
            {
                CreateSubscriptionOptions description = new CreateSubscriptionOptions(_topicName, subName)
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
                };

                await _adminClient.CreateSubscriptionAsync(description);
            }
        }

        public async Task RemoveSubscription(string subName)
        {
            if (await _adminClient.SubscriptionExistsAsync(_topicName, subName))
            {
                await _adminClient.DeleteSubscriptionAsync(_topicName, subName);
            }
        }

        public async Task SendMessage(string username, string messageText)
        {
            await CreateSubscription(username);

            // Create a new message to send to the topic                
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageText));
            message.Subject = username;

            // Send the message to the topic
            await this._topicClient.SendMessageAsync(message);
        }

        public async Task RegisterMessageProcessing(string username)
        {
            var proc = GetProcessor(username);
            proc.ProcessMessageAsync += MessageHandler;
            proc.ProcessErrorAsync += ErrorHandler;
            await proc.StartProcessingAsync().ConfigureAwait(false);
        }

        public async Task MessageHandler(ProcessMessageEventArgs args)
        {
            //var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            string body = args.Message.Body.ToString();
            Console.WriteLine($"{args.Message.Subject}  >  {Encoding.UTF8.GetString(args.Message.Body)}");
            // complete the message. messages is deleted from the subscription. 
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", args.Message.Subject, args.Message.Body);
            await args.CompleteMessageAsync(args.Message);
        }

        public async Task FakeReceive(string user, string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
