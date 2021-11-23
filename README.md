# azure-service-bus-chat-poc

This repository contains a simple example of a chat room made whit Blazor, SignalR and Azure Service Bus.

# Configuration
For use this sample, you need an Azure Service Bus instance whit Standard Plan, that support Topic, feature required.

In the *appsettings.json* you MUST set some variables:

    "AzureServiceBus": {
	    "TopicName": "<Insert topic name here>"
    },
    "ConnectionStrings": {
	    "ServiceBus": "<Insert connection string here>"
	}
  

 - TopicName: the anme of Topic created for this sample
 - ServiceBus: Connection string with Manage rights