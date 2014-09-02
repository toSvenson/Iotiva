using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Iotiva.Models.Things;

namespace Iotiva.Models.Events
{
    public enum EventType { Change, Add, Delete, Message }

    [DataContract]
    public partial class EventModel
    {
        private ThingModel _thing;

        public EventModel()
        {
            EventProperties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            EventProperties.Add("EventTime", DateTime.UtcNow.ToString());
        }
        public EventModel(ThingModel thing) :this()
        {
            _thing = thing;

            this.PartitionKey = thing.PartitionKey;
            this.RowKey = Guid.NewGuid().ToString("N");

            EventProperties.Add("Id", thing.Id);
            EventProperties.Add("EventType", EventType.Change.ToString()); // Set the default type

            if (!string.IsNullOrWhiteSpace(thing.Agent)) EventProperties.Add("Agent", thing.Agent);
        }

        public EventModel(ThingModel thing, EventType type)
            : this(thing)
        {
            EventProperties["EventType"] = type.ToString();            

            switch (type)
            {
                case EventType.Change:
                    break;
                case EventType.Add:
                    break;
                case EventType.Delete:
                    break;
                case EventType.Message:
                    EventProperties.Add("EventMessage", string.Empty); // Add if this is a message
                    break;
                default:
                    break;
            }
        }

        [DataMember]
        public Dictionary<string, string> EventProperties { get; set; }

        public string this[string key]
        {
            get
            {
                return GetProperProperty(key).ToString();
            }
            set
            {
                SetProperProperty(key, value.ToString());
            }
        }
    }
}