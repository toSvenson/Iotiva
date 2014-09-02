using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Iotiva.Models.Events
{
    public partial class EventModel : ITableEntity
    {
        public string ETag { get; set; }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, Microsoft.WindowsAzure.Storage.OperationContext operationContext)
        {
            foreach (var item in properties)
            {
                SetProperProperty(item.Key, item.Value.PropertyAsObject);
            }
        }

        public IDictionary<string, EntityProperty> WriteEntity(Microsoft.WindowsAzure.Storage.OperationContext operationContext)
        {
            var _entities = new Dictionary<string, EntityProperty>(StringComparer.InvariantCultureIgnoreCase);

            // Add all of the properties from the property bag
            foreach (var item in EventProperties)
            {
                _entities.Add(item.Key, new EntityProperty(item.Value));
            }

            return _entities;
        }
    }
}