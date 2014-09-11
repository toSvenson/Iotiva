using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iotiva.Models.Things
{
    /// <summary>
    /// Represents a Thing and it's various properties.
    /// </summary>
    [DataContract]
    public partial class ThingModel
    {
        private IDictionary<string, string> _properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public ThingModel()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        public ThingModel(string partitionKey)
            : this()
        {
            this.PartitionKey = partitionKey;
            this.RowKey = Guid.NewGuid().ToString("N");
            this.Id = this.RowKey;
        }

        public ThingModel(string partitionKey, string id)
            : this()
        {
            this.PartitionKey = partitionKey;
            this.RowKey = id;
            this.Id = id;
        }

        /// <summary>
        /// Name or Id of the external "agent" responsible for this Thing (optional)
        /// Agents are typically a central control unit or HUB with a local connection to the individual Thing.
        /// For example, in a Home Automation scenario the Thing might be a light switch and it's Agent
        /// would be it's controller. In most cases an Agent is a software package running locally which connects
        /// Iotiva to Things that otherwise would not be aware of Iotiva.
        /// </summary>
        [DataMember]
        public string Agent { get; set; }

      
        /// <summary>
        /// Unique identifier (Required)
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Common/Display Name (option)
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Defines a Type of thing (eg. make and model)
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        public string Title
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return Id;
                else return Name;
            }
        }

        /// <summary>
        /// An ad-hoc collection of properties for a Thing (option)
        /// </summary>
        [DataMember]
        public IDictionary<string, string> Properties
        {
            get
            {
                return _properties;
            }
        }

        public string this[string key]
        {
            get
            {
                return GetProperProperty(key).ToString();
            }
        }
    }
}