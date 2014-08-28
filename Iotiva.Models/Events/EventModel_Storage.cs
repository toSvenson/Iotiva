using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Iotiva.Models
{
    public partial class EventModel
    {
        public static EventModel FromRowKey(string partitionKey, string rowKey)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var eventsTable = tableClient.GetTableReference("events");

            if (partitionKey == null) partitionKey = string.Empty;
            TableOperation retrieveOperation = TableOperation.Retrieve<EventModel>(partitionKey, rowKey);
            TableResult retrievedResult = eventsTable.Execute(retrieveOperation);
            return retrievedResult.Result as EventModel;
        }

        public void Save()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var eventsTable = tableClient.GetTableReference("events");
            eventsTable.CreateIfNotExists();
            eventsTable.Execute(TableOperation.InsertOrReplace(this));

        }

        public void Send()
        {
            var eventType = EventProperties["EventType"];
            if(string.IsNullOrWhiteSpace(eventType)) return; // Without an event type we don't notify


            // Check to see if we should send queue events for changes to things or not
            var notifyOnChange = CloudConfigurationManager.GetSetting("NotifyOnChange");
            if (string.IsNullOrWhiteSpace(notifyOnChange)) notifyOnChange = "false";

            if (notifyOnChange.ToLower() == "false")
            {
                switch (eventType)
                {
                    case "Message":
                        // Continue on sending message
                        break;

                    case "Change":
                    case "Add":
                    case "Delete":
                    default:
                        // Without "NotifyOnChanged" set we do not send to the queue on changes
                        return;
                }
            }

            // Find the queue type we're using and deliver the message
            var queueType = CloudConfigurationManager.GetSetting("QueueType");
            if (string.IsNullOrWhiteSpace(queueType)) queueType = "storage";
            switch (queueType.ToLower())
            {
                case "topic":
                    SendToTopic();
                    break;

                case "queue":
                    SendToQueue();
                    break;

                case "storage":
                default:
                    SendToStorage();
                    break;
            }
        }

        /// <summary>
        /// Delivers the message to a Azure Storage Queue. This is the default and simplest of the 
        /// queue methods supported. It is consumed via the /api/queue REST method. 
        /// </summary>
        private void SendToStorage()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            /* This handles one of the limitations of Azure Queues. Since 
             * our Things are likely larger than 64kb we cannot simply serialize 
             * them into the queue. To deal with this we instead store the event body
             * in a separate table called 'events'. We then only queue up the unique RowKey
             * for the event contents. */
            this.Save();

            /* Add a notification to the storage queue that an event has happened.
             * If this is a public queue we'll put the event in a generic 'events' queue
             * but if it has a unique PartitionKey we will queue it in a unique queue
             * for that user. This allows us to provide distinct queues for organization
             * and security benefits. <insert bad joke about Private Things> */
            var queueClient = storageAccount.CreateCloudQueueClient();
            string queueName = "events";
            if (!string.IsNullOrWhiteSpace(_thing.PartitionKey))
            {
                queueName = "events" + _thing.PartitionKey;
            }

            // Reference (and create if needed) our storage queue
            var eventQueue = queueClient.GetQueueReference(queueName);
            eventQueue.CreateIfNotExists();

            // Our message is simply the RowKey for the message in the Events table
            var message = new CloudQueueMessage(this.RowKey);

            // Right now we default to 5min time to live.
            var ttl = new TimeSpan(0, 5, 0);

            // Send the message to the queue
            eventQueue.AddMessage(message, ttl);
        }

        /// <summary>
        /// Delivers the message to an Azure Service Bus Topic. This is the most versatile 
        /// of all the methods and the best method for working with more than one listener (i.e. one message to many recipients)
        /// </summary>
        private void SendToTopic()
        {
            // Get the Service Bus
            string connectionString = CloudConfigurationManager.GetSetting("ServiceBusConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            // Make sure we have a Topic for StateChanges
            if (!namespaceManager.TopicExists("events")) namespaceManager.CreateTopic("events");
            var topicClient = TopicClient.CreateFromConnectionString(connectionString, "events");

            var msg = new BrokeredMessage();
            msg.Label = _thing.PartitionKey + "_" + _thing.RowKey;

            // Copy all of the properties to brokered message
            foreach (var item in this.EventProperties)
            {
                msg.Properties.Add(item.Key, item.Value);
            }

            // Send the message to the queue
            topicClient.Send(msg);
        }

        /// <summary>
        /// Delivers the message to an Azure Service Bus Queue. This is similar to the Storage Queue but results in 
        /// a lot less HTTP traffic.
        /// </summary>
        private void SendToQueue()
        {
            // Get the Service Bus
            string connectionString = CloudConfigurationManager.GetSetting("ServiceBusConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            // Make sure we have a Topic for StateChanges
            if (!namespaceManager.QueueExists("events")) namespaceManager.CreateQueue("events");
            var queueClient = QueueClient.CreateFromConnectionString(connectionString, "events");

            var msg = new BrokeredMessage();
            msg.Label = _thing.PartitionKey + "_" + _thing.RowKey;

            // Copy all of the properties to brokered message
            foreach (var item in this.EventProperties)
            {
                msg.Properties.Add(item.Key, item.Value);
            }

            // Send the message to the queue
            queueClient.Send(msg);
        }


    }
}