using proyecto_vivemas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace proyecto_vivemas.Controllers
{
    public class LotesController : Controller
    {
        
        vivemas_dbEntities db = new vivemas_dbEntities();
        // GET: Lotes
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult buscarLotesAutocomplete(string term, string param1)
        {
            try
            {
                long proyectoId = long.Parse(param1);
                if(proyectoId == 0)
                {
                    return Json(db.vw_busquedaContratosAutocomplete
                    .Where(vw => vw.lote_nombre.Contains(term))
                    .Select(vw => new {
                        id = vw.contrato_id,
                        loteid = vw.lote_id,
                        label = vw.lote_nombre + "-" + vw.proyecto_nombrecorto,
                        lote = vw.lote_nombre,
                        numeracion = vw.contrato_numeracion,
                        nombre = vw.cliente_nrodocumento + "-" + vw.cliente_razonsocial
                    }).Take(10), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(db.vw_busquedaContratosAutocomplete
                   .Where(vw => vw.lote_nombre.Contains(term) && vw.lote_proyecto_id == proyectoId)
                   .Select(vw => new {
                       id = vw.contrato_id,
                       label = vw.lote_nombre + "-" + vw.proyecto_nombrecorto,
                       lote = vw.lote_nombre,
                       numeracion = vw.contrato_numeracion,
                       nombre = vw.cliente_nrodocumento + "-" + vw.cliente_razonsocial
                   }).Take(10), JsonRequestBehavior.AllowGet);
                }

            }catch(Exception ex)
            {

            }
            return null;
        }
    }
}