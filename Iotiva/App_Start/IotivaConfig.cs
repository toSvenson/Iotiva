using Microsoft.ServiceBus;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iotiva
{
    public static class IotivaConfig
    {

        public static void Configure()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));           

            // Table Storage
            var tableClient = storageAccount.CreateCloudTableClient();

            // Thing Table
            var thingTable = tableClient.GetTableReference("things");
            thingTable.CreateIfNotExists();

            // Events Table
            var eventsTable = tableClient.GetTableReference("events");
            eventsTable.CreateIfNotExists();

            // Queue Storage
            var queueClient = storageAccount.CreateCloudQueueClient();
            
            // General Event Queue
            var eventQueue = queueClient.GetQueueReference("events");
            eventQueue.CreateIfNotExists();
        }

        public static string TopicName { get; set; }
    }
}