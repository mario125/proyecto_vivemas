using proyecto_vivemas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace proyecto_vivemas.Controllers
{
    public class ProyectosController : Controller
    {
        JsonResult respuesta;
        vivemas_dbEntities db = new vivemas_dbEntities();
        // GET: Proyectos
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult cargarProyectos()
        {
            respuesta = new JsonResult();
            try
            {

                List<object> dataProyectos = new List<object>();
                List<proyectos> listaProyectos = db.proyectos.ToList();
                dataProyectos.Add(new { 
                    id = 0,
                    text = "TODOS"
                });
                foreach (var itemProyecto in listaProyectos)
                {
                    object dataProyecto = new
                    {
                        id = itemProyecto.proyecto_id,
                        text = itemProyecto.proyecto_nombrecorto
                    };
                    dataProyectos.Add(dataProyecto);
                }
                respuesta.Data = new
                {
                    results = dataProyectos
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}