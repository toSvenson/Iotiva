using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using AccidentalFish.AspNet.Identity.Azure;
using System.Net.Http;


namespace Iotiva.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : TableUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }

        private string _repoId;

        public string RepoId
        {
            get
            {
                /// Used to identify the repo for a user. This is used as the partition key for storage. 
                /// By default we simply use the user's ID (which this will return by default) but 
                /// allows for multiple user's to potentially share the same repo (under consideration)
                if (string.IsNullOrWhiteSpace(_repoId)) return this.Id;
                else return _repoId;
            }
            set { _repoId = value; }
        }
    }
}