using Newtonsoft.Json;
using proyecto_vivemas.Models;
using proyecto_vivemas.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace proyecto_vivemas.Controllers
{
    public class SeparacionesController : Controller
    {
        vivemas_dbEntities db = new vivemas_dbEntities();
        JsonResult respuesta;
        // GET: Separaciones
        public ActionResult Index()
        {
            return View();
        }

        public string cargarSeparaciones(long estadoSeparacionId)
        {
            string respuestajson = "";
            try
            {
                List<object> dataRespuesta = new List<object>();
                List<separaciones> listaSeparaciones = new List<separaciones>();
                listaSeparaciones = db.separaciones.Where(sep => sep.separacion_estadoseparacion_id == estadoSeparacionId).ToList();
                foreach (var itemSeparacion in listaSeparaciones)
                {
                    clientes cliente = db.clientes.Find(itemSeparacion.separacion_cliente_id);
                    lotes lote = db.lotes.Find(itemSeparacion.separacion_lote_id);
                    proyectos proyecto = db.proyectos.Find(lote.lote_proyecto_id);
                    transaccionesseparacion transaccionseparacion = db.transaccionesseparacion.OrderByDescending(tse => tse.transaccionseparacion_id).FirstOrDefault();
                    transacciones transaccion = db.transacciones.Find(transaccionseparacion.transaccionseparacion_transaccion_id);
                    monedas moneda = db.monedas.Find(transaccion.transaccion_moneda_id);
                    object itemRespuesta = new
                    {
                        itemSeparacion.separacion_id,
                        cliente.cliente_id,
                        lote.lote_id,
                        cliente.cliente_nrodocumento,
                        cliente.cliente_razonsocial,
                        proyecto.proyecto_nombrecorto,
                        lote.lote_nombre,
                        separacion_fechainicio = itemSeparacion.separacion_fechainicio.Value.ToString("dd/MM/yyyy"),
                        separacion_fechafin = itemSeparacion.separacion_fechafin.Value.ToString("dd/MM/yyyy"),
                        moneda.moneda_descripcion,
                        itemSeparacion.separacion_monto
                    };
                    dataRespuesta.Add(itemRespuesta);
                }
                respuestajson = JsonConvert.SerializeObject(dataRespuesta);
                return respuestajson;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult cargarProyectos()
        {
            try
            {
                long empresaId = long.Parse(Session["empresaId"].ToString());
                return Json(db.proyectos
                    .Where(pro => pro.proyecto_empresa_id == empresaId && pro.proyecto_estado == true)
                    .Select(pro => new
                    {
                        id = pro.proyecto_id,
                        text = pro.proyecto_nombrecorto
                    }));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult cargarLotes(long idProyecto)
        {
            try
            {
                long empresaId = long.Parse(Session["empresaId"].ToString());
                return Json(db.lotes
                    .Where(lot => lot.lote_proyecto_id == idProyecto && lot.lote_estadolote_id == 1)
                    .Select(lot => new
                    {
                        id = lot.lote_id,
                        text = lot.lote_nombre
                    }));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult cargarDatosLote(long idLote)
        {
            try
            {
                respuesta = new JsonResult();
                lotes lote = db.lotes.Find(idLote);
                respuesta.Data = new
                {
                    lote.lote_id,
                    lote.lote_areatotal,
                    lote.lote_preciometro,
                    lote.lote_precio
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult crearSeparacion(DataSeparaciones dataSeparacion)
        {
            try
            {
                long empresaId = long.Parse(Session["empresaId"].ToString());
                long usuarioId = long.Parse(Session["usuario"].ToString());
                using (TransactionScope transaction = new TransactionScope()){
                    #region modificacion del cliente
                    clientes cliente = db.clientes.Find(dataSeparacion.dataCliente.clienteId);
                    cliente.cliente_tipopersona_id = dataSeparacion.dataCliente.clienteTipoPersona;
                    cliente.cliente_tipodocumentoidentidad_id = dataSeparacion.dataCliente.clienteTipoDocumento;
                    cliente.cliente_nrodocumento = dataSeparacion.dataCliente.clienteNroDocumento;
                    cliente.cliente_nrocontacto = dataSeparacion.dataCliente.clienteNroContacto;
                    cliente.cliente_promotor_id = dataSeparacion.dataCliente.clientePromotor;
                    cliente.cliente_estadointerescliente_id = 2;//interes medio
                    db.Entry(cliente).State = EntityState.Modified;
                    db.SaveChanges();
                    #endregion
                    #region creacion de la transaccion
                    transacciones transaccion = new transacciones
                    {
                        transaccion_empresa_id = empresaId,
                        transaccion_tipotransaccion_id = 1,//SEPARACION
                        transaccion_tipomovimiento_id = 1, //INGRESO
                        transaccion_estadotransaccion_id = 1,//PENDIENTE DE VERIFICACION
                        transaccion_tipometodopago_id = dataSeparacion.dataTransaccion.transaccionMetodoPago,
                        transaccion_banco_id = dataSeparacion.dataTransaccion.transaccionBancoDestino,
                        transaccion_cuentabanco_id = dataSeparacion.dataTransaccion.transaccionCuentaDestino,
                        transaccion_codigodocumento = dataSeparacion.dataTransaccion.transaccionNroSeparacion,
                        transaccion_nrooperacion = dataSeparacion.dataTransaccion.transaccionNroOperacion,
                        transaccion_bancoorigen = dataSeparacion.dataTransaccion.transaccionBancoOrigen,
                        transaccion_monto = dataSeparacion.dataTransaccion.transaccionMonto,
                        transaccion_moneda_id = dataSeparacion.dataTransaccion.transaccionMoneda,
                        transaccion_usuariocreacion = usuarioId,
                        transaccion_fechacreacion = DateTime.Now
                    };
                    if(transaccion.transaccion_moneda_id != 2)//diferente de soles
                    {
                        tiposcambio tipocambio = db.tiposcambio.OrderByDescending(tca => tca.tipocambio_id).FirstOrDefault();
                        transaccion.transaccion_tipocambio_id = tipocambio.tipocambio_id;
                        transaccion.transaccion_tipocambio_fecha = tipocambio.tipocambio_fecha;
                    }
                    db.transacciones.Add(transaccion);
                    db.SaveChanges();
                    #endregion
                    #region creacion de la separacion
                    separaciones separacion = new separaciones
                    {
                        separacion_cliente_id = cliente.cliente_id,
                        separacion_lote_id = dataSeparacion.separacionLoteId,                       
                        separacion_estadoseparacion_id = 1, //ACTIVO
                        separacion_numeroseparacion = dataSeparacion.dataTransaccion.transaccionNroSeparacion,
                        separacion_fechainicio = DateTime.Now,
                        separacion_fechafin = DateTime.Now.AddDays(dataSeparacion.separacionTiempo),
                        separacion_monto = dataSeparacion.dataTransaccion.transaccionMonto,
                        separacion_fechacreacion = DateTime.Now,
                        separacion_usuariocreacion = usuarioId
                    };
                    db.separaciones.Add(separacion);
                    db.SaveChanges();
                    transaccionesseparacion transaccionSeparacion = new transaccionesseparacion {
                        transaccionseparacion_separacion_id = separacion.separacion_id,
                        transaccionseparacion_transaccion_id = transaccion.transaccion_id,
                        transaccionseparacion_fechacreacion = DateTime.Now,
                        transaccionseparacion_usuariocreacion = usuarioId
                    };
                    db.transaccionesseparacion.Add(transaccionSeparacion);
                    db.SaveChanges();
                    #endregion
                    #region actualizacion del lote
                    lotes lote = db.lotes.Find(dataSeparacion.separacionLoteId);
                    lote.lote_estadolote_id = 2; //EN SEPARACION
                    db.Entry(lote).State = EntityState.Modified;
                    db.SaveChanges();
                    #endregion
                    transaction.Complete();
                }
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 1
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 0
                };
                return respuesta;
            }
        }

        public ActionResult cargarEstadosSeparacion()
        {
            try
            {                
                return Json(db.estadosseparacion                    
                    .Select(ese => new
                    {
                        id = ese.estadoseparacion_id,
                        text = ese.estadoseparacion_descripcion
                    }));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult actualizarLote(DataSeparaciones dataSeparacion)
        {
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    separaciones separacion = db.separaciones.Find(dataSeparacion.separacionId);
                    lotes loteAntiguo = db.lotes.Find(separacion.separacion_lote_id);
                    loteAntiguo.lote_estadolote_id = 1;//DISPONIBLE
                    db.Entry(loteAntiguo).State = EntityState.Modified;                    
                    lotes loteNuevo = db.lotes.Find(dataSeparacion.separacionLoteId);
                    loteNuevo.lote_estadolote_id = 2;//EN SEPARACION
                    db.Entry(loteNuevo).State = EntityState.Modified;                    
                    separacion.separacion_lote_id = loteNuevo.lote_id;
                    db.Entry(separacion).State = EntityState.Modified;
                    db.SaveChanges();
                    transaction.Complete();
                }
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 1
                };
                return respuesta;
            }catch(Exception ex)
            {
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 0
                };
                return respuesta;
            }
        }
    }
}