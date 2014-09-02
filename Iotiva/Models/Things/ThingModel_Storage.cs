using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Collections.Specialized;
using Iotiva.Models.Events;

namespace Iotiva.Models.Things
{
    public partial class ThingModel
    {
        private static CloudTable _thingTable;

        private static CloudTable ThingTable
        {
            get
            {
                if (_thingTable == null)
                {
                    var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                    var tableClient = storageAccount.CreateCloudTableClient();
                    _thingTable = tableClient.GetTableReference("things");
                    _thingTable.CreateIfNotExists();
                }
                return _thingTable;
            }
        }

        public static ThingModel FromNameValueCollection(string partitionKey, NameValueCollection pairs, bool autoSave)
        {
            ThingModel thing;

            if (string.IsNullOrWhiteSpace(pairs["id"]))
            {
                // We did not receive an id in the collection, create a new ThingModel
                thing = new ThingModel(partitionKey);
            }
            else
            {
                // We got an id but we need to make sure it exists. We will try
                // and retrieve it and if it fails we'll create a new thing with the passed id
                thing = FromRowKey(partitionKey, pairs["id"]);
                if (thing == null) thing = new ThingModel(partitionKey, pairs["id"]);
            }

            // Cycle through each of the value pairs we were passed and
            // pass them into SetProperProperty to update the model as
            // needed.
            foreach (var item in pairs.AllKeys)
            {
                thing.SetProperProperty(item, pairs[item]);
            }

            if (autoSave) thing.Save();

            // Return the completed thing (note that we have not saved this thing)
            return thing;
        }

        public static IEnumerable<ThingModel> FromPartition(string partitionKey)
        {
            TableQuery<ThingModel> query = new TableQuery<ThingModel>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            var results = ThingTable.ExecuteQuery(query);
            return results;
        }

        public static IEnumerable<ThingModel> FromPropertyValue(string partitionKey, string property, string value)
        {
            string filterA = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            string filterB = TableQuery.GenerateFilterCondition(property, QueryComparisons.Equal, value);
            string combined = TableQuery.CombineFilters(filterA, TableOperators.And, filterB);
            TableQuery<ThingModel> query = new TableQuery<ThingModel>().Where(combined); 
            var results = ThingTable.ExecuteQuery(query);
            return results;
        }

        public static ThingModel FromRowKey(string partitionKey, string rowKey)
        {
            if (partitionKey == null) partitionKey = string.Empty;
            TableOperation retrieveOperation = TableOperation.Retrieve<ThingModel>(partitionKey, rowKey);
            TableResult retrievedResult = ThingTable.Execute(retrieveOperation);
            return retrievedResult.Result as ThingModel;
        }

        public void Delete()
        {
            var eventModel = new EventModel(this, EventType.Delete); 
            var result = ThingTable.Execute(TableOperation.Delete(this));
            eventModel.Send(); // Thing Deleted
        }

        public void Save()
        {
            if (this.RowKey != this.Id)
            {
                this.RowKey = this.Id;
                System.Diagnostics.Debug.WriteLine("Issue: RowKey did not match Id");
            }

            if (this.PartitionKey == null) this.PartitionKey = string.Empty;

            var previousState = FromRowKey(this.PartitionKey, this.RowKey);
            var tabelResult = ThingTable.Execute(TableOperation.InsertOrReplace(this));
            var currentState = tabelResult.Result as ThingModel;


            // Send events
            if(previousState == null)
            {
                // Thing Added
                EventModel eventModel = new EventModel(currentState, EventType.Add);
                eventModel.Send();
            }
            else
            {
                var thingDiff = this.ThingDiff(currentState, previousState);
                if (thingDiff.Count > 0)
                {
                    // Thing Changed
                    EventModel eventModel = new EventModel(currentState, EventType.Change);
                    foreach (var item in thingDiff)
                    {
                        eventModel.EventProperties.Add("diff_" + item.Key, item.Value.Item1);
                    }
                    eventModel.Send();
                }
            }            
        }
    }
}