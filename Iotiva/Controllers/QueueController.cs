using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Filters;
using Microsoft.AspNet.Identity;
using Iotiva.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;

namespace Iotiva.Controllers
{
    /// <summary>
    /// Returns events related to Things in the repository. This can be used to track the adds, deletes and changes. 
    /// </summary>
    [Authorize]
    public class QueueController : ApiController
    {
        // GET: api/Queue
        /// <summary>
        /// Retrieves the top 30 messages from the authenticated user's queue. Messages are automatically removed from the 
        /// queue when they are retrieved (i.e. this is a destructive call). 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public IEnumerable<EventModel> Get()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            /* Get the name of the queue to fetch from. If there is an authenticated user
             * we use the PartionKey as part of the name. Otherwise we use
             * the public events queue */
            var queueClient = storageAccount.CreateCloudQueueClient();
            string queueName = "events";
            if (User.Identity.IsAuthenticated)
            {
                queueName = "events" + User.Identity.GetUserId();
            }
            var eventQueue = queueClient.GetQueueReference(queueName);
            eventQueue.CreateIfNotExists();

            var messages = eventQueue.GetMessages(30);

            if (messages == null || messages.Count() == 0) throw new HttpResponseException(HttpStatusCode.NoContent);

            var eventMessages = new List<EventModel>();
            foreach (var message in messages)
            {
                var eventMessage = EventModel.FromRowKey(User.Identity.GetUserId(), message.AsString); // Find the message content
                if (eventMessage != null) eventMessages.Add(eventMessage); // If it exists, add it to the return set
                eventQueue.DeleteMessage(message); // Remove from the queue                
            }

            return eventMessages;
        }


    }
}
