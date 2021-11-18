using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
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

        public TopicService(IConfiguration config, ServiceBusClient sbClient)
        {
            this._sbClient = sbClient;
            this._config = config;
            this._adminClient = new ServiceBusAdministrationClient(_config.GetConnectionString("ServiceBus"));
            _topicName = _config.GetSection("AzureServiceBus")["TopicName"];
            if (!_adminClient.TopicExistsAsync(_topicName).Result)
            {
                _adminClient.CreateTopicAsync(_topicName);
            }
            _topicClient = _sbClient.CreateSender(_topicName);
        }

        public async Task SendMessage(string username, string messageText)
        {
            if (!await _adminClient.SubscriptionExistsAsync(_topicName, username))
            {
                CreateSubscriptionOptions description = new CreateSubscriptionOptions(_topicName, username)
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
                };

                await _adminClient.CreateSubscriptionAsync(description);
            }
            else
            {
                // subProperties = await _adminClient.GetSubscriptionAsync(_topicName, username);
            }

            // Create a new message to send to the topic                
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageText));
            message.Subject = username;

            // Send the message to the topic
            await this._topicClient.SendMessageAsync(message);
        }
    }
}
