using Iotiva.Models;
using Iotiva.Models.Events;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity.Owin;
using Iotiva.Models.Things;

namespace Iotiva.Controllers
{
    /// <summary>
    /// Things are... well, Things. Frankly, any detailed explanation quickly becomes circular enough to start
    /// making one's view of the world look like a Salvador Dali painting. That said, these are the Nouns you
    /// are looking.
    /// </summary>
    [Authorize]
    public class ThingController : ApiController
    {
        #region Collections - api/things

        /// <summary>
        /// Return an array of Things where the specified property is equal to the supplied value
        /// </summary>
        /// <param name="property">The name of the property (eg. Name)</param>
        /// <param name="value">The value of that property you wish to match. (eg. MyThing)</param>
        /// <param name="Id">Unique identifier for a Thing (Optional)</param>
        /// <returns>Returns an array of Things matching the requested filter parameters</returns>
        [Route("api/things/where/{property}/{value}")]
        [HttpGet]
        public IEnumerable<ThingModel> FindThings(string property, string value)
        {
            if (string.IsNullOrWhiteSpace(property)) throw new HttpResponseException(HttpStatusCode.Forbidden);
            return ThingModel.FromPropertyValue(Lib.UserUtils.GetUser(this).RepoId, property, value);
        }

        /// <summary>
        /// Return an array of Things. For non-authenticated users this will return the 20 most recently Things.
        /// </summary>
        [AllowAnonymous]
        [Route("api/things")]
        [HttpGet]
        public IEnumerable<ThingModel> GetThings()
        {
            var user = Lib.UserUtils.GetUser(this);

            if (string.IsNullOrEmpty(user.RepoId))
            {
                var things = ThingModel.FromPartition(user.RepoId);
                return things.OrderByDescending(c => c.Timestamp.DateTime).Take(20);
            }
            return ThingModel.FromPartition(user.RepoId);
        }

        #endregion Collections - api/things

        #region Single Objects - api/thing

        /// <summary>
        /// Delete the specified Thing
        /// </summary>
        /// <param name="Id">The unique Id of the Thing you wish to delete</param>
        [Route("api/thing/{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteThing(string id)
        {
            try
            {
                var thing = ThingModel.FromRowKey(Lib.UserUtils.GetUser(this).RepoId, id);
                thing.Delete();
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Return the specified Thing
        /// </summary>
        /// <param name="Id">The unique Id of the Thing you wish to return</param>
        [AllowAnonymous]
        [Route("api/thing/{id}")]
        [HttpGet]
        public ThingModel GetThing(string id)
        {
            try
            {
                return ThingModel.FromRowKey(Lib.UserUtils.GetUser(this).RepoId, id);
            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Create/Update a Thing with the specified Id.
        /// </summary>
        /// <param name="Id">The unique Id of the Thing you wish to update</param>
        [AllowAnonymous]
        [Route("api/thing/{id}")]
        [HttpPost]
        public ThingModel UpdateThing(string id, [FromBody] FormDataCollection form)
        {
            /* The ThingModel holds the logic for converting NameValueCollectio into a Thing
             * (or updating an existing thing if it has one already). We simply pass it along
             * and return the results. */
            var dataSet = form.ReadAsNameValueCollection();
            dataSet["id"] = id;
            return ThingModel.FromNameValueCollection(Lib.UserUtils.GetUser(this).RepoId, dataSet, true);
        }

        /// <summary>
        /// Create a Thing. If an Id is defined in the data set that Id will be used.
        /// If an Id is defined and already exists in the repository the existing record will be updated.
        /// </summary>
        [AllowAnonymous]
        [Route("api/thing/")]
        [HttpPost]
        public ThingModel UpdateThing([FromBody] FormDataCollection form)
        {
            return ThingModel.FromNameValueCollection(Lib.UserUtils.GetUser(this).RepoId, form.ReadAsNameValueCollection(), true);
        }

        #endregion Single Objects - api/thing

        #region Messaging - api/thing/{id}/send

        /// <summary>
        /// Send a message to a Thing via the Queue. Post body is passed as raw text.
        /// </summary>
        /// <param name="Id">The unique Id of the Thing you wish to message</param>
        [AllowAnonymous]
        [Route("api/thing/{id}/send")]
        [HttpPost]
        public HttpResponseMessage PostMessage(string id, HttpRequestMessage message)
        {
            try
            {
                var thing = ThingModel.FromRowKey(Lib.UserUtils.GetUser(this).RepoId, id);
                var eventModel = new EventModel(thing, EventType.Message);
                eventModel["EventMessage"] = message.Content.ReadAsStringAsync().Result;

                foreach (var item in thing.Properties)
                {
                    eventModel.EventProperties.Add(item.Key, item.Value);
                }
                eventModel.Send();
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Send a message to a Thing via the Queue
        /// </summary>
        /// <param name="Id">The unique Id of the Thing you wish to message</param>
        /// <param name="message">The message string to deliver</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/thing/{id}/send/{message}")]
        [HttpGet]
        public HttpResponseMessage SendMessage(string id, string message)
        {
            try
            {
                var thing = ThingModel.FromRowKey(Lib.UserUtils.GetUser(this).RepoId, id);
                var eventModel = new EventModel(thing, EventType.Message);
                eventModel["EventMessage"] = message;

                foreach (var item in thing.Properties)
                {
                    eventModel.EventProperties.Add(item.Key, item.Value);
                }
                eventModel.Send();
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        #endregion Messaging - api/thing/{id}/send

        [AcceptVerbs("OPTIONS"), ApiExplorerSettings(IgnoreApi = true)]
        [AllowAnonymous]
        public HttpResponseMessage Options()
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Methods", "GET,POST, DELETE");

            return resp;
        }
    }
}