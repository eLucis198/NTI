using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CentralR.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOff()
        {
            if (Session["Logado"] != null)
            {
                Session.Remove("Logado");
            }
            if (Session["Administrador"] != null)
            {
                Session.Remove("Administrador");
            }
            return RedirectToAction("Index", "Login");
        }
    }
}