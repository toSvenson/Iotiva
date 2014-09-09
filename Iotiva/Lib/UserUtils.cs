using Iotiva.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

using System.Web.Http;

namespace Iotiva.Lib
{
    public class UserUtils
    {
        public static ApplicationUser GetUser(ApiController controller)
        {
            var currentUserId = controller.User.Identity.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                ApplicationUser emptyUser = new ApplicationUser();
                emptyUser.Id = string.Empty;
                emptyUser.RepoId = string.Empty;
                emptyUser.RowKey = string.Empty;
                emptyUser.PartitionKey = string.Empty;
                return emptyUser;
            }
            var manager = controller.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var currentUser = manager.FindById(currentUserId);
            return currentUser;
        }

        public static ApplicationUser GetUser(System.Web.Mvc.Controller controller)
        {
            var currentUserId = controller.User.Identity.GetUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                ApplicationUser emptyUser = new ApplicationUser();
                emptyUser.Id = string.Empty;
                emptyUser.RepoId = string.Empty;
                emptyUser.RowKey = string.Empty;
                emptyUser.PartitionKey = string.Empty;
                return emptyUser;
            }
            var manager = controller.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var currentUser = manager.FindById(currentUserId);
            return currentUser;
        }
    }
}