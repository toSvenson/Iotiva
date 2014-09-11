using Iotiva.Models.Things;
using System.Web.Mvc;
using System.Linq;

namespace Iotiva.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult BrowserTest()
        {
            ViewBag.Title = "Browser Test";
            return View();
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard";
            var user = Lib.UserUtils.GetUser(this);

            return View(new Iotiva.Models.DashboardModel(user.RepoId));
        }

        public ActionResult RepoItems()
        {
            ViewBag.Title = "Repository";

            var user = Lib.UserUtils.GetUser(this);

            var things = ThingModel.FromPartition(user.RepoId);
            //if (string.IsNullOrEmpty(user.RepoId))
            //    return View(things.OrderByDescending(c => c.Timestamp.DateTime).Take(20));
            //else
                return View(things);
        }

        public ActionResult RepoEdit(string id)
        {
            ViewBag.Title = "Edit - " + id;
            var user = Lib.UserUtils.GetUser(this);
            var thing = ThingModel.FromRowKey(user.RepoId, id);
            if (thing != null) 
                return View(thing);
            else
                return RedirectToAction("RepoItems", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateItem(ThingModel model)
        {
            var user = Lib.UserUtils.GetUser(this);
            model.PartitionKey = user.RepoId;
            model.RowKey = model.Id;
            model.Save();
            return RedirectToAction("RepoItems", "Home");
        }
    }
}