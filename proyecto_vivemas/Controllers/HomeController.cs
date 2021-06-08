using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;

namespace proyecto_vivemas.Controllers
{

    public class HomeController : Controller
    {
        /// <summary>crea la instancia de la vista</summary>
        /// <remarks>
        /// es el login
        /// </remarks>
        /// <returns>vista de mantenimiento de pagos</returns>
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        /// <summary>
        /// Es la pagina principal la que aparece cuando logeas correctamente
        /// </summary>
        /// <returns></returns>
        public ActionResult FrontPage()
        {
            return View();
        }
    }
}