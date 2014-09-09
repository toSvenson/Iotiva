using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Iotiva.Models.Things;
using System.Threading.Tasks;

namespace Iotiva.Controllers
{

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult BrowserTest()
        {
            ViewBag.Title = "Browser Test";
            return View();
        }

        public ActionResult RepoItems()
        {
            ViewBag.Title = "Repository";

            var user = Lib.UserUtils.GetUser(this);
            return View(ThingModel.FromPartition(user.RepoId));
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
            return new EmptyResult();
        }


    }
}
