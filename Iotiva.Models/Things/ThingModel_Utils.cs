using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Iotiva.Models
{
    public partial class ThingModel
    {
        public object GetProperProperty(string key)
        {
            switch (key.ToLower())
            {
                case "id":
                    return Id;

                case "name":
                    return Name;

                case "description":
                    return Description;

                case "agent":
                    return Agent;

                default:
                    return _properties[key];
            }
        }

        private void SetProperProperty(string key, object value)
        {
            switch (key.ToLower())
            {
                case "id":
                    Id = value as string;
                    break;

                case "name":
                    Name = value as string;
                    break;

                case "description":
                    Description = value as string;
                    break;

                case "agent":
                    Agent = value as string;
                    break;

                default:
                    if (_properties.ContainsKey(key)) _properties[key] = value as string;
                    else _properties.Add(key, value as string);
                    break;
            }
        }

        private Dictionary<string, Tuple<string, string>> ThingDiff(ThingModel newThing, ThingModel oldThing)
        {
            var results = new Dictionary<string, Tuple<string, string>>();

            if (newThing == null || oldThing == null) return results;

            // Name Change
            if (newThing.Name != oldThing.Name)
            {
                results.Add("Name", new Tuple<string, string>(oldThing.Name, newThing.Name));
            }

            // ID Change
            if (newThing.Id != oldThing.Id)
            {
                results.Add("Id", new Tuple<string, string>(oldThing.Id, newThing.Id));
            }

            if (newThing.Description != oldThing.Description)
            {
                results.Add("Description", new Tuple<string, string>(oldThing.Description, newThing.Description));
            }

            var comparer = EqualityComparer<string>.Default;

            foreach (KeyValuePair<string, string> kvp in newThing.Properties)
            {
                string secondValue;
                if (!oldThing.Properties.TryGetValue(kvp.Key, out secondValue))
                    results.Add(kvp.Key, new Tuple<string, string>(string.Empty, kvp.Value));

                if (!comparer.Equals(kvp.Value, secondValue))
                    results.Add(kvp.Key, new Tuple<string, string>(secondValue, kvp.Value));
            }
            return results;
        }


        private void StateChangedEvent(ThingModel current, ThingModel previous)
        {
            var thingDiff = this.ThingDiff(current, previous);

            if (thingDiff.Count == 0)
            {
                // No actual changes
                System.Diagnostics.Debug.WriteLine("No Changes Found: " + current.Id);
                return;
            }

            EventModel eventModel = new EventModel(current);
            foreach (var item in thingDiff)
            {
                eventModel.EventProperties.Add(item.Key, item.Value.Item1);
            }
            eventModel.Send();
        }
    }
}