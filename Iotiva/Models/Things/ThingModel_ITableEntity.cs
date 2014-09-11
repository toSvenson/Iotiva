using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Iotiva.Models.Things
{
    public partial class ThingModel : ITableEntity
    {
        public string ETag { get; set; }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            foreach (var item in properties)
            {
                SetProperProperty(item.Key, item.Value.PropertyAsObject);
            }
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var _entities = new Dictionary<string, EntityProperty>(StringComparer.InvariantCultureIgnoreCase);

            // Add all of the properties from the property bag
            foreach (var item in _properties)
            {
                _entities.Add(item.Key, new EntityProperty(item.Value));
            }

            // Manually add the non-property bag properties we track
            _entities.Add("Id", new EntityProperty(Id));
            _entities.Add("Name", new EntityProperty(Name));
            _entities.Add("Type", new EntityProperty(Type));
            _entities.Add("Agent", new EntityProperty(Agent));

            return _entities;
        }
    }
}