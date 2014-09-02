namespace Iotiva.Models.Events
{
    public partial class EventModel
    {
        public object GetProperProperty(string key)
        {
            switch (key.ToLower())
            {
                default:
                    return EventProperties[key];
            }
        }

        private void SetProperProperty(string key, object value)
        {
            switch (key.ToLower())
            {
                default:
                    if (EventProperties.ContainsKey(key)) EventProperties[key] = value as string;
                    else EventProperties.Add(key, value as string);
                    break;
            }
        }
    }
}