using Newtonsoft.Json;
using proyecto_vivemas.Models;
using proyecto_vivemas.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace proyecto_vivemas.Controllers
{
    public class TransaccionesController : Controller
    {
        vivemas_dbEntities db = new vivemas_dbEntities();
        JsonResult respuesta;
        // GET: Transacciones
        public ActionResult Transacciones()
        {
            return View();
        }

        public string CargarTiposTransaccion()
        {
            string respuesta = "";
            try
            {
                List<tipostransaccion> listaTiposTransaccion = db.tipostransaccion.ToList();

                respuesta = JsonConvert.SerializeObject(listaTiposTransaccion.Select(ttr => new
                {
                    ttr.tipotransaccion_id,
                    ttr.tipotransaccion_descripcion
                }));
                return respuesta;
            }
            catch (Exception ex)
            {
                return respuesta;
            }
        }

        public string CargarTransaccionesPorTipo(long tipoTransaccion)
        {
            string respuesta = "";

            try
            {
                List<object> listaRespuesta = new List<object>();
                List<long> lista = new List<long>();
                lista.Add(4);
                lista.Add(2);
                List<transacciones> listaTransacciones = db.transacciones.Where(tra => tra.transaccion_tipotransaccion_id == tipoTransaccion && tra.transaccion_estadotransaccion_id != 4 && tra.transaccion_estadotransaccion_id != 5).OrderByDescending(tra => tra.transaccion_id).ToList();
                if (tipoTransaccion == 1)
                { //SEPARACIONES
                    foreach (transacciones transaccionItem in listaTransacciones)
                    {
                        transaccionesseparacion transaccionSeparacion = db.transaccionesseparacion
                            .Where(tse => tse.transaccionseparacion_transaccion_id == transaccionItem.transaccion_id)
                            .FirstOrDefault();
                        separaciones separacion = db.separaciones.Find(transaccionSeparacion.transaccionseparacion_separacion_id);
                        object itemRespuesta = new
                        {
                            transaccionItem.transaccion_id,
                            transaccionItem.transaccion_numeracion,
                            separacion.clientes.cliente_razonsocial,
                            separacion.clientes.cliente_nrodocumento,
                            separacion.lotes.lote_nombre,
                            separacion.lotes.proyectos.proyecto_nombrecorto,
                            transaccionItem.tiposmetodopago.tipometodopago_descripcion,
                            transaccionItem.transaccion_nrooperacion,
                            fechaTransaccion = transaccionItem.transaccion_fecha.Value.ToString("dd/MM/yyyy"),
                            fechaDeposito = transaccionItem.transaccion_fechadeposito.Value.ToString("dd/MM/yyyy"),
                            transaccionItem.cuentasbanco.bancos.banco_descripcioncorta,
                            transaccionItem.cuentasbanco.cuentabanco_cuenta,
                            transaccionItem.transaccion_bancoorigen,
                            transaccionItem.transaccion_monto,
                            transaccionItem.transaccion_estadoemision
                        };
                        listaRespuesta.Add(itemRespuesta);
                    }
                    respuesta = JsonConvert.SerializeObject(listaRespuesta);
                    return respuesta;
                }
                else if (tipoTransaccion == 3)//PAGOS CASH
                {
                    foreach (transacciones transaccionItem in listaTransacciones)
                    {
                        sp_cargarDataTransaccion_Result data = db.sp_cargarDataTransaccion(transaccionItem.transaccion_id).FirstOrDefault();
                        listaRespuesta.Add(data);
                    }
                    respuesta = JsonConvert.SerializeObject(listaRespuesta);
                    return respuesta;
                }
                else if (tipoTransaccion == 4)
                {//PAGOS CUOTA
                    foreach (transacciones transaccionItem in listaTransacciones)
                    {
                        sp_cargarDataTransaccion_Result data = db.sp_cargarDataTransaccion(transaccionItem.transaccion_id).FirstOrDefault();
                        listaRespuesta.Add(data);
                    }
                    respuesta = JsonConvert.SerializeObject(listaRespuesta);
                    return respuesta;
                }


            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public ActionResult CargarTransaccionPorId(long idTransaccion)
        {
            try
            {
                transacciones transaccion = db.transacciones.Find(idTransaccion);
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 1,
                    transaccionRespuesta = new
                    {
                        transaccion.transaccion_cuentabanco_id,
                        transaccion.transaccion_banco_id,
                        transaccion_fechadeposito = transaccion.transaccion_fechadeposito == null ? null : transaccion.transaccion_fechadeposito.Value.ToString("dd/MM/yyyy"),
                        transaccion.transaccion_nrooperacion
                    }
                };
                respuesta.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
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

        public ActionResult CargarObservacionTransaccion(long idTransaccion)
        {
            try
            {
                transacciones transaccion = db.transacciones.Find(idTransaccion);
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 1,
                    transaccion.transaccion_observaciones

                };
                respuesta.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            }
            catch (Exception ex)
            {
                respuesta.Data = new
                {
                    flag = 0
                };
            }
            return respuesta;
        }

        public ActionResult CargarProyectos()
        {
            try
            {
                return Json(db.proyectos
                    .Select(tmp => new
                    {
                        id = tmp.proyecto_id,
                        text = tmp.proyecto_nombrecorto
                    }));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult CargarTiposTransaccionSelect()
        {
            try
            {
                return Json(db.tipostransaccion
                    .Select(tmp => new
                    {
                        id = tmp.tipotransaccion_id,
                        text = tmp.tipotransaccion_descripcion
                    }));
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public ActionResult cargarMetodosPago()
        {
            try
            {
                return Json(db.tiposmetodopago
                    .Where(tmp => tmp.tipometodopago_estado == true)
                    .Select(tmp => new
                    {
                        id = tmp.tipometodopago_id,
                        text = tmp.tipometodopago_descripcion
                    }));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult cargarBancos()
        {
            try
            {
                return Json(db.bancos
                    .Where(ban => ban.banco_estado == true)
                    .Select(ban => new
                    {
                        id = ban.banco_id,
                        text = ban.banco_descripcioncorta
                    }));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult cargarMonedas()
        {
            try
            {
                return Json(db.monedas
                    .Select(mon => new
                    {
                        id = mon.moneda_id,
                        text = mon.moneda_descripcion
                    }));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult cargarCuentas(long idBanco, long idMoneda)
        {
            try
            {
                return Json(db.cuentasbanco
                    .Where(cba => cba.cuentabanco_banco_id == idBanco && cba.cuentabanco_moneda_id == idMoneda && cba.cuentabanco_estado == true)
                    .Select(cba => new
                    {
                        id = cba.cuentabanco_id,
                        text = cba.cuentabanco_cuenta
                    }));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult cargarCuentasBanco(long idBanco)
        {
            try
            {
                return Json(db.cuentasbanco
                    .Where(cba => cba.cuentabanco_banco_id == idBanco && cba.cuentabanco_estado == true)
                    .Select(cba => new
                    {
                        id = cba.cuentabanco_id,
                        text = cba.cuentabanco_cuenta
                    }));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult actualizarTransaccion(DataTransacciones transaccionModificada)
        {
            try
            {
                transacciones transaccion = db.transacciones.Find(transaccionModificada.transaccionId);
                using (TransactionScope transaction = new TransactionScope())
                {
                    transaccion.transaccion_banco_id = transaccionModificada.transaccionBancoDestino;
                    transaccion.transaccion_cuentabanco_id = transaccionModificada.transaccionCuentaDestino;
                    transaccion.transaccion_nrooperacion = transaccionModificada.transaccionNroOperacion;
                    if (transaccionModificada.transaccionFechaDeposito != null)
                    {
                        transaccion.transaccion_fechadeposito = DateTime.Parse(transaccionModificada.transaccionFechaDeposito);
                    }
                    db.Entry(transaccion).State = EntityState.Modified;
                    db.SaveChanges();
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
                respuesta.Data = new
                {
                    flag = 0
                };
                return respuesta;
            }
        }

        public ActionResult anularTransaccion(long transaccionId, long tipoTransaccion)
        {
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    transacciones transaccion = db.transacciones.Find(transaccionId);
                    transaccion.transaccion_estadotransaccion_id = 4; //ANULADO
                    db.Entry(transaccion).State = EntityState.Modified;
                    db.SaveChanges();
                    if (tipoTransaccion == 4) //PAGO DE CUOTAS
                    {
                        List<pagos> listaPagos = db.pagos.Where(pag => pag.pago_transaccion_id == transaccion.transaccion_id).ToList();
                        foreach (pagos itemPago in listaPagos)
                        {
                            cuotas cuota = db.cuotas.Find(itemPago.pago_cuota_id);
                            cuota.cuota_montopagado = cuota.cuota_montopagado - itemPago.pago_cuota_monto;
                            cuota.cuota_estado = true;
                            db.Entry(cuota).State = EntityState.Modified;
                            db.SaveChanges();
                            itemPago.pago_estado = false;
                            db.Entry(itemPago).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    if (transaccion.transaccion_evento_id != null)
                    {
                        eventos evento = db.eventos.Find(transaccion.transaccion_evento_id);
                        evento.evento_estado = true;
                        evento.evento_estadoevento_id = 1;//EN CURSO
                        db.Entry(evento).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    transaction.Complete();
                    respuesta = new JsonResult();
                    respuesta.Data = new
                    {
                        flag = 1
                    };
                }
            }
            catch (Exception ex)
            {
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 0
                };
            }
            return respuesta;
        }

        public ActionResult procesarTransacciones(DataTransaccionesProcesado data)
        {
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    foreach (var item in data.listaTransacciones)
                    {
                        transacciones transaccion = db.transacciones.Find(item.transaccionId);
                        transaccion.transaccion_estadotransaccion_id = 5;//validado/procesado
                        db.Entry(transaccion).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    transaction.Complete();

                }
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 1
                };
            }
            catch (Exception ex)
            {
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 0
                };
            }
            return respuesta;
        }

        public ActionResult actualizarObservacion(DataTransacciones data)
        {
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    transacciones transaccion = db.transacciones.Find(data.transaccionId);
                    transaccion.transaccion_observaciones = data.transaccionObservaciones;
                    db.Entry(transaccion).State = EntityState.Modified;
                    db.SaveChanges();
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