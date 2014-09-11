using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iotiva.Models
{
    public class DashboardModel
    {
        public List<Things.ThingModel> RecentUpdates { get; set; }

        public int ThingCount { get; set; }

        public int EventCount { get; set; }

        public DashboardModel(string repoId)
        {
            var things = Things.ThingModel.FromPartition(repoId);
            RecentUpdates = things.OrderByDescending(c => c.Timestamp).Take(6).ToList();

            ThingCount = Things.ThingModel.RepositoryCount(repoId);
            EventCount = Events.EventModel.EventCount(repoId);
        }
    }
}