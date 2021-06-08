using Newtonsoft.Json.Linq;
using proyecto_vivemas.Models;
//using proyecto_vivemas.servicio_local_emision;
using proyecto_vivemas.servicio_servidor_emision;
using proyecto_vivemas.Util;
using proyecto_vivemas.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

using PdfSharp;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using PdfSharp.Pdf;

namespace proyecto_vivemas.Controllers
{
    /// <summary>
    /// Clase Controlador para los reportes
    /// contiene todos los metodos para trabajar con los reportes
    /// </summary>
    /// <remarks>
    /// esta clase puede mostrar vistas relacionadas con los reportes, puede mostrar reportes
    /// </remarks>
    public class ReportesController : Controller
    {
        /// <value>
        /// Objeto que conecta el modelo con el controlador
        /// </value>
        vivemas_dbEntities db = new vivemas_dbEntities();
        /// <value>
        /// JsonResult usado para devolver informacion a la vista
        /// </value>
        JsonResult respuesta;
        // GET: Reportes
        /// <summary>crea la instancia de la vista</summary>
        /// <returns>vista de reportes de estado cobranza</returns>
        public ActionResult ReporteEstadoCobranza()
        {
            return View();
        }

        /// <summary>crea la instancia de la vista</summary>
        /// <returns>vista de reportes de pagos por cliente</returns>
        public ActionResult ReporteClientesPagos()
        {
            return View();
        }

        public ActionResult ReporteTransacciones()
        {
            return View();
        }

        public ActionResult ReporteClientesMin()
        {
            return View();
        }

        public ActionResult RegistroEventos()
        {
            return View();
        }

        public ActionResult ReporteEventos()
        {
            return View();
        }

        public ActionResult ReporteVentas()
        {
            return View();
        }

        public ActionResult ReporteEventosDetalle()//mario
        {
            return View();
        }

        public ActionResult ReporteComprobantesElectronicos()
        {
            return View();
        }

        public ActionResult EmitirDocumentoElectronico(long transaccionId)
        {
            DocumentoVentaModelo modelo = GenerarDocumentoVenta(transaccionId);
            return View(modelo);
        }

        public ActionResult EmitirDocumentoElectronicoFechaEmision(long transaccionId, string fechaEmisionDet)
        {
            DocumentoVentaModelo modelo = GenerarDocumentoVentaFechaEmision(transaccionId, fechaEmisionDet);
            return View(modelo);
        }

        public ActionResult EmitirDocumentoElectronico2(long transaccionId)
        {
            JsonResult resultado = GenerarDocumentoVenta2(transaccionId);
            return resultado;
        }

        public ActionResult ReimprimirDocumentoElectronico(long documentoId)
        {
            try
            {
                long empresaId = long.Parse(Session["empresaId"].ToString());
                empresas empresa = db.empresas.Find(empresaId);
                documentosventa ventaCabecera = db.documentosventa.Find(documentoId);
                monedas moneda = db.monedas.Find(ventaCabecera.documentoventa_moneda_id);
                transacciones transaccion = db.transacciones.Find(ventaCabecera.documentoventa_transaccion_id);
                string transaccion_fechadeposito = "-";
                string transaccion_nrooperacion = "-";
                string transaccion_banco = "-";
                string transaccion_cuenta = "-";
                string clienteDireccion = "-";
                if (transaccion != null)
                {
                    bancos banco = db.bancos.Find(transaccion.transaccion_banco_id);
                    cuentasbanco cuenta = db.cuentasbanco.Find(transaccion.transaccion_cuentabanco_id);
                    transaccion_fechadeposito = transaccion.transaccion_fechadeposito.Value.ToString("dd/MM/yyyy");
                    transaccion_nrooperacion = transaccion.transaccion_nrooperacion;
                    transaccion_banco = banco == null ? "-" : banco.banco_descripcioncorta;
                    transaccion_cuenta = cuenta == null ? "-" : cuenta.cuentabanco_cuenta.Substring(cuenta.cuentabanco_cuenta.Length - 4, 4);
                    if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACIONES
                    {
                        transaccionesseparacion transaccionseparacion = db.transaccionesseparacion.Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id).FirstOrDefault();
                        separaciones separacion = db.separaciones.Find(transaccionseparacion.transaccionseparacion_separacion_id);

                        clientes cliente = db.clientes.Find(separacion.separacion_cliente_id);
                        clienteDireccion = cliente.cliente_direccion;
                    }
                    else
                    {
                        clienteDireccion = db.sp_obtenerDireccionCliente(transaccion.transaccion_id).FirstOrDefault();
                    }
                }
                else
                {
                    clientes cliente = db.clientes.Where(cli => cli.cliente_nrodocumento == ventaCabecera.documentoventa_cliente_nrodocumento).FirstOrDefault();
                    clienteDireccion = cliente.cliente_direccion;
                    transaccion_fechadeposito = ventaCabecera.documentoventa_fechadeposito == null ? "-" : ventaCabecera.documentoventa_fechadeposito.Value.ToString("dd/MM/yyyy");
                    transaccion_nrooperacion = ventaCabecera.documentoventa_nrooperacion == null ? "-" : ventaCabecera.documentoventa_nrooperacion;
                    transaccion_banco = ventaCabecera.documentoventa_banco_nombre == null ? "-" : ventaCabecera.documentoventa_banco_nombre;
                    transaccion_cuenta = ventaCabecera.documentoventa_cuenta_cuenta == null || ventaCabecera.documentoventa_cuenta_cuenta.Equals("-") ? "-" : ventaCabecera.documentoventa_cuenta_cuenta.Substring(ventaCabecera.documentoventa_cuenta_cuenta.Length - 4, 4);
                }
                string documento_descripcion = "";
                if (ventaCabecera.documentoventa_tipodocumentoventa_id == 1)//BOLETA
                {
                    documento_descripcion = "BOLETA DE VENTA ELECTRONICA";
                }
                else if (ventaCabecera.documentoventa_tipodocumentoventa_id == 2)//FACTURA
                {
                    documento_descripcion = "FACTURA DE VENTA ELECTRONICA";
                }
                DocumentoVentaModelo documentoModelo = new DocumentoVentaModelo
                {
                    documento_cliente_nombre = ventaCabecera.documentoventa_cliente_nombre,
                    documento_cliente_nroDocumento = ventaCabecera.documentoventa_cliente_nrodocumento,
                    documento_cliente_direccion = clienteDireccion,
                    documento_empresa_correo = empresa.empresa_correo,
                    documento_empresa_direccion = empresa.empresa_direccion + " - " + empresa.empresa_departamento + " - " + empresa.empresa_provincia + " - " + empresa.empresa_distrito,
                    documento_empresa_nombre = empresa.empresa_nombrecomercial,
                    documento_empresa_numeroContacto = empresa.empresa_nrocontacto,
                    documento_fechaEmision = ventaCabecera.documentoventa_fechaemision,
                    documento_fechaVencimiento = ventaCabecera.documentoventa_fechavencimiento,
                    documento_igv = ventaCabecera.documentoventa_igv.Value,
                    documento_serie = ventaCabecera.documentoventa_serie_serie + "-" + ventaCabecera.documentoventa_serie_numeracion,
                    documento_subtotal = ventaCabecera.documentoventa_subtotal.Value,
                    documento_total = ventaCabecera.documentoventa_total.Value,
                    documento_digestValue = ventaCabecera.documentoventa_digestvalue,
                    documento_descripcion = documento_descripcion,
                    documento_empresa_documento = empresa.empresa_documento,
                    documento_moneda_descripcion = moneda.moneda_descripcion,
                    documento_qrcodeValue = empresa.empresa_documento + "|" + empresa.tiposdocumentoidentidad.tipodocumentoidentidad_codigo + "|" + ventaCabecera.documentoventa_serie_serie + "|" + ventaCabecera.documentoventa_serie_numeracion + "|" + ventaCabecera.documentoventa_igv + "|" + ventaCabecera.documentoventa_total + "|" + ventaCabecera.documentoventa_fechaemision + "|" + ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo + "|" + ventaCabecera.documentoventa_cliente_nrodocumento,
                    documento_montoletras = ventaCabecera.documentoventa_totalletras,
                    moneda_simbolo = moneda.moneda_caracter,
                    transaccion_fechadeposito = transaccion_fechadeposito,
                    transaccion_nrooperacion = transaccion_nrooperacion,
                    transaccion_banco = transaccion_banco,
                    transaccion_cuenta = transaccion_cuenta
                };
                List<DocumentoDetalleModelo> detalleModelo = new List<DocumentoDetalleModelo>();
                List<documentosventadetalle> listaDetalleVenta = db.documentosventadetalle.Where(dvd => dvd.documentoventadetalle_documentoventa_id == ventaCabecera.documentoventa_id).ToList();

                foreach (var itemDetalle in listaDetalleVenta)
                {
                    DocumentoDetalleModelo detalle = new DocumentoDetalleModelo
                    {
                        documentoDetalle_cantidad = itemDetalle.documentoventadetalle_cantidad.Value,
                        documentoDetalle_codigo = itemDetalle.documentoventadetalle_codigo,
                        documentoDetalle_descripcion = itemDetalle.documentoventadetalle_descripcion,
                        documentoDetalle_total = itemDetalle.documentoventadetalle_total.Value,
                        documentoDetalle_unidad = "UNIDAD",
                        documentoDetalle_valorUnitario = itemDetalle.documentoventadetalle_subtotal.Value
                    };
                    detalleModelo.Add(detalle);
                }
                documentoModelo.detalleVenta = detalleModelo;
                return View(documentoModelo);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult imprimirNotaCredito(long documentoId)
        {
            try
            {
                long empresaId = long.Parse(Session["empresaId"].ToString());
                empresas empresa = db.empresas.Find(empresaId);
                notascredito ventaCabecera = db.notascredito.Find(documentoId);
                monedas moneda = db.monedas.Find(ventaCabecera.notacredito_moneda_id);
                documentosventa documento = db.documentosventa.Find(ventaCabecera.notacredito_documentoventa_id);
                transacciones transaccion = db.transacciones.Find(documento.documentoventa_transaccion_id);
                tiposnotacredito tipoNota = db.tiposnotacredito.Find(ventaCabecera.notacredito_tiponotacredito_id);
                string transaccion_fechadeposito = "-";
                string transaccion_nrooperacion = "-";
                string transaccion_banco = "-";
                string transaccion_cuenta = "-";
                string clienteDireccion = "-";
                if (transaccion != null)
                {
                    bancos banco = db.bancos.Find(transaccion.transaccion_banco_id);
                    cuentasbanco cuenta = db.cuentasbanco.Find(transaccion.transaccion_cuentabanco_id);
                    transaccion_fechadeposito = transaccion.transaccion_fechadeposito.Value.ToString("dd/MM/yyyy");
                    if (banco == null)
                    {

                    }
                    else
                    {
                        transaccion_banco = banco.banco_descripcioncorta;
                        transaccion_cuenta = cuenta.cuentabanco_cuenta.Substring(cuenta.cuentabanco_cuenta.Length - 4, 4);

                    }
                    transaccion_nrooperacion = transaccion.transaccion_nrooperacion;
                    //transaccion_banco = banco.banco_descripcioncorta;
                    //transaccion_cuenta = cuenta.cuentabanco_cuenta.Substring(cuenta.cuentabanco_cuenta.Length - 4, 4);
                    if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACIONES
                    {
                        transaccionesseparacion transaccionseparacion = db.transaccionesseparacion.Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id).FirstOrDefault();
                        separaciones separacion = db.separaciones.Find(transaccionseparacion.transaccionseparacion_separacion_id);

                        clientes cliente = db.clientes.Find(separacion.separacion_cliente_id);
                        clienteDireccion = cliente.cliente_direccion;
                    }
                    else
                    {
                        clienteDireccion = db.sp_obtenerDireccionCliente(transaccion.transaccion_id).FirstOrDefault();
                    }
                }
                else
                {
                    clientes cliente = db.clientes.Where(cli => cli.cliente_nrodocumento == ventaCabecera.notacredito_cliente_nrodocumento).FirstOrDefault();
                    clienteDireccion = cliente.cliente_direccion;
                }
                string documento_descripcion = "NOTA DE CREDITO ELECTRONICA";

                DocumentoVentaModelo documentoModelo = new DocumentoVentaModelo
                {
                    documento_cliente_nombre = ventaCabecera.notacredito_cliente_nombre,
                    documento_cliente_nroDocumento = ventaCabecera.notacredito_cliente_nrodocumento,
                    documento_cliente_direccion = clienteDireccion,
                    documento_empresa_correo = empresa.empresa_correo,
                    documento_empresa_direccion = empresa.empresa_direccion + " - " + empresa.empresa_departamento + " - " + empresa.empresa_provincia + " - " + empresa.empresa_distrito,
                    documento_empresa_nombre = empresa.empresa_nombrecomercial,
                    documento_empresa_numeroContacto = empresa.empresa_nrocontacto,
                    documento_fechaEmision = ventaCabecera.notacredito_fechaemision,
                    documento_igv = ventaCabecera.notacredito_igv.Value,
                    documento_serie = ventaCabecera.notacredito_serie_serie + "-" + ventaCabecera.notacredito_serie_numeracion,
                    documento_fechaemisionreferencia = documento.documentoventa_fechaemision,
                    documento_subtotal = ventaCabecera.notacredito_subtotal.Value,
                    documento_total = ventaCabecera.notacredito_total.Value,
                    documento_digestValue = ventaCabecera.notacredito_digestvalue,
                    documento_descripcion = documento_descripcion,
                    documento_empresa_documento = empresa.empresa_documento,
                    documento_moneda_descripcion = moneda.moneda_descripcion,
                    documento_qrcodeValue = empresa.empresa_documento + "|" + empresa.tiposdocumentoidentidad.tipodocumentoidentidad_codigo + "|" + ventaCabecera.notacredito_serie_serie + "|" + ventaCabecera.notacredito_serie_numeracion + "|" + ventaCabecera.notacredito_igv + "|" + ventaCabecera.notacredito_total + "|" + ventaCabecera.notacredito_fechaemision + "|" + ventaCabecera.notacredito_cliente_tipodocumentoidentidad_codigo + "|" + ventaCabecera.notacredito_cliente_nrodocumento,
                    documento_montoletras = ventaCabecera.notacredito_totalletras,
                    moneda_simbolo = moneda.moneda_caracter,
                    transaccion_fechadeposito = transaccion_fechadeposito,
                    transaccion_nrooperacion = transaccion_nrooperacion,
                    transaccion_banco = transaccion_banco,
                    transaccion_cuenta = transaccion_cuenta,
                    documento_documentoreferencia = documento.documentoventa_serie_serie + "-" + documento.documentoventa_serie_numeracion,
                    documento_tiponota = tipoNota.tiponotacredito_descripcion,
                    documento_motivo = ventaCabecera.notacredito_descripcionnota,

                };
                List<DocumentoDetalleModelo> detalleModelo = new List<DocumentoDetalleModelo>();
                List<notascreditodetalle> listaDetalleVenta = db.notascreditodetalle.Where(dvd => dvd.notacreditodetalle_notadecredito_id == ventaCabecera.notacredito_id).ToList();

                foreach (var itemDetalle in listaDetalleVenta)
                {
                    DocumentoDetalleModelo detalle = new DocumentoDetalleModelo
                    {
                        documentoDetalle_cantidad = decimal.Parse(itemDetalle.notacreditodetalle_cantidad),
                        documentoDetalle_codigo = itemDetalle.notacreditodetalle_codigo,
                        documentoDetalle_descripcion = itemDetalle.notacreditodetalle_descripcion,
                        documentoDetalle_total = itemDetalle.notacreditodetalle_total.Value,
                        documentoDetalle_unidad = "UNIDAD",
                        documentoDetalle_valorUnitario = itemDetalle.notacreditodetalle_subtotal.Value
                    };
                    detalleModelo.Add(detalle);
                }
                documentoModelo.detalleVenta = detalleModelo;
                return View(documentoModelo);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult obtenerReportePagosCliente(long idContrato, string fechaInicio, string fechaFin)
        {
            JsonResult resultado = new JsonResult();
            try
            {
                List<DataReportePagos> listaDataReportePagos = new List<DataReportePagos>();
                #region
                sp_obtenerReportePagosClienteCabecera_Result reporteCabecera = db.sp_obtenerReportePagosClienteCabecera(idContrato).FirstOrDefault();
                anexoscontratocotizacion anexo = db.anexoscontratocotizacion.Where(acc => acc.anexocontratocotizacion_contrato_id == idContrato).FirstOrDefault();
                List<cuotas> listaCuotas = db.cuotas.Where(cuo => cuo.cuota_anexocontratocotizacion_id == anexo.anexocontratocotizacion_id).ToList();
                foreach (cuotas cuota in listaCuotas)
                {
                    List<sp_obtenerReportePagosClienteDetalle_v3_Result> reporteDetalle2 = db.sp_obtenerReportePagosClienteDetalle_v3(cuota.cuota_id).ToList();
                    if (reporteDetalle2.Count != 0)
                    {
                        foreach (var itemReporteDetalle in reporteDetalle2)
                        {
                            DataReportePagos itemReporte = new DataReportePagos();
                            itemReporte.cuota_numeracion = cuota.cuota_numeracion;
                            itemReporte.cuota_fechavencimiento = cuota.cuota_fechavencimiento.Value.ToString("dd/MM/yyyy");
                            itemReporte.cuota_monto = cuota.cuota_monto;
                            itemReporte.transaccion_monto = itemReporteDetalle.pago_montonetopago;
                            itemReporte.pago_montodescuento = itemReporteDetalle.pago_montodescuento;
                            itemReporte.fecha_pago = itemReporteDetalle.transaccion_fechadeposito == null ? "" : itemReporteDetalle.transaccion_fechadeposito.Value.ToString("dd/MM/yyyy");
                            if (cuota.cuota_estado == false)
                            {
                                itemReporte.cuota_estado = "CANCELADO";
                            }
                            else
                            {
                                if (DateTime.Compare(cuota.cuota_fechavencimiento.Value, DateTime.Now) < 0)
                                {
                                    itemReporte.cuota_estado = "VENCIDO";
                                }
                                else
                                {
                                    itemReporte.cuota_estado = "";
                                }
                            }
                            listaDataReportePagos.Add(itemReporte);
                        }
                    }
                    else
                    {
                        DataReportePagos itemReporte = new DataReportePagos();
                        itemReporte.cuota_numeracion = cuota.cuota_numeracion;
                        itemReporte.cuota_fechavencimiento = cuota.cuota_fechavencimiento.Value.ToString("dd/MM/yyyy");
                        itemReporte.cuota_monto = cuota.cuota_monto;
                        itemReporte.transaccion_monto = 0;
                        itemReporte.pago_montodescuento = 0;
                        itemReporte.fecha_pago = "";
                        if (cuota.cuota_estado == false)
                        {
                            itemReporte.cuota_estado = "CANCELADO";
                        }
                        else
                        {
                            if (DateTime.Compare(cuota.cuota_fechavencimiento.Value, DateTime.Now) < 0)
                            {
                                itemReporte.cuota_estado = "VENCIDO";
                            }
                            else
                            {
                                itemReporte.cuota_estado = "";
                            }
                        }
                        listaDataReportePagos.Add(itemReporte);
                    }
                }
                #endregion
                resultado.Data = new
                {
                    flag = 1,
                    cabecera = reporteCabecera,
                    detalle = listaDataReportePagos
                };
                return resultado;
            }
            catch (Exception ex)
            {
                resultado.Data = new
                {
                    flag = 0
                };
                return resultado;
            }
        }

        public ActionResult obtenerReporteEstadoCobranza(long idProyecto)
        {
            JsonResult resultado = new JsonResult();
            try
            {
                resultado.Data = new
                {
                    flag = 1,
                    reporte = db.sp_obtenerReporteEstadoCobranza(idProyecto).ToList().Select(rpt => new
                    {
                        rpt.cotizacion_montoinicial,
                        anexocontratocotizacion_fechapagoefectivo = rpt.anexocontratocotizacion_fechapagoefectivo.Value.ToString("dd/MM/yyyy"),
                        rpt.cotizacion_preciototal,
                        rpt.cotizacion_promotor_nombre,
                        rpt.cuota_cuotaspagadas,
                        rpt.cuota_devolucion,
                        cuota_fechaultimopago = rpt.cuota_fechaultimopago.Value.ToString("dd/MM/yyyy"),
                        rpt.cuota_letrapendiente,
                        rpt.cuota_montopagado,
                        rpt.cuota_montopagadototal,
                        rpt.cuota_montopendiente,
                        rpt.cuota_porcentajepagado,
                        rpt.lote_nombre,
                        rpt.moneda_descripcioncorta,
                        rpt.proformauif_cliente_nrodocumento,
                        rpt.proformauif_cliente_razonsocial,
                        rpt.proyecto_nombrecorto
                    })
                };
            }
            catch (Exception ex)
            {
                resultado.Data = new
                {
                    flag = 0
                };
            }
            return resultado;
        }

        public ActionResult obtenerReporteTransacciones(DataReportes reporte)
        {
            JsonResult resultado = new JsonResult();
            try
            {
                DateTime fechaDesde = new DateTime();
                DateTime fechaHasta = new DateTime();
                if (reporte.fechaDesde != null)
                {
                    fechaDesde = DateTime.Parse(reporte.fechaDesde);
                }
                if (reporte.fechaHasta != null)
                {
                    fechaHasta = DateTime.Parse(reporte.fechaHasta);
                }
                List<sp_obtenerReporteTransacciones_Result> dataReporte = db.sp_obtenerReporteTransacciones()
                    .Where(rpt => (reporte.idTipoTransaccion == 0 || rpt.tipotransaccion_id == reporte.idTipoTransaccion) &&
                                 (reporte.idMetodoPago == 0 || rpt.tipometodopago_id == reporte.idMetodoPago) &&
                                 (reporte.idProyecto == 0 || rpt.proyecto_id == reporte.idProyecto) &&
                                 (reporte.idBancoDestino == 0 || rpt.banco_id == reporte.idBancoDestino) &&
                                 (reporte.idNroCuentaBancoDestino == 0 || rpt.cuentabanco_id == reporte.idNroCuentaBancoDestino) &&
                                 (reporte.fechaDesde == null || rpt.transaccion_fechadeposito >= fechaDesde) &&
                                 (reporte.fechaHasta == null || rpt.transaccion_fechadeposito <= fechaHasta))
                    .ToList();
                decimal montoTotal = (decimal)dataReporte.Sum(rpt => rpt.transaccion_monto);
                decimal montoTotalNeto = (decimal)dataReporte.Sum(rpt => rpt.transaccion_montonetototal);
                decimal montoTotalDescuento = (decimal)dataReporte.Sum(rpt => rpt.transaccion_montototaldescuento);
                resultado.Data = new
                {
                    flag = 1,
                    data = dataReporte.Select(rpt => new
                    {
                        rpt.transaccion_numeracion,
                        rpt.proformauif_cliente_nrodocumento,
                        rpt.proformauif_cliente_razonsocial,
                        rpt.proyecto_nombrecorto,
                        rpt.lote_nombre,
                        rpt.tipotransaccion_descripcion,
                        rpt.tipometodopago_descripcion,
                        transaccion_fechadeposito = rpt.transaccion_fechadeposito == null ? "-" : rpt.transaccion_fechadeposito.Value.ToString("dd/MM/yyyy"),
                        banco_descripcion = rpt.banco_id == null ? "-" : rpt.banco_descripcioncorta,
                        cuentabanco_cuenta = rpt.cuentabanco_id == null ? "-" : rpt.cuentabanco_cuenta,
                        rpt.moneda_descripcioncorta,
                        rpt.transaccion_monto,
                        rpt.transaccion_montonetototal,
                        rpt.transaccion_montototaldescuento
                    }),
                    montoTotal,
                    montoTotalNeto,
                    montoTotalDescuento
                };
                return resultado;
            }
            catch (Exception ex)
            {
                resultado.Data = new
                {
                    flag = 0,

                };
                return resultado;
            }

        }

        public ActionResult obtenerReporteVentas(DataReportes reporte)
        {
            try
            {
                respuesta = new JsonResult();
                if (reporte.idContrato == null)
                {
                    if (reporte.fechaDesde == null)
                    {
                        if (reporte.fechaHasta == null)
                        {
                            if (reporte.idProyecto == 0)//MOSTRAR TODOS
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision
                                    })
                                };

                            }
                            else if (reporte.idProyecto == 1)//SOLO CASUALINAS
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.documentoventa_transaccion_id == null).Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision
                                    })
                                };
                            }
                            else
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.cotizacion_proyecto_id == reporte.idProyecto).Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision

                                    })
                                };
                            }
                        }
                        else
                        {
                            DateTime fechaHasta = new DateTime();
                            fechaHasta = DateTime.Parse(reporte.fechaHasta);
                            if (reporte.idProyecto == 0)//MOSTRAR TODOS
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.documentoventa_fechacreacion <= fechaHasta).Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision
                                    })
                                };

                            }
                            else if (reporte.idProyecto == 1)// SOLO CASULINAS
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.documentoventa_transaccion_id == null && rpt.documentoventa_fechacreacion <= fechaHasta).Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision
                                    })
                                };
                            }
                            else
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.cotizacion_proyecto_id == reporte.idProyecto && rpt.documentoventa_fechacreacion <= fechaHasta).Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision
                                    })
                                };
                            }
                        }
                    }
                    else
                    {
                        DateTime fechaDesde = new DateTime();
                        fechaDesde = DateTime.Parse(reporte.fechaDesde);
                        if (reporte.fechaHasta == null)
                        {
                            if (reporte.idProyecto == 0)
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.documentoventa_fechacreacion >= fechaDesde).Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision
                                    })
                                };

                            }
                            else if (reporte.idProyecto == 1)
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.documentoventa_transaccion_id == null && rpt.documentoventa_fechacreacion >= fechaDesde).Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision
                                    })
                                };
                            }
                            else
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.cotizacion_proyecto_id == reporte.idProyecto && rpt.documentoventa_fechacreacion >= fechaDesde).Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision
                                    })
                                };
                            }
                        }
                        else
                        {
                            DateTime fechaHasta = new DateTime();
                            fechaHasta = DateTime.Parse(reporte.fechaHasta);
                            if (reporte.idProyecto == 0)
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.documentoventa_fechacreacion >= fechaDesde && rpt.documentoventa_fechacreacion <= fechaHasta).Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision
                                    })
                                };

                            }
                            else if (reporte.idProyecto == 1)
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.documentoventa_transaccion_id == null && rpt.documentoventa_fechacreacion >= fechaDesde && rpt.documentoventa_fechacreacion <= fechaHasta).Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision
                                    })
                                };
                            }
                            else
                            {
                                respuesta.Data = new
                                {
                                    flag = 1,
                                    dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.cotizacion_proyecto_id == reporte.idProyecto && rpt.documentoventa_fechacreacion >= fechaDesde && rpt.documentoventa_fechacreacion <= fechaHasta).Select(rpt => new
                                    {
                                        rpt.documentoEmision,
                                        rpt.notacreditoanulacion,
                                        rpt.documentoventa_cliente_nrodocumento,
                                        rpt.documentoventa_cliente_nombre,
                                        rpt.documentoventa_total,
                                        rpt.documentoventa_moneda_codigo,
                                        rpt.documentoventa_fechaemision,
                                        rpt.notacredito_fechaemision
                                    })
                                };
                            }
                        }
                    }
                    return respuesta;
                }
                else
                {
                    if (reporte.fechaDesde == null)
                    {
                        if (reporte.fechaHasta == null)
                        {
                            respuesta.Data = new
                            {
                                flag = 1,
                                dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.anexocontratocotizacion_contrato_id == reporte.idContrato).Select(rpt => new
                                {
                                    rpt.documentoEmision,
                                    rpt.notacreditoanulacion,
                                    rpt.documentoventa_cliente_nrodocumento,
                                    rpt.documentoventa_cliente_nombre,
                                    rpt.documentoventa_total,
                                    rpt.documentoventa_moneda_codigo,
                                    rpt.documentoventa_fechaemision,
                                    rpt.notacredito_fechaemision
                                })
                            };
                        }
                        else
                        {
                            DateTime fechaHasta = new DateTime();
                            fechaHasta = DateTime.Parse(reporte.fechaHasta);
                            respuesta.Data = new
                            {
                                flag = 1,
                                dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.anexocontratocotizacion_contrato_id == reporte.idContrato && rpt.documentoventa_fechacreacion <= fechaHasta).Select(rpt => new
                                {
                                    rpt.documentoEmision,
                                    rpt.notacreditoanulacion,
                                    rpt.documentoventa_cliente_nrodocumento,
                                    rpt.documentoventa_cliente_nombre,
                                    rpt.documentoventa_total,
                                    rpt.documentoventa_moneda_codigo,
                                    rpt.documentoventa_fechaemision,
                                    rpt.notacredito_fechaemision
                                })
                            };
                        }
                    }
                    else
                    {
                        DateTime fechaDesde = new DateTime();
                        fechaDesde = DateTime.Parse(reporte.fechaDesde);
                        if (reporte.fechaHasta == null)
                        {
                            respuesta.Data = new
                            {
                                flag = 1,
                                dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.anexocontratocotizacion_contrato_id == reporte.idContrato && rpt.documentoventa_fechacreacion >= fechaDesde).Select(rpt => new
                                {
                                    rpt.documentoEmision,
                                    rpt.notacreditoanulacion,
                                    rpt.documentoventa_cliente_nrodocumento,
                                    rpt.documentoventa_cliente_nombre,
                                    rpt.documentoventa_total,
                                    rpt.documentoventa_moneda_codigo,
                                    rpt.documentoventa_fechaemision,
                                    rpt.notacredito_fechaemision
                                })
                            };
                        }
                        else
                        {
                            DateTime fechaHasta = new DateTime();
                            fechaHasta = DateTime.Parse(reporte.fechaHasta);
                            respuesta.Data = new
                            {
                                flag = 1,
                                dataReporte = db.vw_reporteVentas.ToList().Where(rpt => rpt.anexocontratocotizacion_contrato_id == reporte.idContrato && rpt.documentoventa_fechacreacion >= fechaDesde && rpt.documentoventa_fechacreacion <= fechaHasta).Select(rpt => new
                                {
                                    rpt.documentoEmision,
                                    rpt.notacreditoanulacion,
                                    rpt.documentoventa_cliente_nrodocumento,
                                    rpt.documentoventa_cliente_nombre,
                                    rpt.documentoventa_total,
                                    rpt.documentoventa_moneda_codigo,
                                    rpt.documentoventa_fechaemision,
                                    rpt.notacredito_fechaemision
                                })
                            };
                        }
                    }
                    return respuesta;
                }
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

        public ActionResult obtenerReporteEventos(DataReportes reporte)
        {
            try
            {
                respuesta = new JsonResult();
                if (reporte.idProyecto == 0)
                {
                    respuesta.Data = new
                    {
                        flag = 1,
                        dataReporte = db.vw_reporteeventos.ToList().Select(rpt => new {
                            rpt.proformauif_cliente_nrodocumento,
                            rpt.proformauif_cliente_razonsocial,
                            rpt.proyecto_nombrecorto,
                            rpt.cotizacion_lote_nombre,
                            rpt.llamada,
                            rpt.whatsapp,
                            rpt.correo,
                            rpt.entrevista
                        })
                    };
                }
                else
                {
                    respuesta.Data = new
                    {
                        flag = 1,
                        dataReporte = db.vw_reporteeventos.Where(rpt => rpt.proyecto_id == reporte.idProyecto).ToList().Select(rpt => new {
                            rpt.proformauif_cliente_nrodocumento,
                            rpt.proformauif_cliente_razonsocial,
                            rpt.proyecto_nombrecorto,
                            rpt.cotizacion_lote_nombre,
                            rpt.llamada,
                            rpt.whatsapp,
                            rpt.correo,
                            rpt.entrevista
                        })
                    };
                }
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

        public ActionResult getClientes125(DataGetClientes data)
        {
            respuesta = new JsonResult();

            respuesta.Data = new
            {
                flag = 1,

                dataReporte = db.sp_get_clientes(data.documento + "%", 1).ToList().ToList().Select(rpt => new {
                    id = rpt.cliente_nrodocumento,
                    label = rpt.cliente_nrodocumento + "-" + rpt.cliente_razonsocial
                })
            };

            Console.Write(respuesta);

            return Json(respuesta);
        }

        public ActionResult getSeries()
        {
            try
            {
                return Json(db.sp_documento_series().ToList().Select(r => new { id = r.id, text = r.serie }));
            }
            catch (Exception e)
            {
                return null;
            }



        }
        public ActionResult getdDocumentosElectronicos(DataDocumentoElectronico data)
        {
            respuesta = new JsonResult();

            respuesta.Data = new
            {
                flag = 1,

                dataReporte = db.sp_get_documentos_electronicos(data.serie, data.fechaInicio, data.fechaFin).ToList()
            };

            Console.Write(respuesta);

            return Json(respuesta);
        }

        public ActionResult getdNotaCredito(DataDocumentoElectronico data)
        {
            respuesta = new JsonResult();

            respuesta.Data = new
            {
                flag = 1,

                dataReporte = db.sp_get_notas_credito(data.serie, data.fechaInicio, data.fechaFin).ToList()
            };

            Console.Write(respuesta);

            return Json(respuesta);
        }

        public ActionResult buscarClientesContrato(string term, string param1)
        {
            respuesta = new JsonResult();
            try
            {
                long proyectoId = long.Parse(param1);
                if (proyectoId == 0)
                {
                    return Json(db.vw_busquedaContratosAutocomplete
                    .Where(vw => vw.cliente_nrodocumento.Contains(term) || vw.cliente_razonsocial.Contains(term))
                    .Select(vw => new {
                        id = vw.contrato_id,
                        label = vw.cliente_nrodocumento + "-" + vw.cliente_razonsocial,
                        numeracion = vw.contrato_numeracion,
                        lote = vw.lote_nombre + "-" + vw.proyecto_nombrecorto
                    }).Take(10), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(db.vw_busquedaContratosAutocomplete
                    .Where(vw => (vw.cliente_nrodocumento.Contains(term) || vw.cliente_razonsocial.Contains(term)) && vw.lote_proyecto_id == proyectoId)
                    .Select(vw => new {
                        id = vw.contrato_id,
                        label = vw.cliente_nrodocumento + "-" + vw.cliente_razonsocial,
                        numeracion = vw.contrato_numeracion,
                        lote = vw.lote_nombre + "-" + vw.proyecto_nombrecorto
                    }).Take(10), JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public ActionResult obtenerReporteEventosDetalle(DataGetClientes data)//mario
        {
            try
            {

                respuesta = new JsonResult();
                if (data.documento != "")
                {
                    respuesta.Data = new
                    {
                        flag = 1,
                        dataReporte = db.sp_evento_detallado(data.documento, data.proyecto, data.lote).ToList().Select(rpt => new {
                            rpt.proyecto_nombrecorto,
                            rpt.cotizacion_lote_nombre,
                            rpt.proformauif_cliente_nrodocumento,
                            rpt.proformauif_cliente_razonsocial,
                            rpt.evento_fechacreacion,
                            rpt.evento_fechapropuesta,
                            rpt.evento_montopropuesto,
                            rpt.evento_descripcion,
                            rpt.estadoevento_descripcion,
                            rpt.tipocontacto_descripcion
                        })
                    };
                }
                else
                {
                    //---
                }
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


        private JsonResult GenerarDocumentoVenta2(long transaccionId)
        {
            JsonResult resultado = new JsonResult();
            try
            {
                List<DocumentoDetalleModelo> detalleModelo = new List<DocumentoDetalleModelo>();
                using (TransactionScope transaction = new TransactionScope())
                {
                    long empresaId = long.Parse(Session["empresaId"].ToString());
                    long usuarioId = long.Parse(Session["usuario"].ToString());
                    string codigoPago = "";
                    empresas empresa = db.empresas.Find(empresaId);
                    transacciones transaccion = db.transacciones.Find(transaccionId);
                    documentosventa ventaCabecera = new documentosventa();
                    ventaCabecera.documentoventa_transaccion_id = transaccion.transaccion_id;
                    ventaCabecera.documentoventa_estadodocumento_id = 2;//PENDIENTE DE ENVIO
                    //DATOS DE LA EMPRESA
                    ventaCabecera.documentoventa_empresa_id = empresa.empresa_id;
                    ventaCabecera.documentoventa_empresa_nrodocumento = empresa.empresa_documento;
                    ventaCabecera.documentoventa_empresa_razonsocial = empresa.empresa_nombre;
                    ventaCabecera.documentoventa_empresa_tipodocumentoidentidad_codigo = empresa.tiposdocumentoidentidad.tipodocumentoidentidad_codigo;
                    ventaCabecera.documentoventa_empresa_ubigeo = empresa.empresa_ubigeo;
                    ventaCabecera.documentoventa_empresa_direccion = empresa.empresa_direccion;
                    ventaCabecera.documentoventa_empresa_zona = empresa.empresa_zona;
                    ventaCabecera.documentoventa_empresa_distrito = empresa.empresa_distrito;
                    ventaCabecera.documentoventa_empresa_provincia = empresa.empresa_provincia;
                    ventaCabecera.documentoventa_empresa_departamento = empresa.empresa_departamento;
                    ventaCabecera.documentoventa_empresa_codigopais = empresa.empresa_codigopais;
                    //FIN DATOS DE LA EMPRESA
                    //DATOS DE CLIENTE
                    if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACION
                    {
                        transaccionesseparacion transaccionSeparacion = db.transaccionesseparacion
                            .Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id)
                            .FirstOrDefault();
                        separaciones separacion = db.separaciones.Find(transaccionSeparacion.transaccionseparacion_separacion_id);
                        clientes datosCliente = db.clientes.Find(separacion.separacion_cliente_id);
                        ventaCabecera.documentoventa_cliente_nombre = datosCliente.cliente_razonsocial;
                        ventaCabecera.documentoventa_cliente_nrodocumento = datosCliente.cliente_nrodocumento;
                        ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo = datosCliente.tiposdocumentoidentidad.tipodocumentoidentidad_codigo;
                        codigoPago = "SEP";
                    }
                    else if (transaccion.transaccion_tipotransaccion_id == 3 || transaccion.transaccion_tipotransaccion_id == 4)//PAGOS CASH O PAGO CUOTA
                    {
                        sp_obtenerDatosCliente_Result datosCliente = db.sp_obtenerDatosCliente(transaccionId).FirstOrDefault();
                        ventaCabecera.documentoventa_cliente_nombre = datosCliente.cliente_nombre;
                        ventaCabecera.documentoventa_cliente_nrodocumento = datosCliente.cliente_nrodocumento;
                        ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo = datosCliente.cliente_tipodocumentoIdentidad_codigo;
                        codigoPago = datosCliente.cotizacion_lote_nombre;
                    }
                    //FIN DATOS CLIENTE
                    //DATOS DEL DOCUMENTO ELECTRONICO
                    monedas moneda = db.monedas.Find(transaccion.transaccion_moneda_id);
                    tiposdocumentoventa tipoDocumentoVenta;
                    series serie;
                    DateTime fechaEmision = DateTime.Now;
                    if (ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo.Equals("6")) //RUC - FACTURA
                    {
                        tipoDocumentoVenta = db.tiposdocumentoventa.Find(2);//FACTURA
                        serie = db.series.Find(1);
                    }
                    else // BOLETAS PARA CUALQUIER OTRO TIPO DE DOCUMENTO
                    {
                        tipoDocumentoVenta = db.tiposdocumentoventa.Find(1);//BOLETA
                        serie = db.series.Find(2);
                    }
                    ventaCabecera.documentoventa_tipodocumentoventa_id = tipoDocumentoVenta.tipodocumentoventa_id;
                    ventaCabecera.documentoventa_tipodocumentoventa_codigo = tipoDocumentoVenta.tipodocumentoventa_codigosunat;
                    ventaCabecera.documentoventa_serie_serie = serie.serie_serie;
                    ventaCabecera.documentoventa_serie_numeracion = serie.serie_numeracion.Value.ToString("D8");
                    ventaCabecera.documentoventa_moneda_id = moneda.moneda_id;
                    ventaCabecera.documentoventa_moneda_codigo = moneda.moneda_descripcioncorta;
                    ventaCabecera.documentoventa_tipoemision = "1";
                    ventaCabecera.documentoventa_tipometodopago_id = transaccion.transaccion_tipometodopago_id;
                    ventaCabecera.documentoventa_fechaemision = fechaEmision.ToString("yyyy-MM-dd");
                    ventaCabecera.documentoventa_horaemision = fechaEmision.ToString("hh:mm:ss");
                    ventaCabecera.documentoventa_fechavencimiento = fechaEmision.ToString("yyyy-MM-dd");
                    ventaCabecera.documentoventa_subtotal = transaccion.transaccion_monto;
                    ventaCabecera.documentoventa_igv = 0;
                    ventaCabecera.documentoventa_total = transaccion.transaccion_monto;
                    ventaCabecera.documentoventa_condicioncodigo = "ANT";
                    if (moneda.moneda_descripcioncorta.Equals("PEN"))
                    {
                        ventaCabecera.documentoventa_totalletras = Utilities.ToString(transaccion.transaccion_monto.ToString()).ToUpper();
                    }
                    else
                    {
                        ventaCabecera.documentoventa_totalletras = Utilities.ToStringDolares(transaccion.transaccion_monto.ToString()).ToUpper();
                    }
                    ventaCabecera.documentoventa_fechacreacion = fechaEmision;
                    ventaCabecera.documentoventa_usuariocreacion = usuarioId;
                    serie.serie_numeracion = serie.serie_numeracion + 1;
                    db.Entry(serie).State = EntityState.Modified;
                    db.SaveChanges();
                    //FIN DATOS DEL DOCUMENTO ELECTRONICO (FALTAN LOS DATOS DE LA SUNAAAT NO  OLVIDAR)
                    db.documentosventa.Add(ventaCabecera);
                    db.SaveChanges();
                    //DETALLE DE LA VENTA
                    List<documentosventadetalle> listaDetalleVenta = new List<documentosventadetalle>();
                    if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACION
                    {
                        transaccionesseparacion transaccionSeparacion = db.transaccionesseparacion
                            .Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id)
                            .FirstOrDefault();
                        documentosventadetalle detalleVenta = new documentosventadetalle();
                        detalleVenta.documentoventadetalle_documentoventa_id = ventaCabecera.documentoventa_id;
                        detalleVenta.documentoventadetalle_descripcion = transaccionSeparacion.transaccionseparacion_descripcion;
                        detalleVenta.documentoventadetalle_unidadmedida_codigo = "NIU"; //CODIGO PARA UNIDAD NO MOVER
                        detalleVenta.documentoventadetalle_codigo = codigoPago;
                        detalleVenta.documentoventadetalle_cantidad = 1;
                        detalleVenta.documentoventadetalle_tipoventasunat = "02";
                        detalleVenta.documentoventadetalle_tipoafectacion = "30"; //INAFECTA
                        detalleVenta.documentoventadetalle_subtotal = transaccion.transaccion_monto;
                        detalleVenta.documentoventadetalle_igv = 0;
                        detalleVenta.documentoventadetalle_total = transaccion.transaccion_monto;
                        detalleVenta.documentoventadetalle_fechacreacion = fechaEmision;
                        detalleVenta.documentoventadetalle_usuariocreacion = usuarioId;
                        listaDetalleVenta.Add(detalleVenta);
                    }
                    else if (transaccion.transaccion_tipotransaccion_id == 3 || transaccion.transaccion_tipotransaccion_id == 4)//PAGOS CASH O PAGO CUOTA
                    {
                        List<pagos> listaPagos = db.pagos.Where(pag => pag.pago_transaccion_id == transaccion.transaccion_id).ToList();
                        foreach (pagos itemPago in listaPagos)
                        {
                            documentosventadetalle detalleVenta = new documentosventadetalle();
                            detalleVenta.documentoventadetalle_documentoventa_id = ventaCabecera.documentoventa_id;
                            detalleVenta.documentoventadetalle_descripcion = itemPago.pago_descripcion;
                            detalleVenta.documentoventadetalle_unidadmedida_codigo = "NIU"; //CODIGO PARA UNIDAD NO MOVER
                            detalleVenta.documentoventadetalle_codigo = codigoPago;
                            detalleVenta.documentoventadetalle_cantidad = 1;
                            detalleVenta.documentoventadetalle_tipoventasunat = "02";
                            detalleVenta.documentoventadetalle_tipoafectacion = "30"; //INAFECTA
                            detalleVenta.documentoventadetalle_subtotal = itemPago.pago_cuota_monto;
                            detalleVenta.documentoventadetalle_igv = 0;
                            detalleVenta.documentoventadetalle_total = itemPago.pago_cuota_monto;
                            detalleVenta.documentoventadetalle_fechacreacion = fechaEmision;
                            detalleVenta.documentoventadetalle_usuariocreacion = usuarioId;
                            listaDetalleVenta.Add(detalleVenta);
                        }
                    }
                    db.documentosventadetalle.AddRange(listaDetalleVenta);
                    db.SaveChanges();
                    configuraciones configuracion = db.configuraciones.Where(con => con.configuracion_empresa_id == empresaId).FirstOrDefault();
                    //FIN DETALLE DE LA VENTA

                    #region Facturacion electronica

                    string rutaDocumento = "";
                    string rutaCdrDocumento = "";

                    if (ventaCabecera.documentoventa_tipodocumentoventa_id == 1)//BOLETA
                    {
                        rutaDocumento = configuracion.configuracion_rutaboleta;
                        rutaCdrDocumento = configuracion.configuracion_rutacdrboleta;
                    }
                    else if (ventaCabecera.documentoventa_tipodocumentoventa_id == 2)//FACTURA
                    {
                        rutaDocumento = configuracion.configuracion_rutafactura;
                        rutaCdrDocumento = configuracion.configuracion_rutacdrfactura;
                    }
                    JObject documento = new JObject(
                            new JProperty("rutaDocumento", rutaDocumento.Replace("/", "\\")),
                            new JProperty("rutaCdrDocumento", rutaCdrDocumento.Replace("/", "\\")),
                            new JProperty("rutaFirmaElectronica", configuracion.configuracion_rutafirmadigital.Replace("/", "\\")),
                            new JProperty("passFirmaElectronica", configuracion.configuracion_passfirmadigital),
                            new JProperty("credencialesFirmaElectronica", configuracion.configuracion_credencialfirmadigital),
                            new JProperty("passCredencialFimaElectronica", configuracion.configuracion_passcredencialfirmadigital),
                            new JProperty("nombreFirmaElectronica", "signatureVIVEINCO"),
                            new JProperty("usuarioEmisor", configuracion.configuracion_usuarioemisor),
                            new JProperty("passEmisor", configuracion.configuracion_passemisor),
                            new JProperty("tipoEmision", ventaCabecera.documentoventa_tipoemision),//1 Emision sunat , 2 Emision OSE, 3 Emision Pruebas
                            new JProperty("dccTotalGravado", ventaCabecera.documentoventa_igv.ToString()),
                            new JProperty("dccFechaEmision", ventaCabecera.documentoventa_fechaemision),
                            new JProperty("dccHoraEmision", ventaCabecera.documentoventa_horaemision),
                            new JProperty("dccFechaVencimiento", ventaCabecera.documentoventa_fechavencimiento),
                            new JProperty("empRazonSocial", empresa.empresa_nombre),
                            new JProperty("empRuc", empresa.empresa_documento),
                            new JProperty("empTipoEntidad", ventaCabecera.documentoventa_empresa_tipodocumentoidentidad_codigo),
                            new JProperty("ubiCodigo", empresa.empresa_ubigeo),
                            new JProperty("empDireccion", empresa.empresa_direccion),
                            new JProperty("empZona", empresa.empresa_zona),
                            new JProperty("empDistrito", empresa.empresa_distrito),
                            new JProperty("empProvincia", empresa.empresa_provincia),
                            new JProperty("empDepartamento", empresa.empresa_departamento),
                            new JProperty("empCodigoPais", empresa.empresa_codigopais),
                            new JProperty("tdcCodigo", ventaCabecera.documentoventa_tipodocumentoventa_codigo),
                            new JProperty("dccSerie", ventaCabecera.documentoventa_serie_serie),
                            new JProperty("dccNumero", ventaCabecera.documentoventa_serie_numeracion),
                            new JProperty("entNombre", ventaCabecera.documentoventa_cliente_nombre),
                            new JProperty("entDocumento", ventaCabecera.documentoventa_cliente_nrodocumento),
                            new JProperty("tdeCodigo", ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo),
                            new JProperty("monCodigo", ventaCabecera.documentoventa_moneda_codigo),
                            new JProperty("dccTotalIgv", ventaCabecera.documentoventa_igv),
                            new JProperty("dccTotalVenta", ventaCabecera.documentoventa_total),
                            new JProperty("dccCondicion", "CON"),//CONTADO
                                                                 //new JProperty("dccGuiaderemision", ""),
                            new JProperty("dccTotalVentaLetras", ventaCabecera.documentoventa_totalletras),
                            new JProperty("ITEMS",
                            new JArray(
                                from itemdetalle in listaDetalleVenta
                                select new JObject(
                                    //new JProperty("proId", itemdetalle.ventadetalle_producto_id),
                                    new JProperty("proNombre", itemdetalle.documentoventadetalle_descripcion),
                                    new JProperty("uniCodigo", itemdetalle.documentoventadetalle_unidadmedida_codigo),
                                    new JProperty("dcdCantidad", itemdetalle.documentoventadetalle_cantidad.ToString()),
                                    new JProperty("dcdVentaBruto", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dcdVenta", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dcdPrecioUnitario", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dccPrecioVariable", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dcdValorDesc", "0.00"),
                                    new JProperty("tipCodigo", "01"),
                                    new JProperty("dcdAfectacion", itemdetalle.documentoventadetalle_igv.ToString()),
                                    new JProperty("tpaCodigo", itemdetalle.documentoventadetalle_tipoafectacion)
                                    //new JProperty("dcdValorUnitario", itemdetalle.ventadetalle_subtotal.Value.ToString("F"))
                                    //new JProperty("dcdImporteTotal", itemdetalle.ventadetalle_total.Value.ToString("F"))
                                    )))
                        );
                    string documentoventa = documento.ToString();
                    generarFactura2RequestBody generarfacturaRB = new generarFactura2RequestBody();
                    generarfacturaRB.documento = documentoventa;
                    generarFactura2Request generarfacturaR = new generarFactura2Request();
                    generarfacturaR.Body = generarfacturaRB;
                    wsemision wsemision = new wsemisionClient();
                    generarFactura2Response generarfacturaRSP = new generarFactura2Response();
                    generarfacturaRSP = wsemision.generarFactura2(generarfacturaR);
                    string respuesta = generarfacturaRSP.Body.@return;
                    JObject jsonrespuesta = JObject.Parse(respuesta);

                    #endregion



                    int status = (int)jsonrespuesta["status"];
                    if (status == 1)
                    {


                        int responseCode = (int)jsonrespuesta["responseCode"];
                        string digestValue = (string)jsonrespuesta["digestValue"];
                        if (responseCode != 0 || digestValue.Equals(""))
                        {
                            ventaCabecera.documentoventa_estadodocumento_id = 3;//PENDIENTE DE VERIFICACION
                            ventaCabecera.documentoventa_estadosunat = responseCode.ToString();
                            db.Entry(ventaCabecera).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        else
                        {
                            ventaCabecera.documentoventa_digestvalue = digestValue;
                            ventaCabecera.documentoventa_estadodocumento_id = 1;//CORRECTO SUNAT
                            ventaCabecera.documentoventa_estadosunat = responseCode.ToString();
                            db.Entry(ventaCabecera).State = EntityState.Modified;
                            db.SaveChanges();
                            resultado.Data = new
                            {
                                flag = 1
                            };
                        }
                    }
                    else
                    {
                        ventaCabecera.documentoventa_estadodocumento_id = 4;// CON ERRORES
                        ventaCabecera.documentoventa_estadosunat = (string)jsonrespuesta["error"];
                        db.Entry(ventaCabecera).State = EntityState.Modified;
                        db.SaveChanges();
                        resultado.Data = new
                        {
                            flag = 4
                        };
                    }
                    transaction.Complete();
                }
                //FIN CARGADO
                return resultado;
            }
            catch (Exception ex)
            {
                resultado = new JsonResult();
                resultado.Data = new
                {
                    flag = 0
                };
                return resultado;
            }
        }

        private DocumentoVentaModelo GenerarDocumentoVenta(long transaccionId)
        {
            try
            {
                DocumentoVentaModelo documentoModelo;
                List<DocumentoDetalleModelo> detalleModelo = new List<DocumentoDetalleModelo>();
                using (TransactionScope transaction = new TransactionScope())
                {
                    long empresaId = long.Parse(Session["empresaId"].ToString());
                    long usuarioId = long.Parse(Session["usuario"].ToString());
                    string codigoPago = "";
                    empresas empresa = db.empresas.Find(empresaId);
                    transacciones transaccion = db.transacciones.Find(transaccionId);
                    documentosventa ventaCabecera = new documentosventa();
                    ventaCabecera.documentoventa_transaccion_id = transaccion.transaccion_id;
                    ventaCabecera.documentoventa_estadodocumento_id = 2;//PENDIENTE DE ENVIO
                    //DATOS DE LA EMPRESA
                    ventaCabecera.documentoventa_empresa_id = empresa.empresa_id;
                    ventaCabecera.documentoventa_empresa_nrodocumento = empresa.empresa_documento;
                    ventaCabecera.documentoventa_empresa_razonsocial = empresa.empresa_nombre;
                    ventaCabecera.documentoventa_empresa_tipodocumentoidentidad_codigo = empresa.tiposdocumentoidentidad.tipodocumentoidentidad_codigo;
                    ventaCabecera.documentoventa_empresa_ubigeo = empresa.empresa_ubigeo;
                    ventaCabecera.documentoventa_empresa_direccion = empresa.empresa_direccion;
                    ventaCabecera.documentoventa_empresa_zona = empresa.empresa_zona;
                    ventaCabecera.documentoventa_empresa_distrito = empresa.empresa_distrito;
                    ventaCabecera.documentoventa_empresa_provincia = empresa.empresa_provincia;
                    ventaCabecera.documentoventa_empresa_departamento = empresa.empresa_departamento;
                    ventaCabecera.documentoventa_empresa_codigopais = empresa.empresa_codigopais;
                    //FIN DATOS DE LA EMPRESA
                    //DATOS DE CLIENTE
                    if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACION
                    {
                        transaccionesseparacion transaccionSeparacion = db.transaccionesseparacion
                            .Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id)
                            .FirstOrDefault();
                        separaciones separacion = db.separaciones.Find(transaccionSeparacion.transaccionseparacion_separacion_id);
                        clientes datosCliente = db.clientes.Find(separacion.separacion_cliente_id);
                        ventaCabecera.documentoventa_cliente_nombre = datosCliente.cliente_razonsocial;
                        ventaCabecera.documentoventa_cliente_nrodocumento = datosCliente.cliente_nrodocumento;
                        ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo = datosCliente.tiposdocumentoidentidad.tipodocumentoidentidad_codigo;
                        codigoPago = "SEP";
                    }
                    else if (transaccion.transaccion_tipotransaccion_id == 3 || transaccion.transaccion_tipotransaccion_id == 4)//PAGOS CASH O PAGO CUOTA
                    {
                        sp_obtenerDatosCliente_Result datosCliente = db.sp_obtenerDatosCliente(transaccionId).FirstOrDefault();
                        ventaCabecera.documentoventa_cliente_nombre = datosCliente.cliente_nombre;
                        ventaCabecera.documentoventa_cliente_nrodocumento = datosCliente.cliente_nrodocumento;
                        ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo = datosCliente.cliente_tipodocumentoIdentidad_codigo;
                        codigoPago = datosCliente.cotizacion_lote_nombre;
                    }
                    //FIN DATOS CLIENTE
                    //DATOS DEL DOCUMENTO ELECTRONICO
                    monedas moneda = db.monedas.Find(transaccion.transaccion_moneda_id);
                    tiposdocumentoventa tipoDocumentoVenta;
                    series serie;
                    DateTime fechaEmision = DateTime.Now;
                    if (ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo.Equals("6")) //RUC - FACTURA
                    {
                        tipoDocumentoVenta = db.tiposdocumentoventa.Find(2);//FACTURA
                        serie = db.series.Find(1);
                    }
                    else // BOLETAS PARA CUALQUIER OTRO TIPO DE DOCUMENTO
                    {
                        tipoDocumentoVenta = db.tiposdocumentoventa.Find(1);//BOLETA
                        serie = db.series.Find(2);
                    }
                    ventaCabecera.documentoventa_tipodocumentoventa_id = tipoDocumentoVenta.tipodocumentoventa_id;
                    ventaCabecera.documentoventa_tipodocumentoventa_codigo = tipoDocumentoVenta.tipodocumentoventa_codigosunat;
                    ventaCabecera.documentoventa_serie_serie = serie.serie_serie;
                    ventaCabecera.documentoventa_serie_numeracion = serie.serie_numeracion.Value.ToString("D8");
                    ventaCabecera.documentoventa_moneda_id = moneda.moneda_id;
                    ventaCabecera.documentoventa_moneda_codigo = moneda.moneda_descripcioncorta;
                    ventaCabecera.documentoventa_tipoemision = "1";//SERVIDOR REAL
                    ventaCabecera.documentoventa_tipometodopago_id = transaccion.transaccion_tipometodopago_id;
                    ventaCabecera.documentoventa_fechaemision = fechaEmision.ToString("yyyy-MM-dd");
                    ventaCabecera.documentoventa_horaemision = fechaEmision.ToString("hh:mm:ss");
                    ventaCabecera.documentoventa_fechavencimiento = fechaEmision.ToString("yyyy-MM-dd");
                    ventaCabecera.documentoventa_subtotal = transaccion.transaccion_montonetototal;
                    ventaCabecera.documentoventa_igv = 0;
                    ventaCabecera.documentoventa_total = transaccion.transaccion_montonetototal;
                    ventaCabecera.documentoventa_condicioncodigo = "ANT";
                    if (moneda.moneda_descripcioncorta.Equals("PEN"))
                    {
                        ventaCabecera.documentoventa_totalletras = Utilities.ToString(transaccion.transaccion_montonetototal.ToString()).ToUpper();
                    }
                    else
                    {
                        ventaCabecera.documentoventa_totalletras = Utilities.ToStringDolares(transaccion.transaccion_montonetototal.ToString()).ToUpper();
                    }
                    ventaCabecera.documentoventa_fechacreacion = fechaEmision;
                    ventaCabecera.documentoventa_usuariocreacion = usuarioId;
                    serie.serie_numeracion = serie.serie_numeracion + 1;
                    db.Entry(serie).State = EntityState.Modified;
                    db.SaveChanges();
                    //FIN DATOS DEL DOCUMENTO ELECTRONICO (FALTAN LOS DATOS DE LA SUNAAAT NO  OLVIDAR)
                    db.documentosventa.Add(ventaCabecera);
                    db.SaveChanges();
                    //DETALLE DE LA VENTA
                    List<documentosventadetalle> listaDetalleVenta = new List<documentosventadetalle>();
                    if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACION
                    {
                        transaccionesseparacion transaccionSeparacion = db.transaccionesseparacion
                            .Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id)
                            .FirstOrDefault();
                        documentosventadetalle detalleVenta = new documentosventadetalle();
                        detalleVenta.documentoventadetalle_documentoventa_id = ventaCabecera.documentoventa_id;
                        detalleVenta.documentoventadetalle_descripcion = transaccionSeparacion.transaccionseparacion_descripcion + "- PAGO ANTICIPADO";
                        detalleVenta.documentoventadetalle_unidadmedida_codigo = "NIU"; //CODIGO PARA UNIDAD NO MOVER
                        detalleVenta.documentoventadetalle_codigo = codigoPago;
                        detalleVenta.documentoventadetalle_cantidad = 1;
                        detalleVenta.documentoventadetalle_tipoventasunat = "02";
                        detalleVenta.documentoventadetalle_tipoafectacion = "30"; //INAFECTA
                        detalleVenta.documentoventadetalle_subtotal = transaccion.transaccion_montonetototal;
                        detalleVenta.documentoventadetalle_igv = 0;
                        detalleVenta.documentoventadetalle_total = transaccion.transaccion_montonetototal;
                        detalleVenta.documentoventadetalle_fechacreacion = fechaEmision;
                        detalleVenta.documentoventadetalle_usuariocreacion = usuarioId;
                        listaDetalleVenta.Add(detalleVenta);
                    }
                    else if (transaccion.transaccion_tipotransaccion_id == 3 || transaccion.transaccion_tipotransaccion_id == 4)//PAGOS CASH O PAGO CUOTA
                    {
                        List<pagos> listaPagos = db.pagos.Where(pag => pag.pago_transaccion_id == transaccion.transaccion_id).ToList();
                        foreach (pagos itemPago in listaPagos)
                        {
                            documentosventadetalle detalleVenta = new documentosventadetalle();
                            detalleVenta.documentoventadetalle_documentoventa_id = ventaCabecera.documentoventa_id;
                            detalleVenta.documentoventadetalle_descripcion = itemPago.pago_descripcion + " - PAGO ANTICIPADO";
                            detalleVenta.documentoventadetalle_unidadmedida_codigo = "NIU"; //CODIGO PARA UNIDAD NO MOVER
                            detalleVenta.documentoventadetalle_codigo = codigoPago;
                            detalleVenta.documentoventadetalle_cantidad = 1;
                            detalleVenta.documentoventadetalle_tipoventasunat = "02";
                            detalleVenta.documentoventadetalle_tipoafectacion = "30"; //INAFECTA
                            detalleVenta.documentoventadetalle_subtotal = itemPago.pago_montonetopago;
                            detalleVenta.documentoventadetalle_igv = 0;
                            detalleVenta.documentoventadetalle_total = itemPago.pago_montonetopago;
                            detalleVenta.documentoventadetalle_fechacreacion = fechaEmision;
                            detalleVenta.documentoventadetalle_usuariocreacion = usuarioId;
                            listaDetalleVenta.Add(detalleVenta);
                        }
                    }
                    db.documentosventadetalle.AddRange(listaDetalleVenta);
                    db.SaveChanges();
                    configuraciones configuracion = db.configuraciones.Where(con => con.configuracion_empresa_id == empresaId).FirstOrDefault();
                    //FIN DETALLE DE LA VENTA

                    #region Facturacion electronica

                    string rutaDocumento = "";
                    string rutaCdrDocumento = "";

                    if (ventaCabecera.documentoventa_tipodocumentoventa_id == 1)//BOLETA
                    {
                        rutaDocumento = configuracion.configuracion_rutaboleta;
                        rutaCdrDocumento = configuracion.configuracion_rutacdrboleta;
                    }
                    else if (ventaCabecera.documentoventa_tipodocumentoventa_id == 2)//FACTURA
                    {
                        rutaDocumento = configuracion.configuracion_rutafactura;
                        rutaCdrDocumento = configuracion.configuracion_rutacdrfactura;
                    }
                    JObject documento = new JObject(
                            new JProperty("rutaDocumento", rutaDocumento.Replace("/", "\\")),
                            new JProperty("rutaCdrDocumento", rutaCdrDocumento.Replace("/", "\\")),
                            new JProperty("rutaFirmaElectronica", configuracion.configuracion_rutafirmadigital.Replace("/", "\\")),
                            new JProperty("passFirmaElectronica", configuracion.configuracion_passfirmadigital),
                            new JProperty("credencialesFirmaElectronica", configuracion.configuracion_credencialfirmadigital),
                            new JProperty("passCredencialFimaElectronica", configuracion.configuracion_passcredencialfirmadigital),
                            new JProperty("nombreFirmaElectronica", "signatureVIVEINCO"),
                            new JProperty("usuarioEmisor", configuracion.configuracion_usuarioemisor),
                            new JProperty("passEmisor", configuracion.configuracion_passemisor),
                            new JProperty("tipoEmision", ventaCabecera.documentoventa_tipoemision),//1 Emision sunat , 2 Emision OSE, 3 Emision Pruebas
                            new JProperty("dccTotalGravado", ventaCabecera.documentoventa_igv.ToString()),
                            new JProperty("dccFechaEmision", ventaCabecera.documentoventa_fechaemision),
                            new JProperty("dccHoraEmision", ventaCabecera.documentoventa_horaemision),
                            new JProperty("dccFechaVencimiento", ventaCabecera.documentoventa_fechavencimiento),
                            new JProperty("empRazonSocial", empresa.empresa_nombre),
                            new JProperty("empRuc", empresa.empresa_documento),
                            new JProperty("empTipoEntidad", ventaCabecera.documentoventa_empresa_tipodocumentoidentidad_codigo),
                            new JProperty("ubiCodigo", empresa.empresa_ubigeo),
                            new JProperty("empDireccion", empresa.empresa_direccion),
                            new JProperty("empZona", empresa.empresa_zona),
                            new JProperty("empDistrito", empresa.empresa_distrito),
                            new JProperty("empProvincia", empresa.empresa_provincia),
                            new JProperty("empDepartamento", empresa.empresa_departamento),
                            new JProperty("empCodigoPais", empresa.empresa_codigopais),
                            new JProperty("tdcCodigo", ventaCabecera.documentoventa_tipodocumentoventa_codigo),
                            new JProperty("dccSerie", ventaCabecera.documentoventa_serie_serie),
                            new JProperty("dccNumero", ventaCabecera.documentoventa_serie_numeracion),
                            new JProperty("entNombre", ventaCabecera.documentoventa_cliente_nombre),
                            new JProperty("entDocumento", ventaCabecera.documentoventa_cliente_nrodocumento),
                            new JProperty("tdeCodigo", ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo),
                            new JProperty("monCodigo", ventaCabecera.documentoventa_moneda_codigo),
                            new JProperty("dccTotalIgv", ventaCabecera.documentoventa_igv),
                            new JProperty("dccTotalVenta", ventaCabecera.documentoventa_total),
                            new JProperty("dccCondicion", "CON"),//CONTADO
                                                                 //new JProperty("dccGuiaderemision", ""),
                            new JProperty("dccTotalVentaLetras", ventaCabecera.documentoventa_totalletras),
                            new JProperty("ITEMS",
                            new JArray(
                                from itemdetalle in listaDetalleVenta
                                select new JObject(
                                    //new JProperty("proId", itemdetalle.ventadetalle_producto_id),
                                    new JProperty("proNombre", itemdetalle.documentoventadetalle_descripcion),
                                    new JProperty("uniCodigo", itemdetalle.documentoventadetalle_unidadmedida_codigo),
                                    new JProperty("dcdCantidad", itemdetalle.documentoventadetalle_cantidad.ToString()),
                                    new JProperty("dcdVentaBruto", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dcdVenta", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dcdPrecioUnitario", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dccPrecioVariable", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dcdValorDesc", "0.00"),
                                    new JProperty("tipCodigo", "01"),
                                    new JProperty("dcdAfectacion", itemdetalle.documentoventadetalle_igv.ToString()),
                                    new JProperty("tpaCodigo", itemdetalle.documentoventadetalle_tipoafectacion)
                                    //new JProperty("dcdValorUnitario", itemdetalle.ventadetalle_subtotal.Value.ToString("F"))
                                    //new JProperty("dcdImporteTotal", itemdetalle.ventadetalle_total.Value.ToString("F"))
                                    )))
                        );
                    string documentoventa = documento.ToString();
                    generarFactura2RequestBody generarfacturaRB = new generarFactura2RequestBody();
                    generarfacturaRB.documento = documentoventa;
                    generarFactura2Request generarfacturaR = new generarFactura2Request();
                    generarfacturaR.Body = generarfacturaRB;
                    wsemision wsemision = new wsemisionClient();
                    generarFactura2Response generarfacturaRSP = new generarFactura2Response();
                    generarfacturaRSP = wsemision.generarFactura2(generarfacturaR);
                    string respuesta = generarfacturaRSP.Body.@return;
                    JObject jsonrespuesta = JObject.Parse(respuesta);

                    #endregion

                    JsonResult resultado = new JsonResult();

                    int status = (int)jsonrespuesta["status"];
                    if (status == 1)
                    {


                        int responseCode = (int)jsonrespuesta["responseCode"];
                        string digestValue = (string)jsonrespuesta["digestValue"];
                        if (responseCode != 0 || digestValue.Equals(""))
                        {
                            ventaCabecera.documentoventa_estadodocumento_id = 3;//PENDIENTE DE VERIFICACION
                            ventaCabecera.documentoventa_estadosunat = responseCode.ToString();
                            db.Entry(ventaCabecera).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        else
                        {
                            ventaCabecera.documentoventa_digestvalue = digestValue;
                            ventaCabecera.documentoventa_estadodocumento_id = 1;//CORRECTO SUNAT
                            ventaCabecera.documentoventa_estadosunat = responseCode.ToString();
                            db.Entry(ventaCabecera).State = EntityState.Modified;
                            db.SaveChanges();
                            resultado.Data = new
                            {
                                flag = 1
                            };
                        }
                    }
                    else
                    {
                        ventaCabecera.documentoventa_estadodocumento_id = 4;// CON ERRORES
                        ventaCabecera.documentoventa_estadosunat = (string)jsonrespuesta["error"];
                        ventaCabecera.documentoventa_digestvalue = "";
                        db.Entry(ventaCabecera).State = EntityState.Modified;
                        db.SaveChanges();
                        resultado.Data = new
                        {
                            flag = 4
                        };
                    }

                    transaccion.transaccion_estadoemision = true;
                    db.Entry(transaccion).State = EntityState.Modified;
                    db.SaveChanges();

                    bancos banco = db.bancos.Find(transaccion.transaccion_banco_id);
                    cuentasbanco cuenta = db.cuentasbanco.Find(transaccion.transaccion_cuentabanco_id);
                    string clienteDireccion = "-";
                    if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACIONES
                    {
                        transaccionesseparacion transaccionseparacion = db.transaccionesseparacion.Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id).FirstOrDefault();
                        separaciones separacion = db.separaciones.Find(transaccionseparacion.transaccionseparacion_separacion_id);

                        clientes cliente = db.clientes.Find(separacion.separacion_cliente_id);
                        clienteDireccion = cliente.cliente_direccion;
                    }
                    else
                    {
                        clienteDireccion = db.sp_obtenerDireccionCliente(transaccion.transaccion_id).FirstOrDefault();
                    }

                    string documento_descripcion = "";
                    if (ventaCabecera.documentoventa_tipodocumentoventa_id == 1)//BOLETA
                    {
                        tipoDocumentoVenta = db.tiposdocumentoventa.Find(2);//FACTURA
                        documento_descripcion = "BOLETA DE VENTA ELECTRONICA";
                    }
                    else if (ventaCabecera.documentoventa_tipodocumentoventa_id == 2)//FACTURA
                    {
                        tipoDocumentoVenta = db.tiposdocumentoventa.Find(1);//BOLETA
                        documento_descripcion = "FACTURA DE VENTA ELECTRONICA";
                    }

                    // transaction.Complete();

                    //CARGANDO DATOS DEL DOCUMENTO PARA IMPRIMIR


                    documentoModelo = new DocumentoVentaModelo
                    {
                        documento_cliente_nombre = ventaCabecera.documentoventa_cliente_nombre,
                        documento_cliente_nroDocumento = ventaCabecera.documentoventa_cliente_nrodocumento,
                        documento_cliente_direccion = clienteDireccion,
                        documento_empresa_correo = empresa.empresa_correo,
                        documento_empresa_direccion = empresa.empresa_direccion + " - " + empresa.empresa_departamento + " - " + empresa.empresa_provincia + " - " + empresa.empresa_distrito,
                        documento_empresa_nombre = empresa.empresa_nombrecomercial,
                        documento_empresa_numeroContacto = empresa.empresa_nrocontacto,
                        documento_fechaEmision = ventaCabecera.documentoventa_fechaemision,
                        documento_fechaVencimiento = ventaCabecera.documentoventa_fechavencimiento,
                        documento_igv = ventaCabecera.documentoventa_igv.Value,
                        documento_serie = ventaCabecera.documentoventa_serie_serie + "-" + ventaCabecera.documentoventa_serie_numeracion,
                        documento_subtotal = ventaCabecera.documentoventa_subtotal.Value,
                        documento_total = ventaCabecera.documentoventa_total.Value,
                        documento_digestValue = ventaCabecera.documentoventa_digestvalue,
                        documento_descripcion = documento_descripcion,
                        documento_empresa_documento = empresa.empresa_documento,
                        documento_moneda_descripcion = moneda.moneda_descripcion,
                        documento_qrcodeValue = empresa.empresa_documento + "|" + empresa.tiposdocumentoidentidad.tipodocumentoidentidad_codigo + "|" + ventaCabecera.documentoventa_serie_serie + "|" + ventaCabecera.documentoventa_serie_numeracion + "|" + ventaCabecera.documentoventa_igv + "|" + ventaCabecera.documentoventa_total + "|" + ventaCabecera.documentoventa_fechaemision + "|" + ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo + "|" + ventaCabecera.documentoventa_cliente_nrodocumento,
                        moneda_simbolo = moneda.moneda_caracter,
                        transaccion_fechadeposito = transaccion.transaccion_fechadeposito.Value.ToString("dd/MM/yyyy"),
                        transaccion_nrooperacion = transaccion.transaccion_nrooperacion,
                        transaccion_banco = banco == null ? "-" : banco.banco_descripcioncorta,
                        transaccion_cuenta = cuenta == null ? "-" : cuenta.cuentabanco_cuenta.Substring(cuenta.cuentabanco_cuenta.Length - 4, 4),
                        documento_montoletras = ventaCabecera.documentoventa_totalletras
                    };
                    foreach (var itemDetalle in listaDetalleVenta)
                    {
                        DocumentoDetalleModelo detalle = new DocumentoDetalleModelo
                        {
                            documentoDetalle_cantidad = itemDetalle.documentoventadetalle_cantidad.Value,
                            documentoDetalle_codigo = itemDetalle.documentoventadetalle_codigo,
                            documentoDetalle_descripcion = itemDetalle.documentoventadetalle_descripcion,
                            documentoDetalle_total = itemDetalle.documentoventadetalle_total.Value,
                            documentoDetalle_unidad = "UNIDAD",
                            documentoDetalle_valorUnitario = itemDetalle.documentoventadetalle_subtotal.Value
                        };
                        detalleModelo.Add(detalle);
                    }
                    documentoModelo.detalleVenta = detalleModelo;
                }

                InvoiceTemplate(documentoModelo);

                //FIN CARGADO
                return documentoModelo;
            }
            catch (Exception ex)
            {
                JsonResult resultado = new JsonResult();
                resultado.Data = new
                {
                    flag = 0
                };
                return null;
            }
        }

        public static Byte[] PdfSharpConvert(String html)
        {
            Byte[] res = null;
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    var pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4);
            //    pdf.Save(ms);
            //    res = ms.ToArray();
            //}

            PdfDocument pdf = PdfGenerator.GeneratePdf("<p><h1>Hello World</h1>This is html rendered text</p>", PageSize.A4);
            pdf.Save("document125.pdf");

            return res;
        }

        public String InvoiceTemplate(DocumentoVentaModelo model)
        {

            String html = @"<div id='invoice'>
                    <div class='invoice overflow-auto'>
                        <div style='min-width: 600px;'>
                            <header>
                                <div class='row'>
                                    <div class='col-3'>
                                        <div class='text-center'>
                                            <img src='/Img/Vivemas-logo-v2.svg' data-holder-rendered='true' class='img-fluid'>
                                        </div>
                                    </div>
                                    <div class='col-6 company-details'>
                                        <h2 class='name' style='text-align:center'>
                                            <b id='documento_empresa_nombre'>VIVEINCO S.A.C.</b>
                                        </h2>
                                        <div style='text-align:center' id='documento_empresa_direccion'><b>Domicilio Fiscal:</b>AV. VICTOR ANDRES BELAUNDE MZA. C LOTE. 4 URB. URBANIZACION CIUDAD MAGISTERIAL - AREQUIPA - AREQUIPA - AREQUIPA</div>
                                        <div style='text-align:center' id='documento_empresa_numeroContacto'><b>Telefono:</b>054-627969, +51 933063251</div>
                                        <div style='text-align:center' id='documento_empresa_correo'><b>Correo:</b>cobranza@vivemasinmobiliaria.com</div>
                                    </div>
                                    <div class='col-3' style='border:solid 1px;'>
                                        <div class='font-xl text-center' id='documento_empresa_documento'>RUC: 20603192517</div>
                                        <div class='font-xl text-center font-weight-bold' id='documento_descripcion'></div>
                                        <div class='text-center font-lg' id='documento_serie'>B001-00000722</div>
                                    </div>
                                </div>
                            </header>
                            <main>
                                <div class='row'>
                                    <div class=' col-12' style='border:solid 1px;'>
                                        <div class='row'>
                                            <div class='col-2'>
                                                <b>Cliente</b>
                                            </div>
                                            <div class='col-10' id='documento_cliente_nombre'>: JOSE LUIS NEYRA PORTUGAL</div>
                                        </div>
                                        <div class='row'>
                                            <div class='col-2'>
                                                <b>Nro. Documento</b>
                                            </div>
                                            <div class='col-10' id='documento_cliente_nroDocumento'>: 43281086</div>
                                        </div>
                                        <div class='row'>
                                            <div class='col-2'>
                                                <b>Direccion</b>
                                            </div>
                                            <div class='col-10' id='documento_cliente_direccion'>: URB. BARTOLOME HERRERA MZ. B LT. 06</div>
                                        </div>
                                        <div class='row'>
                                            <div class='col-2'>
                                                <b>Fecha de Emision</b>
                                            </div>
                                            <div class='col-5' id='documento_fechaEmision'>: 2021-05-10</div>
                                            <div class='col-2'>
                                                <b>Moneda</b>
                                            </div>
                                            <div class='col-3' id='documento_moneda_descripcion'>: SOLES</div>
                                        </div>
                                    </div>

                                </div>
                                <div class='row mt-1'>
                                    <table border='1' style='width:100%'>
                                        <thead>
                                            <tr>
                                                <th style='padding:5px;'>CANTIDAD</th>
                                                <th class='text-center' style='padding:5px;'>CODIGO</th>
                                                <th class='text-center' style='padding:5px;'>DESCRIPCION</th>
                                                <th class='text-center' style='padding:5px;'>U. M.</th>
                                                <th class='text-center'>VALOR UNITARIO</th>
                                                <th class='text-center' style='width:100px'>IMPORTE DE VENTA</th>
                                            </tr>
                                        </thead>
                                        <tbody id='tbodyDetalles'>
                                         <tr><td class='text-right'>1</td><td class='text-center'>AC-09</td><td class='text-left'>POR EL PAGO DE LA LETRA NRO 6 SEGUN PROFORMA NRO 00000879 POR LA COMPRA DEL LOTE 09 DE LA MANZANA AC DEL PROYECTO SUNSET BAY BEACH CONDO - PAGO ANTICIPADO</td><td class='text-center'>UNIDAD</td><td class='text-right'>100</td><td class='text-right'>100</td></tr></tbody>
                                    </table>
                                </div>

                                <!--<div class='thanks'>Sin impuestos</div>-->
                                <div class='row'>
                                    <div class='col-6'>
                                        <div class='text-center'>
                                            <div class='row ml-1 mt-2 mb-2' id='documento_montoletras'>SON: CIEN CON 00/100 SOLES</div>
                                            <div class='thanks mt-1 mt-2 mb-2'>
                                                <canvas id='qr-code' height='100' width='100'></canvas>
                                            </div>
                                        </div>
                                    </div>
                                    <div class='col-6' style='padding:0;'>
                                        <table width='100%' border='1'>
                                            <tbody><tr>
                                                <td class='text-right'><b>OP. GRAVADA:</b></td>
                                                <td style='width:100px'>
                                                    <div class='ml-1 text-left monedaSimbolo' style='float:left' id='moneda_simbolo'>S/</div>
                                                    <div class='text-right' style='float:right'>0.00</div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='text-right'><b>OP. EXONERADA:</b></td>
                                                <td style='width:100px'>
                                                    <div class='ml-1 text-left monedaSimbolo' style='float:left' id='moneda_simbolo2'>S/</div>
                                                    <div class='text-right' style='float:right'>0.00</div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='text-right'><b>OP. INAFECTA:</b></td>
                                                <td style='width:100px'>
                                                    <div class='ml-1 text-left monedaSimbolo' style='float:left'>S/</div>
                                                    <div class='text-right' style='float:right' id='documento_subtotal'>100</div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='text-right'><b>ISC:</b></td>
                                                <td style='width:100px'>
                                                    <div class='ml-1 text-left monedaSimbolo' style='float:left'>S/</div>
                                                    <div class='text-right' style='float:right'>0.00</div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='text-right'><b>IGV:</b></td>
                                                <td class='text-right'>
                                                    <div class='ml-1 text-left monedaSimbolo' style='float:left'>S/</div>
                                                    <div class='text-right' style='float:right' id='documento_igv'>0</div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='text-right'><b>ICBPER:</b></td>
                                                <td style='width:100px'>
                                                    <div class='ml-1 text-left monedaSimbolo' style='float:left'>S/</div>
                                                    <div class='text-right' style='float:right'>0.00</div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='text-right'><b>OTROS CARGOS:</b></td>
                                                <td style='width:100px'>
                                                    <div class='ml-1 text-left monedaSimbolo' style='float:left'>S/</div>
                                                    <div class='text-right' style='float:right'>0.00</div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='text-right'><b>OTROS TRIBUTOS:</b></td>
                                                <td style='width:100px'>
                                                    <div class='ml-1 text-left monedaSimbolo' style='float:left'>S/</div>
                                                    <div class='text-right' style='float:right'>0.00</div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class='text-right'><b>TOTAL:</b></td>
                                                <td class='text-right'>
                                                    <div class='ml-1 text-left monedaSimbolo' style='float:left'>S/</div>
                                                    <div class='text-right documentoTotal' style='float:right'>100</div>
                                                </td>
                                            </tr>
                                        </tbody></table>
                                    </div>
                                    <div class='col-12 mt-2'>
                                        <div style='border-bottom:solid 2px'><b>Informacion Adicional</b></div>

                                        <div class='col-6'>
                                            <div class='row'>
                                                <div class='col-6'>
                                                    Fecha Deposito:
                                                </div>
                                                <div class='col-6' id='transaccion_fechadeposito'>10/05/2021</div>
                                            </div>
                                            <div class='row'>
                                                <div class='col-6'>
                                                    Total:
                                                </div>
                                                <div class='col-6 documentoTotal'>100</div>
                                            </div>
                                            <div class='row'>
                                                <div class='col-6'>
                                                    Nro de Operacion:
                                                </div>
                                                <div class='col-6' id='transaccion_nrooperacion'></div>
                                            </div>
                                            <div class='row'>
                                                <div class='col-6'>
                                                    Banco:
                                                </div>
                                                <div class='col-6' id='transaccion_banco'>-</div>
                                            </div>
                                            <div class='row'>
                                                <div class='col-6'>
                                                    Cuenta:
                                                </div>
                                                <div class='col-6' id='transaccion_cuenta'>-</div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class='notices mt-2'>
                                        <div>Aviso:</div>
                                        <div class='notice'>
                                            Este documento es una representacion impresa de un documento emitido en la sunat desde el sistema del contribuyente.
                                            El emisor electronico puede verificarla utilizando su clave SOL, el adquiriente o Usuario puede consultar su validez en SUNAT virtual: www.sunat.gob.pe, en Opciones sin clave SOL/Consulta de validez CPE.
                                        </div>
                                    </div>
                                </div>

                            </main>
                            <footer id='documento_digestValue'>ESTO ES UNA PREVISUALIZACION</footer>
                        </div>
                        <!--DO NOT DELETE THIS div. IT is responsible for showing footer always at the bottom-->
                        <div></div>
                    </div>

                </div>";
            PdfDocument pdf = PdfGenerator.GeneratePdf(html, PageSize.A4);
            pdf.Save("D:\\DOC\\document125.pdf");



            return null;
        }

        private DocumentoVentaModelo GenerarDocumentoVentaFechaEmision(long transaccionId, string fechaEmisionDet)
        {
            try
            {
                DocumentoVentaModelo documentoModelo;
                List<DocumentoDetalleModelo> detalleModelo = new List<DocumentoDetalleModelo>();
                using (TransactionScope transaction = new TransactionScope())
                {
                    long empresaId = long.Parse(Session["empresaId"].ToString());
                    long usuarioId = long.Parse(Session["usuario"].ToString());
                    string codigoPago = "";
                    empresas empresa = db.empresas.Find(empresaId);
                    transacciones transaccion = db.transacciones.Find(transaccionId);
                    documentosventa ventaCabecera = new documentosventa();
                    ventaCabecera.documentoventa_transaccion_id = transaccion.transaccion_id;
                    ventaCabecera.documentoventa_estadodocumento_id = 2;//PENDIENTE DE ENVIO
                    //DATOS DE LA EMPRESA
                    ventaCabecera.documentoventa_empresa_id = empresa.empresa_id;
                    ventaCabecera.documentoventa_empresa_nrodocumento = empresa.empresa_documento;
                    ventaCabecera.documentoventa_empresa_razonsocial = empresa.empresa_nombre;
                    ventaCabecera.documentoventa_empresa_tipodocumentoidentidad_codigo = empresa.tiposdocumentoidentidad.tipodocumentoidentidad_codigo;
                    ventaCabecera.documentoventa_empresa_ubigeo = empresa.empresa_ubigeo;
                    ventaCabecera.documentoventa_empresa_direccion = empresa.empresa_direccion;
                    ventaCabecera.documentoventa_empresa_zona = empresa.empresa_zona;
                    ventaCabecera.documentoventa_empresa_distrito = empresa.empresa_distrito;
                    ventaCabecera.documentoventa_empresa_provincia = empresa.empresa_provincia;
                    ventaCabecera.documentoventa_empresa_departamento = empresa.empresa_departamento;
                    ventaCabecera.documentoventa_empresa_codigopais = empresa.empresa_codigopais;
                    //FIN DATOS DE LA EMPRESA
                    //DATOS DE CLIENTE
                    if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACION
                    {
                        transaccionesseparacion transaccionSeparacion = db.transaccionesseparacion
                            .Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id)
                            .FirstOrDefault();
                        separaciones separacion = db.separaciones.Find(transaccionSeparacion.transaccionseparacion_separacion_id);
                        clientes datosCliente = db.clientes.Find(separacion.separacion_cliente_id);
                        ventaCabecera.documentoventa_cliente_nombre = datosCliente.cliente_razonsocial;
                        ventaCabecera.documentoventa_cliente_nrodocumento = datosCliente.cliente_nrodocumento;
                        ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo = datosCliente.tiposdocumentoidentidad.tipodocumentoidentidad_codigo;
                        codigoPago = "SEP";
                    }
                    else if (transaccion.transaccion_tipotransaccion_id == 3 || transaccion.transaccion_tipotransaccion_id == 4)//PAGOS CASH O PAGO CUOTA
                    {
                        sp_obtenerDatosCliente_Result datosCliente = db.sp_obtenerDatosCliente(transaccionId).FirstOrDefault();
                        ventaCabecera.documentoventa_cliente_nombre = datosCliente.cliente_nombre;
                        ventaCabecera.documentoventa_cliente_nrodocumento = datosCliente.cliente_nrodocumento;
                        ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo = datosCliente.cliente_tipodocumentoIdentidad_codigo;
                        codigoPago = datosCliente.cotizacion_lote_nombre;
                    }
                    //FIN DATOS CLIENTE
                    //DATOS DEL DOCUMENTO ELECTRONICO
                    monedas moneda = db.monedas.Find(transaccion.transaccion_moneda_id);
                    tiposdocumentoventa tipoDocumentoVenta;
                    series serie;
                    DateTime fechaEmision = DateTime.Parse(fechaEmisionDet);
                    if (ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo.Equals("6")) //RUC - FACTURA
                    {
                        tipoDocumentoVenta = db.tiposdocumentoventa.Find(2);//FACTURA
                        serie = db.series.Find(1);
                    }
                    else // BOLETAS PARA CUALQUIER OTRO TIPO DE DOCUMENTO
                    {
                        tipoDocumentoVenta = db.tiposdocumentoventa.Find(1);//BOLETA
                        serie = db.series.Find(2);
                    }
                    ventaCabecera.documentoventa_tipodocumentoventa_id = tipoDocumentoVenta.tipodocumentoventa_id;
                    ventaCabecera.documentoventa_tipodocumentoventa_codigo = tipoDocumentoVenta.tipodocumentoventa_codigosunat;
                    ventaCabecera.documentoventa_serie_serie = serie.serie_serie;
                    ventaCabecera.documentoventa_serie_numeracion = serie.serie_numeracion.Value.ToString("D8");
                    ventaCabecera.documentoventa_moneda_id = moneda.moneda_id;
                    ventaCabecera.documentoventa_moneda_codigo = moneda.moneda_descripcioncorta;
                    ventaCabecera.documentoventa_tipoemision = "1";//SERVIDOR REAL
                    ventaCabecera.documentoventa_tipometodopago_id = transaccion.transaccion_tipometodopago_id;
                    ventaCabecera.documentoventa_fechaemision = fechaEmision.ToString("yyyy-MM-dd");
                    ventaCabecera.documentoventa_horaemision = fechaEmision.ToString("hh:mm:ss");
                    ventaCabecera.documentoventa_fechavencimiento = fechaEmision.ToString("yyyy-MM-dd");
                    ventaCabecera.documentoventa_subtotal = transaccion.transaccion_montonetototal;
                    ventaCabecera.documentoventa_igv = 0;
                    ventaCabecera.documentoventa_total = transaccion.transaccion_montonetototal;
                    ventaCabecera.documentoventa_condicioncodigo = "ANT";
                    if (moneda.moneda_descripcioncorta.Equals("PEN"))
                    {
                        ventaCabecera.documentoventa_totalletras = Utilities.ToString(transaccion.transaccion_montonetototal.ToString()).ToUpper();
                    }
                    else
                    {
                        ventaCabecera.documentoventa_totalletras = Utilities.ToStringDolares(transaccion.transaccion_montonetototal.ToString()).ToUpper();
                    }
                    ventaCabecera.documentoventa_fechacreacion = fechaEmision;
                    ventaCabecera.documentoventa_usuariocreacion = usuarioId;
                    serie.serie_numeracion = serie.serie_numeracion + 1;
                    db.Entry(serie).State = EntityState.Modified;
                    db.SaveChanges();
                    //FIN DATOS DEL DOCUMENTO ELECTRONICO (FALTAN LOS DATOS DE LA SUNAAAT NO  OLVIDAR)
                    db.documentosventa.Add(ventaCabecera);
                    db.SaveChanges();
                    //DETALLE DE LA VENTA
                    List<documentosventadetalle> listaDetalleVenta = new List<documentosventadetalle>();
                    if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACION
                    {
                        transaccionesseparacion transaccionSeparacion = db.transaccionesseparacion
                            .Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id)
                            .FirstOrDefault();
                        documentosventadetalle detalleVenta = new documentosventadetalle();
                        detalleVenta.documentoventadetalle_documentoventa_id = ventaCabecera.documentoventa_id;
                        detalleVenta.documentoventadetalle_descripcion = transaccionSeparacion.transaccionseparacion_descripcion + "- PAGO ANTICIPADO";
                        detalleVenta.documentoventadetalle_unidadmedida_codigo = "NIU"; //CODIGO PARA UNIDAD NO MOVER
                        detalleVenta.documentoventadetalle_codigo = codigoPago;
                        detalleVenta.documentoventadetalle_cantidad = 1;
                        detalleVenta.documentoventadetalle_tipoventasunat = "02";
                        detalleVenta.documentoventadetalle_tipoafectacion = "30"; //INAFECTA
                        detalleVenta.documentoventadetalle_subtotal = transaccion.transaccion_montonetototal;
                        detalleVenta.documentoventadetalle_igv = 0;
                        detalleVenta.documentoventadetalle_total = transaccion.transaccion_montonetototal;
                        detalleVenta.documentoventadetalle_fechacreacion = fechaEmision;
                        detalleVenta.documentoventadetalle_usuariocreacion = usuarioId;
                        listaDetalleVenta.Add(detalleVenta);
                    }
                    else if (transaccion.transaccion_tipotransaccion_id == 3 || transaccion.transaccion_tipotransaccion_id == 4)//PAGOS CASH O PAGO CUOTA
                    {
                        List<pagos> listaPagos = db.pagos.Where(pag => pag.pago_transaccion_id == transaccion.transaccion_id).ToList();
                        foreach (pagos itemPago in listaPagos)
                        {
                            documentosventadetalle detalleVenta = new documentosventadetalle();
                            detalleVenta.documentoventadetalle_documentoventa_id = ventaCabecera.documentoventa_id;
                            detalleVenta.documentoventadetalle_descripcion = itemPago.pago_descripcion + " - PAGO ANTICIPADO";
                            detalleVenta.documentoventadetalle_unidadmedida_codigo = "NIU"; //CODIGO PARA UNIDAD NO MOVER
                            detalleVenta.documentoventadetalle_codigo = codigoPago;
                            detalleVenta.documentoventadetalle_cantidad = 1;
                            detalleVenta.documentoventadetalle_tipoventasunat = "02";
                            detalleVenta.documentoventadetalle_tipoafectacion = "30"; //INAFECTA
                            detalleVenta.documentoventadetalle_subtotal = itemPago.pago_montonetopago;
                            detalleVenta.documentoventadetalle_igv = 0;
                            detalleVenta.documentoventadetalle_total = itemPago.pago_montonetopago;
                            detalleVenta.documentoventadetalle_fechacreacion = fechaEmision;
                            detalleVenta.documentoventadetalle_usuariocreacion = usuarioId;
                            listaDetalleVenta.Add(detalleVenta);
                        }
                    }
                    db.documentosventadetalle.AddRange(listaDetalleVenta);
                    db.SaveChanges();
                    configuraciones configuracion = db.configuraciones.Where(con => con.configuracion_empresa_id == empresaId).FirstOrDefault();
                    //FIN DETALLE DE LA VENTA

                    #region Facturacion electronica

                    string rutaDocumento = "";
                    string rutaCdrDocumento = "";

                    if (ventaCabecera.documentoventa_tipodocumentoventa_id == 1)//BOLETA
                    {
                        rutaDocumento = configuracion.configuracion_rutaboleta;
                        rutaCdrDocumento = configuracion.configuracion_rutacdrboleta;
                    }
                    else if (ventaCabecera.documentoventa_tipodocumentoventa_id == 2)//FACTURA
                    {
                        rutaDocumento = configuracion.configuracion_rutafactura;
                        rutaCdrDocumento = configuracion.configuracion_rutacdrfactura;
                    }
                    JObject documento = new JObject(
                            new JProperty("rutaDocumento", rutaDocumento.Replace("/", "\\")),
                            new JProperty("rutaCdrDocumento", rutaCdrDocumento.Replace("/", "\\")),
                            new JProperty("rutaFirmaElectronica", configuracion.configuracion_rutafirmadigital.Replace("/", "\\")),
                            new JProperty("passFirmaElectronica", configuracion.configuracion_passfirmadigital),
                            new JProperty("credencialesFirmaElectronica", configuracion.configuracion_credencialfirmadigital),
                            new JProperty("passCredencialFimaElectronica", configuracion.configuracion_passcredencialfirmadigital),
                            new JProperty("nombreFirmaElectronica", "signatureVIVEINCO"),
                            new JProperty("usuarioEmisor", configuracion.configuracion_usuarioemisor),
                            new JProperty("passEmisor", configuracion.configuracion_passemisor),
                            new JProperty("tipoEmision", ventaCabecera.documentoventa_tipoemision),//1 Emision sunat , 2 Emision OSE, 3 Emision Pruebas
                            new JProperty("dccTotalGravado", ventaCabecera.documentoventa_igv.ToString()),
                            new JProperty("dccFechaEmision", ventaCabecera.documentoventa_fechaemision),
                            new JProperty("dccHoraEmision", ventaCabecera.documentoventa_horaemision),
                            new JProperty("dccFechaVencimiento", ventaCabecera.documentoventa_fechavencimiento),
                            new JProperty("empRazonSocial", empresa.empresa_nombre),
                            new JProperty("empRuc", empresa.empresa_documento),
                            new JProperty("empTipoEntidad", ventaCabecera.documentoventa_empresa_tipodocumentoidentidad_codigo),
                            new JProperty("ubiCodigo", empresa.empresa_ubigeo),
                            new JProperty("empDireccion", empresa.empresa_direccion),
                            new JProperty("empZona", empresa.empresa_zona),
                            new JProperty("empDistrito", empresa.empresa_distrito),
                            new JProperty("empProvincia", empresa.empresa_provincia),
                            new JProperty("empDepartamento", empresa.empresa_departamento),
                            new JProperty("empCodigoPais", empresa.empresa_codigopais),
                            new JProperty("tdcCodigo", ventaCabecera.documentoventa_tipodocumentoventa_codigo),
                            new JProperty("dccSerie", ventaCabecera.documentoventa_serie_serie),
                            new JProperty("dccNumero", ventaCabecera.documentoventa_serie_numeracion),
                            new JProperty("entNombre", ventaCabecera.documentoventa_cliente_nombre),
                            new JProperty("entDocumento", ventaCabecera.documentoventa_cliente_nrodocumento),
                            new JProperty("tdeCodigo", ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo),
                            new JProperty("monCodigo", ventaCabecera.documentoventa_moneda_codigo),
                            new JProperty("dccTotalIgv", ventaCabecera.documentoventa_igv),
                            new JProperty("dccTotalVenta", ventaCabecera.documentoventa_total),
                            new JProperty("dccCondicion", "CON"),//CONTADO
                                                                 //new JProperty("dccGuiaderemision", ""),
                            new JProperty("dccTotalVentaLetras", ventaCabecera.documentoventa_totalletras),
                            new JProperty("ITEMS",
                            new JArray(
                                from itemdetalle in listaDetalleVenta
                                select new JObject(
                                    //new JProperty("proId", itemdetalle.ventadetalle_producto_id),
                                    new JProperty("proNombre", itemdetalle.documentoventadetalle_descripcion),
                                    new JProperty("uniCodigo", itemdetalle.documentoventadetalle_unidadmedida_codigo),
                                    new JProperty("dcdCantidad", itemdetalle.documentoventadetalle_cantidad.ToString()),
                                    new JProperty("dcdVentaBruto", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dcdVenta", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dcdPrecioUnitario", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dccPrecioVariable", itemdetalle.documentoventadetalle_total.ToString()),
                                    new JProperty("dcdValorDesc", "0.00"),
                                    new JProperty("tipCodigo", "01"),
                                    new JProperty("dcdAfectacion", itemdetalle.documentoventadetalle_igv.ToString()),
                                    new JProperty("tpaCodigo", itemdetalle.documentoventadetalle_tipoafectacion)
                                    //new JProperty("dcdValorUnitario", itemdetalle.ventadetalle_subtotal.Value.ToString("F"))
                                    //new JProperty("dcdImporteTotal", itemdetalle.ventadetalle_total.Value.ToString("F"))
                                    )))
                        );
                    string documentoventa = documento.ToString();
                    generarFactura2RequestBody generarfacturaRB = new generarFactura2RequestBody();
                    generarfacturaRB.documento = documentoventa;
                    generarFactura2Request generarfacturaR = new generarFactura2Request();
                    generarfacturaR.Body = generarfacturaRB;
                    wsemision wsemision = new wsemisionClient();
                    generarFactura2Response generarfacturaRSP = new generarFactura2Response();
                    generarfacturaRSP = wsemision.generarFactura2(generarfacturaR);
                    string respuesta = generarfacturaRSP.Body.@return;
                    JObject jsonrespuesta = JObject.Parse(respuesta);

                    #endregion

                    JsonResult resultado = new JsonResult();

                    int status = (int)jsonrespuesta["status"];
                    if (status == 1)
                    {


                        int responseCode = (int)jsonrespuesta["responseCode"];
                        string digestValue = (string)jsonrespuesta["digestValue"];
                        if (responseCode != 0 || digestValue.Equals(""))
                        {
                            ventaCabecera.documentoventa_estadodocumento_id = 3;//PENDIENTE DE VERIFICACION
                            ventaCabecera.documentoventa_estadosunat = responseCode.ToString();
                            db.Entry(ventaCabecera).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        else
                        {
                            ventaCabecera.documentoventa_digestvalue = digestValue;
                            ventaCabecera.documentoventa_estadodocumento_id = 1;//CORRECTO SUNAT
                            ventaCabecera.documentoventa_estadosunat = responseCode.ToString();
                            db.Entry(ventaCabecera).State = EntityState.Modified;
                            db.SaveChanges();
                            resultado.Data = new
                            {
                                flag = 1
                            };
                        }
                    }
                    else
                    {
                        ventaCabecera.documentoventa_estadodocumento_id = 4;// CON ERRORES
                        ventaCabecera.documentoventa_estadosunat = (string)jsonrespuesta["error"];
                        ventaCabecera.documentoventa_digestvalue = "";
                        db.Entry(ventaCabecera).State = EntityState.Modified;
                        db.SaveChanges();
                        resultado.Data = new
                        {
                            flag = 4
                        };
                    }

                    transaccion.transaccion_estadoemision = true;
                    db.Entry(transaccion).State = EntityState.Modified;
                    db.SaveChanges();

                    bancos banco = db.bancos.Find(transaccion.transaccion_banco_id);
                    cuentasbanco cuenta = db.cuentasbanco.Find(transaccion.transaccion_cuentabanco_id);
                    string clienteDireccion = "-";
                    if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACIONES
                    {
                        transaccionesseparacion transaccionseparacion = db.transaccionesseparacion.Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id).FirstOrDefault();
                        separaciones separacion = db.separaciones.Find(transaccionseparacion.transaccionseparacion_separacion_id);

                        clientes cliente = db.clientes.Find(separacion.separacion_cliente_id);
                        clienteDireccion = cliente.cliente_direccion;
                    }
                    else
                    {
                        clienteDireccion = db.sp_obtenerDireccionCliente(transaccion.transaccion_id).FirstOrDefault();
                    }

                    string documento_descripcion = "";
                    if (ventaCabecera.documentoventa_tipodocumentoventa_id == 1)//BOLETA
                    {
                        tipoDocumentoVenta = db.tiposdocumentoventa.Find(2);//FACTURA
                        documento_descripcion = "BOLETA DE VENTA ELECTRONICA";
                    }
                    else if (ventaCabecera.documentoventa_tipodocumentoventa_id == 2)//FACTURA
                    {
                        tipoDocumentoVenta = db.tiposdocumentoventa.Find(1);//BOLETA
                        documento_descripcion = "FACTURA DE VENTA ELECTRONICA";
                    }

                    transaction.Complete();

                    //CARGANDO DATOS DEL DOCUMENTO PARA IMPRIMIR


                    documentoModelo = new DocumentoVentaModelo
                    {
                        documento_cliente_nombre = ventaCabecera.documentoventa_cliente_nombre,
                        documento_cliente_nroDocumento = ventaCabecera.documentoventa_cliente_nrodocumento,
                        documento_cliente_direccion = clienteDireccion,
                        documento_empresa_correo = empresa.empresa_correo,
                        documento_empresa_direccion = empresa.empresa_direccion + " - " + empresa.empresa_departamento + " - " + empresa.empresa_provincia + " - " + empresa.empresa_distrito,
                        documento_empresa_nombre = empresa.empresa_nombrecomercial,
                        documento_empresa_numeroContacto = empresa.empresa_nrocontacto,
                        documento_fechaEmision = ventaCabecera.documentoventa_fechaemision,
                        documento_fechaVencimiento = ventaCabecera.documentoventa_fechavencimiento,
                        documento_igv = ventaCabecera.documentoventa_igv.Value,
                        documento_serie = ventaCabecera.documentoventa_serie_serie + "-" + ventaCabecera.documentoventa_serie_numeracion,
                        documento_subtotal = ventaCabecera.documentoventa_subtotal.Value,
                        documento_total = ventaCabecera.documentoventa_total.Value,
                        documento_digestValue = ventaCabecera.documentoventa_digestvalue,
                        documento_descripcion = documento_descripcion,
                        documento_empresa_documento = empresa.empresa_documento,
                        documento_moneda_descripcion = moneda.moneda_descripcion,
                        documento_qrcodeValue = empresa.empresa_documento + "|" + empresa.tiposdocumentoidentidad.tipodocumentoidentidad_codigo + "|" + ventaCabecera.documentoventa_serie_serie + "|" + ventaCabecera.documentoventa_serie_numeracion + "|" + ventaCabecera.documentoventa_igv + "|" + ventaCabecera.documentoventa_total + "|" + ventaCabecera.documentoventa_fechaemision + "|" + ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo + "|" + ventaCabecera.documentoventa_cliente_nrodocumento,
                        moneda_simbolo = moneda.moneda_caracter,
                        transaccion_fechadeposito = transaccion.transaccion_fechadeposito.Value.ToString("dd/MM/yyyy"),
                        transaccion_nrooperacion = transaccion.transaccion_nrooperacion,
                        transaccion_banco = banco == null ? "-" : banco.banco_descripcioncorta,
                        transaccion_cuenta = cuenta == null ? "-" : cuenta.cuentabanco_cuenta.Substring(cuenta.cuentabanco_cuenta.Length - 4, 4),
                        documento_montoletras = ventaCabecera.documentoventa_totalletras
                    };
                    foreach (var itemDetalle in listaDetalleVenta)
                    {
                        DocumentoDetalleModelo detalle = new DocumentoDetalleModelo
                        {
                            documentoDetalle_cantidad = itemDetalle.documentoventadetalle_cantidad.Value,
                            documentoDetalle_codigo = itemDetalle.documentoventadetalle_codigo,
                            documentoDetalle_descripcion = itemDetalle.documentoventadetalle_descripcion,
                            documentoDetalle_total = itemDetalle.documentoventadetalle_total.Value,
                            documentoDetalle_unidad = "UNIDAD",
                            documentoDetalle_valorUnitario = itemDetalle.documentoventadetalle_subtotal.Value
                        };
                        detalleModelo.Add(detalle);
                    }
                    documentoModelo.detalleVenta = detalleModelo;
                }
                //FIN CARGADO
                return documentoModelo;
            }
            catch (Exception ex)
            {
                JsonResult resultado = new JsonResult();
                resultado.Data = new
                {
                    flag = 0
                };
                return null;
            }
        }

        public ActionResult cargarModelo(long transaccionId)
        {
            JsonResult resultado = new JsonResult();
            try
            {
                DocumentoVentaModelo documentoModelo = new DocumentoVentaModelo();
                List<DocumentoDetalleModelo> detalleModelo = new List<DocumentoDetalleModelo>();
                transacciones transaccion = db.transacciones.Find(transaccionId);
                long empresaId = long.Parse(Session["empresaId"].ToString());
                empresas empresa = db.empresas.Find(empresaId);
                string tipoDocumento = "";
                string codigoPago = "";
                if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACION
                {
                    transaccionesseparacion transaccionSeparacion = db.transaccionesseparacion
                        .Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id)
                        .FirstOrDefault();
                    separaciones separacion = db.separaciones.Find(transaccionSeparacion.transaccionseparacion_separacion_id);
                    clientes datosCliente = db.clientes.Find(separacion.separacion_cliente_id);
                    documentoModelo.documento_cliente_nombre = datosCliente.cliente_razonsocial;
                    documentoModelo.documento_cliente_nroDocumento = datosCliente.cliente_nrodocumento;
                    tipoDocumento = datosCliente.tiposdocumentoidentidad.tipodocumentoidentidad_codigo;
                    codigoPago = "SEP";
                }
                else if (transaccion.transaccion_tipotransaccion_id == 3 || transaccion.transaccion_tipotransaccion_id == 4)//PAGOS CASH O PAGO CUOTA
                {
                    sp_obtenerDatosCliente_Result datosCliente = db.sp_obtenerDatosCliente(transaccionId).FirstOrDefault();
                    documentoModelo.documento_cliente_nombre = datosCliente.cliente_nombre;
                    documentoModelo.documento_cliente_nroDocumento = datosCliente.cliente_nrodocumento;
                    tipoDocumento = datosCliente.cliente_tipodocumentoIdentidad_codigo;
                    codigoPago = datosCliente.cotizacion_lote_nombre;
                }
                string clienteDireccion = "-";
                if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACIONES
                {
                    transaccionesseparacion transaccionseparacion = db.transaccionesseparacion.Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id).FirstOrDefault();
                    separaciones separacion = db.separaciones.Find(transaccionseparacion.transaccionseparacion_separacion_id);

                    clientes cliente = db.clientes.Find(separacion.separacion_cliente_id);
                    clienteDireccion = cliente.cliente_direccion;
                }
                else
                {
                    clienteDireccion = db.sp_obtenerDireccionCliente(transaccion.transaccion_id).FirstOrDefault();
                }
                tiposdocumentoventa tipoDocumentoVenta = new tiposdocumentoventa();
                series serie;
                if (tipoDocumento.Equals("6")) //RUC - FACTURA
                {
                    serie = db.series.Find(1);
                }
                else // BOLETAS PARA CUALQUIER OTRO TIPO DE DOCUMENTO
                {
                    serie = db.series.Find(2);
                }

                string documento_descripcion = "";
                if (tipoDocumentoVenta.tipodocumentoventa_id == 1)//BOLETA
                {
                    tipoDocumentoVenta = db.tiposdocumentoventa.Find(2);//FACTURA
                    documento_descripcion = "BOLETA DE VENTA ELECTRONICA";
                }
                else if (tipoDocumentoVenta.tipodocumentoventa_id == 2)//FACTURA
                {
                    tipoDocumentoVenta = db.tiposdocumentoventa.Find(1);//BOLETA
                    documento_descripcion = "FACTURA DE VENTA ELECTRONICA";
                }
                monedas moneda = db.monedas.Find(transaccion.transaccion_moneda_id);
                bancos banco = db.bancos.Find(transaccion.transaccion_banco_id);
                cuentasbanco cuenta = db.cuentasbanco.Find(transaccion.transaccion_cuentabanco_id);
                string totalletras = "";
                if (moneda.moneda_descripcioncorta.Equals("PEN"))
                {
                    totalletras = Utilities.ToString(transaccion.transaccion_montonetototal.ToString()).ToUpper();
                }
                else
                {
                    totalletras = Utilities.ToStringDolares(transaccion.transaccion_montonetototal.ToString()).ToUpper();
                }

                documentoModelo.documento_cliente_direccion = clienteDireccion;
                documentoModelo.documento_empresa_correo = empresa.empresa_correo;
                documentoModelo.documento_empresa_direccion = empresa.empresa_direccion + " - " + empresa.empresa_departamento + " - " + empresa.empresa_provincia + " - " + empresa.empresa_distrito;
                documentoModelo.documento_empresa_nombre = empresa.empresa_nombrecomercial;
                documentoModelo.documento_empresa_numeroContacto = empresa.empresa_nrocontacto;
                documentoModelo.documento_fechaEmision = DateTime.Now.ToString("yyyy-MM-dd");
                documentoModelo.documento_fechaVencimiento = DateTime.Now.ToString("yyyy-MM-dd");
                documentoModelo.documento_igv = 0;
                documentoModelo.documento_serie = serie.serie_serie + "-" + serie.serie_numeracion.Value.ToString("D8");
                documentoModelo.documento_subtotal = transaccion.transaccion_montonetototal.Value;
                documentoModelo.documento_total = transaccion.transaccion_montonetototal.Value;
                documentoModelo.documento_digestValue = "ESTO ES UNA PREVISUALIZACION";
                documentoModelo.documento_descripcion = documento_descripcion;
                documentoModelo.documento_empresa_documento = empresa.empresa_documento;
                documentoModelo.documento_moneda_descripcion = moneda.moneda_descripcion;
                documentoModelo.documento_qrcodeValue = empresa.empresa_documento + "|" + empresa.tiposdocumentoidentidad.tipodocumentoidentidad_codigo + "|" + serie.serie_serie + "|" + serie.serie_numeracion.Value.ToString("D8") + "|" + "0" + "|" + transaccion.transaccion_montonetototal.Value + "|" + DateTime.Now.ToString("yyyy-MM-dd") + "|" + tipoDocumento + "|" + documentoModelo.documento_cliente_nroDocumento;
                documentoModelo.moneda_simbolo = moneda.moneda_caracter;
                documentoModelo.transaccion_fechadeposito = transaccion.transaccion_fechadeposito.Value.ToString("dd/MM/yyyy");
                documentoModelo.transaccion_nrooperacion = transaccion.transaccion_nrooperacion;
                documentoModelo.transaccion_banco = banco == null ? "-" : banco.banco_descripcioncorta;
                documentoModelo.transaccion_cuenta = cuenta == null ? "-" : cuenta.cuentabanco_cuenta.Substring(cuenta.cuentabanco_cuenta.Length - 4, 4);
                documentoModelo.documento_montoletras = totalletras;

                if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACION
                {
                    transaccionesseparacion transaccionSeparacion = db.transaccionesseparacion
                        .Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id)
                        .FirstOrDefault();

                    DocumentoDetalleModelo detalle = new DocumentoDetalleModelo
                    {
                        documentoDetalle_cantidad = 1,
                        documentoDetalle_codigo = codigoPago,
                        documentoDetalle_descripcion = transaccionSeparacion.transaccionseparacion_descripcion + "- PAGO ANTICIPADO",
                        documentoDetalle_total = transaccion.transaccion_montonetototal.Value,
                        documentoDetalle_unidad = "UNIDAD",
                        documentoDetalle_valorUnitario = transaccion.transaccion_montonetototal.Value
                    };

                    detalleModelo.Add(detalle);
                }
                else if (transaccion.transaccion_tipotransaccion_id == 3 || transaccion.transaccion_tipotransaccion_id == 4)//PAGOS CASH O PAGO CUOTA
                {
                    List<pagos> listaPagos = db.pagos.Where(pag => pag.pago_transaccion_id == transaccion.transaccion_id).ToList();
                    foreach (pagos itemPago in listaPagos)
                    {
                        DocumentoDetalleModelo detalle = new DocumentoDetalleModelo
                        {
                            documentoDetalle_cantidad = 1,
                            documentoDetalle_codigo = codigoPago,
                            documentoDetalle_descripcion = itemPago.pago_descripcion + " - PAGO ANTICIPADO",
                            documentoDetalle_total = itemPago.pago_montonetopago.Value,
                            documentoDetalle_unidad = "UNIDAD",
                            documentoDetalle_valorUnitario = itemPago.pago_montonetopago.Value
                        };
                        detalleModelo.Add(detalle);
                    }
                }
                documentoModelo.detalleVenta = detalleModelo;
                resultado.Data = new
                {
                    flag = 1,
                    data = documentoModelo
                };
            }
            catch (Exception ex)
            {
                resultado.Data = new
                {
                    flag = 0
                };
            }
            return resultado;
        }

        public ActionResult cargarModeloFechaEmision(long transaccionId, string fechaEmision)
        {
            JsonResult resultado = new JsonResult();
            try
            {
                DocumentoVentaModelo documentoModelo = new DocumentoVentaModelo();
                List<DocumentoDetalleModelo> detalleModelo = new List<DocumentoDetalleModelo>();
                transacciones transaccion = db.transacciones.Find(transaccionId);
                long empresaId = long.Parse(Session["empresaId"].ToString());
                empresas empresa = db.empresas.Find(empresaId);
                string tipoDocumento = "";
                string codigoPago = "";
                if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACION
                {
                    transaccionesseparacion transaccionSeparacion = db.transaccionesseparacion
                        .Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id)
                        .FirstOrDefault();
                    separaciones separacion = db.separaciones.Find(transaccionSeparacion.transaccionseparacion_separacion_id);
                    clientes datosCliente = db.clientes.Find(separacion.separacion_cliente_id);
                    documentoModelo.documento_cliente_nombre = datosCliente.cliente_razonsocial;
                    documentoModelo.documento_cliente_nroDocumento = datosCliente.cliente_nrodocumento;
                    tipoDocumento = datosCliente.tiposdocumentoidentidad.tipodocumentoidentidad_codigo;
                    codigoPago = "SEP";
                }
                else if (transaccion.transaccion_tipotransaccion_id == 3 || transaccion.transaccion_tipotransaccion_id == 4)//PAGOS CASH O PAGO CUOTA
                {
                    sp_obtenerDatosCliente_Result datosCliente = db.sp_obtenerDatosCliente(transaccionId).FirstOrDefault();
                    documentoModelo.documento_cliente_nombre = datosCliente.cliente_nombre;
                    documentoModelo.documento_cliente_nroDocumento = datosCliente.cliente_nrodocumento;
                    tipoDocumento = datosCliente.cliente_tipodocumentoIdentidad_codigo;
                    codigoPago = datosCliente.cotizacion_lote_nombre;
                }
                string clienteDireccion = "-";
                if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACIONES
                {
                    transaccionesseparacion transaccionseparacion = db.transaccionesseparacion.Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id).FirstOrDefault();
                    separaciones separacion = db.separaciones.Find(transaccionseparacion.transaccionseparacion_separacion_id);

                    clientes cliente = db.clientes.Find(separacion.separacion_cliente_id);
                    clienteDireccion = cliente.cliente_direccion;
                }
                else
                {
                    clienteDireccion = db.sp_obtenerDireccionCliente(transaccion.transaccion_id).FirstOrDefault();
                }
                tiposdocumentoventa tipoDocumentoVenta = new tiposdocumentoventa();
                series serie;
                if (tipoDocumento.Equals("6")) //RUC - FACTURA
                {
                    serie = db.series.Find(1);
                }
                else // BOLETAS PARA CUALQUIER OTRO TIPO DE DOCUMENTO
                {
                    serie = db.series.Find(2);
                }

                string documento_descripcion = "";
                if (tipoDocumentoVenta.tipodocumentoventa_id == 1)//BOLETA
                {
                    tipoDocumentoVenta = db.tiposdocumentoventa.Find(2);//FACTURA
                    documento_descripcion = "BOLETA DE VENTA ELECTRONICA";
                }
                else if (tipoDocumentoVenta.tipodocumentoventa_id == 2)//FACTURA
                {
                    tipoDocumentoVenta = db.tiposdocumentoventa.Find(1);//BOLETA
                    documento_descripcion = "FACTURA DE VENTA ELECTRONICA";
                }
                monedas moneda = db.monedas.Find(transaccion.transaccion_moneda_id);
                bancos banco = db.bancos.Find(transaccion.transaccion_banco_id);
                cuentasbanco cuenta = db.cuentasbanco.Find(transaccion.transaccion_cuentabanco_id);
                string totalletras = "";
                if (moneda.moneda_descripcioncorta.Equals("PEN"))
                {
                    totalletras = Utilities.ToString(transaccion.transaccion_montonetototal.ToString()).ToUpper();
                }
                else
                {
                    totalletras = Utilities.ToStringDolares(transaccion.transaccion_montonetototal.ToString()).ToUpper();
                }

                documentoModelo.documento_cliente_direccion = clienteDireccion;
                documentoModelo.documento_empresa_correo = empresa.empresa_correo;
                documentoModelo.documento_empresa_direccion = empresa.empresa_direccion + " - " + empresa.empresa_departamento + " - " + empresa.empresa_provincia + " - " + empresa.empresa_distrito;
                documentoModelo.documento_empresa_nombre = empresa.empresa_nombrecomercial;
                documentoModelo.documento_empresa_numeroContacto = empresa.empresa_nrocontacto;
                documentoModelo.documento_fechaEmision = DateTime.Parse(fechaEmision).ToString("yyyy-MM-dd");
                documentoModelo.documento_fechaVencimiento = DateTime.Now.ToString("yyyy-MM-dd");
                documentoModelo.documento_igv = 0;
                documentoModelo.documento_serie = serie.serie_serie + "-" + serie.serie_numeracion.Value.ToString("D8");
                documentoModelo.documento_subtotal = transaccion.transaccion_montonetototal.Value;
                documentoModelo.documento_total = transaccion.transaccion_montonetototal.Value;
                documentoModelo.documento_digestValue = "ESTO ES UNA PREVISUALIZACION";
                documentoModelo.documento_descripcion = documento_descripcion;
                documentoModelo.documento_empresa_documento = empresa.empresa_documento;
                documentoModelo.documento_moneda_descripcion = moneda.moneda_descripcion;
                documentoModelo.documento_qrcodeValue = empresa.empresa_documento + "|" + empresa.tiposdocumentoidentidad.tipodocumentoidentidad_codigo + "|" + serie.serie_serie + "|" + serie.serie_numeracion.Value.ToString("D8") + "|" + "0" + "|" + transaccion.transaccion_montonetototal.Value + "|" + DateTime.Now.ToString("yyyy-MM-dd") + "|" + tipoDocumento + "|" + documentoModelo.documento_cliente_nroDocumento;
                documentoModelo.moneda_simbolo = moneda.moneda_caracter;
                documentoModelo.transaccion_fechadeposito = transaccion.transaccion_fechadeposito.Value.ToString("dd/MM/yyyy");
                documentoModelo.transaccion_nrooperacion = transaccion.transaccion_nrooperacion;
                documentoModelo.transaccion_banco = banco == null ? "-" : banco.banco_descripcioncorta;
                documentoModelo.transaccion_cuenta = cuenta == null ? "-" : cuenta.cuentabanco_cuenta.Substring(cuenta.cuentabanco_cuenta.Length - 4, 4);
                documentoModelo.documento_montoletras = totalletras;

                if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACION
                {
                    transaccionesseparacion transaccionSeparacion = db.transaccionesseparacion
                        .Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id)
                        .FirstOrDefault();

                    DocumentoDetalleModelo detalle = new DocumentoDetalleModelo
                    {
                        documentoDetalle_cantidad = 1,
                        documentoDetalle_codigo = codigoPago,
                        documentoDetalle_descripcion = transaccionSeparacion.transaccionseparacion_descripcion + "- PAGO ANTICIPADO",
                        documentoDetalle_total = transaccion.transaccion_montonetototal.Value,
                        documentoDetalle_unidad = "UNIDAD",
                        documentoDetalle_valorUnitario = transaccion.transaccion_montonetototal.Value
                    };

                    detalleModelo.Add(detalle);
                }
                else if (transaccion.transaccion_tipotransaccion_id == 3 || transaccion.transaccion_tipotransaccion_id == 4)//PAGOS CASH O PAGO CUOTA
                {
                    List<pagos> listaPagos = db.pagos.Where(pag => pag.pago_transaccion_id == transaccion.transaccion_id).ToList();
                    foreach (pagos itemPago in listaPagos)
                    {
                        DocumentoDetalleModelo detalle = new DocumentoDetalleModelo
                        {
                            documentoDetalle_cantidad = 1,
                            documentoDetalle_codigo = codigoPago,
                            documentoDetalle_descripcion = itemPago.pago_descripcion + " - PAGO ANTICIPADO",
                            documentoDetalle_total = itemPago.pago_montonetopago.Value,
                            documentoDetalle_unidad = "UNIDAD",
                            documentoDetalle_valorUnitario = itemPago.pago_montonetopago.Value
                        };
                        detalleModelo.Add(detalle);
                    }
                }
                documentoModelo.detalleVenta = detalleModelo;
                resultado.Data = new
                {
                    flag = 1,
                    data = documentoModelo
                };
            }
            catch (Exception ex)
            {
                resultado.Data = new
                {
                    flag = 0
                };
            }
            return resultado;
        }

        public ActionResult cargarModeloNotaCredito(long documentoId, long tipoAnulacion, string motivo)
        {
            JsonResult resultado = new JsonResult();
            try
            {
                long empresaId = long.Parse(Session["empresaId"].ToString());
                empresas empresa = db.empresas.Find(empresaId);
                documentosventa ventaCabecera = db.documentosventa.Find(documentoId);
                monedas moneda = db.monedas.Find(ventaCabecera.documentoventa_moneda_id);
                transacciones transaccion = db.transacciones.Find(ventaCabecera.documentoventa_transaccion_id);
                tiposnotacredito tiponotacredito;
                if (tipoAnulacion == 1)
                {
                    tiponotacredito = db.tiposnotacredito.Find(6);
                }
                else
                {
                    tiponotacredito = db.tiposnotacredito.Find(1);
                }
                string transaccion_fechadeposito = "-";
                string transaccion_nrooperacion = "-";
                string transaccion_banco = "-";
                string transaccion_cuenta = "-";
                string clienteDireccion = "-";
                if (transaccion != null)
                {
                    bancos banco = db.bancos.Find(transaccion.transaccion_banco_id);
                    cuentasbanco cuenta = db.cuentasbanco.Find(transaccion.transaccion_cuentabanco_id);
                    transaccion_fechadeposito = transaccion.transaccion_fechadeposito.Value.ToString("dd/MM/yyyy");
                    transaccion_nrooperacion = transaccion.transaccion_nrooperacion;
                    if (banco==null)
                    {

                    }
                    else
                    {
                        transaccion_banco = banco.banco_descripcioncorta;
                        transaccion_cuenta = cuenta.cuentabanco_cuenta.Substring(cuenta.cuentabanco_cuenta.Length - 4, 4);
                    }
    
                   
                    if (transaccion.transaccion_tipotransaccion_id == 1)//SEPARACIONES
                    {
                        transaccionesseparacion transaccionseparacion = db.transaccionesseparacion.Where(tse => tse.transaccionseparacion_transaccion_id == transaccion.transaccion_id).FirstOrDefault();
                        separaciones separacion = db.separaciones.Find(transaccionseparacion.transaccionseparacion_separacion_id);

                        clientes cliente = db.clientes.Find(separacion.separacion_cliente_id);
                        clienteDireccion = cliente.cliente_direccion;
                    }
                    else
                    {
                        clienteDireccion = db.sp_obtenerDireccionCliente(transaccion.transaccion_id).FirstOrDefault();
                    }
                }
                else
                {
                    clientes cliente = db.clientes.Where(cli => cli.cliente_nrodocumento == ventaCabecera.documentoventa_cliente_nrodocumento).FirstOrDefault();
                    clienteDireccion = cliente.cliente_direccion;
                }
                string documento_descripcion = "NOTA DE CREDITO ELECTRONICA";

                series serie;
                if (ventaCabecera.documentoventa_serie_serie.Equals("F001"))
                {
                    serie = db.series.Find(5);
                }
                else if (ventaCabecera.documentoventa_serie_serie.Equals("B001"))
                {
                    serie = db.series.Find(7);
                }
                else if (ventaCabecera.documentoventa_serie_serie.Equals("F002"))
                {
                    serie = db.series.Find(6);
                }
                else
                {
                    serie = db.series.Find(8);
                }

                DocumentoVentaModelo documentoModelo = new DocumentoVentaModelo
                {
                    documento_cliente_nombre = ventaCabecera.documentoventa_cliente_nombre,
                    documento_cliente_nroDocumento = ventaCabecera.documentoventa_cliente_nrodocumento,
                    documento_cliente_direccion = clienteDireccion,
                    documento_empresa_correo = empresa.empresa_correo,
                    documento_empresa_direccion = empresa.empresa_direccion + " - " + empresa.empresa_departamento + " - " + empresa.empresa_provincia + " - " + empresa.empresa_distrito,
                    documento_empresa_nombre = empresa.empresa_nombrecomercial,
                    documento_empresa_numeroContacto = empresa.empresa_nrocontacto,
                    documento_fechaEmision = DateTime.Now.ToString("yyyy-MM-dd"),
                    documento_igv = ventaCabecera.documentoventa_igv.Value,
                    documento_serie = serie.serie_serie + "-" + serie.serie_numeracion.Value.ToString("D8"),
                    documento_fechaemisionreferencia = ventaCabecera.documentoventa_fechaemision,
                    documento_subtotal = ventaCabecera.documentoventa_subtotal.Value,
                    documento_total = ventaCabecera.documentoventa_total.Value,
                    documento_digestValue = "ESTO ES UNA PREVISUALIZACION",
                    documento_descripcion = documento_descripcion,
                    documento_empresa_documento = empresa.empresa_documento,
                    documento_moneda_descripcion = moneda.moneda_descripcion,
                    documento_qrcodeValue = empresa.empresa_documento + "|" + empresa.tiposdocumentoidentidad.tipodocumentoidentidad_codigo + "|" + serie.serie_serie + "|" + serie.serie_numeracion.Value.ToString("D8") + "|" + ventaCabecera.documentoventa_igv + "|" + ventaCabecera.documentoventa_total + "|" + DateTime.Now.ToString("yyyy-MM-dd") + "|" + ventaCabecera.documentoventa_cliente_tipodocumentoidentidad_codigo + "|" + ventaCabecera.documentoventa_cliente_nrodocumento,
                    documento_montoletras = ventaCabecera.documentoventa_totalletras,
                    moneda_simbolo = moneda.moneda_caracter,
                    transaccion_fechadeposito = transaccion_fechadeposito,
                    transaccion_nrooperacion = transaccion_nrooperacion,
                    transaccion_banco = transaccion_banco,
                    transaccion_cuenta = transaccion_cuenta,
                    documento_documentoreferencia = ventaCabecera.documentoventa_serie_serie + "-" + ventaCabecera.documentoventa_serie_numeracion,
                    documento_tiponota = tiponotacredito.tiponotacredito_descripcion,
                    documento_motivo = motivo,

                };
                List<DocumentoDetalleModelo> detalleModelo = new List<DocumentoDetalleModelo>();
                List<documentosventadetalle> listaDetalleVenta = db.documentosventadetalle.Where(dvd => dvd.documentoventadetalle_documentoventa_id == documentoId).ToList();
                foreach (var itemDetalle in listaDetalleVenta)
                {
                    DocumentoDetalleModelo detalle = new DocumentoDetalleModelo
                    {
                        documentoDetalle_cantidad = itemDetalle.documentoventadetalle_cantidad.Value,
                        documentoDetalle_codigo = itemDetalle.documentoventadetalle_codigo,
                        documentoDetalle_descripcion = itemDetalle.documentoventadetalle_descripcion,
                        documentoDetalle_total = itemDetalle.documentoventadetalle_total.Value,
                        documentoDetalle_unidad = "UNIDAD",
                        documentoDetalle_valorUnitario = itemDetalle.documentoventadetalle_subtotal.Value
                    };
                    detalleModelo.Add(detalle);
                }
                documentoModelo.detalleVenta = detalleModelo;

                resultado.Data = new
                {
                    flag = 1,
                    data = documentoModelo
                };
            }
            catch (Exception ex)
            {
                resultado.Data = new
                {
                    flag = 0
                };
            }

            return resultado;
        }

        public ActionResult cargarEventos()
        {
            try
            {
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 1,
                    data = db.vw_cargarEventos.Select(vw => new
                    {
                        vw.evento_id,
                        vw.title,
                        vw.start,
                        color = vw.evento_estadoevento_id == 1 ? "standard" : vw.evento_estadoevento_id == 2 ? "#e55353" : vw.evento_estadoevento_id == 3 ? "#248f48" : "#d69405"
                    })
                };
                respuesta.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
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
        
        private string cargarColor(long? estadoevento)
        {
            switch (estadoevento)
            {
                case 1:
                    {
                        return "standard";
                    }
                case 2:
                    {
                        return "#e55353";
                    }
                case 3:
                    {
                        return "#248f48";
                    }
                default:
                    {
                        return "#d69405";
                    }
            }
        }
    }
}