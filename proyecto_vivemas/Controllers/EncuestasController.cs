using proyecto_vivemas.Models;
using proyecto_vivemas.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Transactions;

namespace proyecto_vivemas.Controllers
{
    /// <summary>
    /// Clase controlador para las encuestas
    /// </summary>
    /// <remarks>
    /// Esta clase puede mostrar vistas relacionadas con las encuestas, puede enviar informacion de las encuestas o informacion necesaria de las encuestas
    /// </remarks>
    public class EncuestasController : Controller
    {
        /// <value>
        /// JsonResult usado para devolver informacion a la vista
        /// </value>
        JsonResult respuesta;
        /// <value>
        /// Objeto que conecta el modelo con el controlador 
        /// </value>
        vivemas_dbEntities db = new vivemas_dbEntities();
        // GET: Encuestas
        /// <summary>crea la instancia de la vista</summary>
        /// <returns>vista de encuesta de satisfaccion del cliente</returns>
        public ActionResult EncuestaSatisfaccionCliente()
        {
            return View();
        }

        /// <summary>
        /// funcion encargada de los datos para la encuesta de satisfaccion
        /// </summary>
        /// <returns><c>json</c> con los datos de la encuesta</returns>
        public ActionResult cargarEncuestaSatisfaccionCliente()
        {
            try
            {
                tiposencuesta encuesta = db.tiposencuesta.Find(1);
                List<preguntas> listaPreguntas = db.preguntas.Where(pre => pre.pregunta_tipoencuesta_id == 1).ToList();
                List<respuestas> listaRespuestas = new List<respuestas>();
                List<object> encuestaRespuesta = new List<object>();
                foreach (preguntas preguntaItem in listaPreguntas)
                {
                    object item = new { 
                        pregunta = new { 
                            preguntaItem.pregunta_id,
                            preguntaItem.pregunta_descripcion
                        },
                        respuesta = db.respuestas.Where(res => res.respuesta_pregunta_id == preguntaItem.pregunta_id).ToList().Select(res => new { 
                            res.respuesta_id,
                            res.respuesta_pregunta_id,
                            res.respuesta_descripcion
                        })
                    };
                    encuestaRespuesta.Add(item);
                }
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    data = encuestaRespuesta,
                    flag = 1
                };
            }
            catch
            {
                respuesta = new JsonResult();
                respuesta.Data = new {
                    flag = 0
                };
            }
            respuesta.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return respuesta;
        }

        /// <summary>
        /// funcion encargada de guardar las encuestas en la base de datos
        /// </summary>
        /// <returns><c>json</c> con los resultados de la operacion</returns>
        public ActionResult procesarEncuesta(DataEncuesta dataEncuesta)
        {
            try
            {
                using(TransactionScope transaction = new TransactionScope())
                {
                    datosencuesta datoEncuesta = new datosencuesta
                    {
                        datoencuesta_tipoencuesta_id = 1, //encuesta de satisfaccion del cliente
                        datoencuesta_cliente_nombre = dataEncuesta.clienteNombre,
                        datoencuesta_fecha = DateTime.Now,
                        datoencuesta_observaciones  = dataEncuesta.observaciones
                    };
                    db.datosencuesta.Add(datoEncuesta);
                    db.SaveChanges();
                    foreach(var itemPregunta in dataEncuesta.encuesta)
                    {
                        encuestas encuesta = new encuestas
                        {
                            encuesta_datoencuesta_id = datoEncuesta.datoencuesta_id,
                            encuesta_pregunta_id = itemPregunta.preguntaId,
                            encuesta_respuesta_id = itemPregunta.respuestaId
                        };
                        db.encuestas.Add(encuesta);
                        db.SaveChanges();
                    }
                    transaction.Complete();
                }
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 1
                };
            }catch(Exception ex)
            {
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 0
                };
            }
            return respuesta;
        }
    }
}