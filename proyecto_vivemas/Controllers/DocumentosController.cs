using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using proyecto_vivemas.Models;
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

namespace proyecto_vivemas.Controllers
{
    /// <summary>
    /// Clase Controlador para los documentos de venta
    /// contiene todos los metodos para trabajar con los documentos de venta
    /// </summary>
    /// <remarks>
    /// esta clase puede mostrar vistas relacionadas con los documentos, puede enviar informacion de los documentos o informacion necesaria de los documentos
    /// a traves de un JsonResult
    /// </remarks>
    public class DocumentosController : Controller
    {
        /// <value>
        /// Objeto que conecta el modelo con el controlador 
        /// </value>
        vivemas_dbEntities db = new vivemas_dbEntities();
        /// <value>
        /// JsonResult usado para devolver informacion a la vista
        /// </value>
        JsonResult respuesta, resultado;
        // GET: Documentos
        /// <summary>crea la instancia de la vista</summary>
        /// <returns>vista de registro de ventas</returns>
        public ActionResult RegistroVentas()
        {
            return View();
        }
        /// <summary>crea la instancia de la vista</summary>
        /// <returns>vista de punto de venta</returns>
        public ActionResult PuntoVenta()
        {
            return View();
        }
        /// <summary>
        /// funcion encargada de devolver los documentos de venta
        /// </summary>
        /// <returns><c>json</c> convertido en <c>string</c></returns>
        public string CargarRegistroVentas()
        {
            string respuesta = "";
            try
            {
                var listaDocumentos = db.documentosventa.ToList().OrderByDescending(doc => doc.documentoventa_id).Select(doc => new
                {
                    doc.documentoventa_id,
                    doc.documentoventa_tipodocumentoventa_id,
                    doc.tiposdocumentoventa.tipodocumentoventa_descripcion,
                    serie = doc.documentoventa_serie_serie + "-" + doc.documentoventa_serie_numeracion,
                    notaId = obtenerNotaId(doc.documentoventa_id),
                    serieReferencia = obtenerSerieReferencia(doc.documentoventa_id),
                    doc.documentoventa_fechaemision,
                    doc.documentoventa_cliente_nrodocumento,
                    doc.documentoventa_cliente_nombre,
                    doc.estadosdocumento.estadodocumento_descripcion,
                    doc.documentoventa_estadosunat,
                    doc.documentoventa_moneda_codigo,
                    doc.documentoventa_total
                });

                respuesta = JsonConvert.SerializeObject(listaDocumentos);
                return respuesta;
            }
            catch (Exception ex)
            {
                return respuesta;
            }
        }
        /// <summary>
        /// funcion encargada de devolver el documento de referencia
        /// </summary>
        /// <remarks>
        /// la serie de referencia es para las notas de credito
        /// </remarks>
        /// <returns><c>string</c> con la serie y numero de la nota de credito</returns>
        private string obtenerSerieReferencia(long documentoId)
        {
            notascredito notacredito = db.notascredito.Where(nc => nc.notacredito_documentoventa_id == documentoId).FirstOrDefault();
            if (notacredito == null)
            {
                return "";
            }
            else
            {
                return notacredito.notacredito_serie_serie + "-" + notacredito.notacredito_serie_numeracion;
            }
        }
        /// <summary>
        /// funcion encargada de devolver el id de la nota de credito
        /// </summary>
        /// <returns><c>long</c> con el id de la nota de credito</returns>
        private long obtenerNotaId(long documentoId)
        {
            notascredito notacredito = db.notascredito.Where(nc => nc.notacredito_documentoventa_id == documentoId).FirstOrDefault();
            if (notacredito == null)
            {
                return 0;
            }
            else
            {
                return notacredito.notacredito_id;
            }
        }

        /// <summary>
        /// funcion encargada de cargar los tipos de documentos de ventas 
        /// </summary>        
        /// <returns><c>json</c> con los tipos de documentos de venta</returns>
        public ActionResult cargarTiposDocumentoVenta()
        {
            try
            {
                return Json(db.tiposdocumentoventa
                    .Where(tdv => tdv.tipodocumentoventa_tipo == 1)
                    .Select(tdv => new
                    {
                        id = tdv.tipodocumentoventa_id,
                        text = tdv.tipodocumentoventa_descripcion
                    }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de guardar en la base de datos y emitir el documento de venta desde el punto de venta
        /// </summary>
        /// <param name="documentoModelo">ViewModel con los datos del documento</param>
        /// <returns><c>json</c> con el estado de la emision del documento</returns>
        public ActionResult emitirDocumentoPuntoVenta(DocumentoVentaModelo documentoModelo)
        {
            try
            {
                clientes cliente = new clientes();
                tiposdocumentoidentidad tipodocumentoidentidad = new tiposdocumentoidentidad();
                long empresaId = long.Parse(Session["empresaId"].ToString());
                long usuarioId = long.Parse(Session["usuario"].ToString());
                using (TransactionScope transaction = new TransactionScope())
                {
                    empresas empresa = db.empresas.Find(empresaId);
                    tiposdocumentoventa tipodocumentoventa = db.tiposdocumentoventa.Find(documentoModelo.documento_tipodocumentoventa_id);
                    monedas moneda = db.monedas.Find(documentoModelo.documento_moneda_id);
                    bancos banco = db.bancos.Find(documentoModelo.documento_banco_id);
                    cuentasbanco cuenta = db.cuentasbanco.Find(documentoModelo.documento_cuenta_id);
                    DateTime fechaEmision = DateTime.Parse(documentoModelo.documento_fecha_emision);
                    DateTime? fechadeposito;
                    series serie;
                    if (documentoModelo.documento_cliente_id != null)
                    {
                        cliente = db.clientes.Find(documentoModelo.documento_cliente_id);
                        tipodocumentoidentidad = db.tiposdocumentoidentidad.Find(cliente.cliente_tipodocumentoidentidad_id);
                    }
                    if (tipodocumentoventa.tipodocumentoventa_id == 2)//FACTURA
                    {
                        serie = db.series.Find(3);
                    }
                    else
                    {
                        serie = db.series.Find(4);
                    }
                    if (documentoModelo.documento_fechadeposito == null)
                    {
                        fechadeposito = null;
                    }
                    else
                    {
                        fechadeposito = new DateTime();
                        fechadeposito = DateTime.Parse(documentoModelo.documento_fechadeposito);
                    }

                    documentosventa ventaCabecera = new documentosventa
                    {
                        documentoventa_empresa_id = empresa.empresa_id,
                        documentoventa_tipodocumentoventa_id = tipodocumentoventa.tipodocumentoventa_id,
                        documentoventa_estadodocumento_id = 2,//PENDIENTE DE ENVIO
                        documentoventa_tipometodopago_id = documentoModelo.documento_tipometodopago_id,
                        documentoventa_moneda_id = moneda.moneda_id,
                        documentoventa_tipoemision = "1",
                        documentoventa_fechaemision = fechaEmision.ToString("yyyy-MM-dd"),
                        documentoventa_horaemision = fechaEmision.ToString("hh:mm:ss"),
                        documentoventa_fechavencimiento = fechaEmision.ToString("yyyy-MM-dd"),
                        documentoventa_empresa_razonsocial = empresa.empresa_nombre,
                        documentoventa_empresa_nrodocumento = empresa.empresa_documento,
                        documentoventa_empresa_tipodocumentoidentidad_codigo = empresa.tiposdocumentoidentidad.tipodocumentoidentidad_codigo,
                        documentoventa_empresa_ubigeo = empresa.empresa_ubigeo,
                        documentoventa_empresa_direccion = empresa.empresa_direccion,
                        documentoventa_empresa_zona = empresa.empresa_zona,
                        documentoventa_empresa_distrito = empresa.empresa_distrito,
                        documentoventa_empresa_provincia = empresa.empresa_provincia,
                        documentoventa_empresa_departamento = empresa.empresa_departamento,
                        documentoventa_empresa_codigopais = empresa.empresa_codigopais,
                        documentoventa_tipodocumentoventa_codigo = tipodocumentoventa.tipodocumentoventa_codigosunat,
                        documentoventa_serie_serie = serie.serie_serie,
                        documentoventa_serie_numeracion = serie.serie_numeracion.Value.ToString("D8"),
                        documentoventa_cliente_nombre = documentoModelo.documento_cliente_id == null ? null : cliente.cliente_razonsocial,
                        documentoventa_cliente_nrodocumento = documentoModelo.documento_cliente_id == null ? null : cliente.cliente_nrodocumento,
                        documentoventa_cliente_tipodocumentoidentidad_codigo = documentoModelo.documento_cliente_id == null ? null : tipodocumentoidentidad.tipodocumentoidentidad_codigo,
                        documentoventa_moneda_codigo = moneda.moneda_descripcioncorta,
                        documentoventa_subtotal = documentoModelo.documento_subtotal,
                        documentoventa_igv = documentoModelo.documento_igv,
                        documentoventa_total = documentoModelo.documento_total,
                        documentoventa_condicioncodigo = "ANT",
                        documentoventa_totalletras = moneda.moneda_descripcioncorta.Equals("PEN") ? Utilities.ToString(documentoModelo.documento_total.ToString()).ToUpper() : Utilities.ToStringDolares(documentoModelo.documento_total.ToString()).ToUpper(),
                        documentoventa_fechacreacion = fechaEmision,
                        documentoventa_usuariocreacion = usuarioId,
                        documentoventa_banco_nombre = banco == null ? "-" : banco.banco_descripcioncorta,
                        documentoventa_cuenta_cuenta = cuenta == null ? "-" : cuenta.cuentabanco_cuenta,
                        documentoventa_fechadeposito = fechadeposito,
                        documentoventa_nrooperacion = documentoModelo.documento_nrooperacion
                    };
                    db.documentosventa.Add(ventaCabecera);
                    db.SaveChanges();
                    List<documentosventadetalle> listaDetalleVenta = new List<documentosventadetalle>();
                    foreach (var detalleItem in documentoModelo.detalleVenta)
                    {
                        documentosventadetalle detalleVenta = new documentosventadetalle
                        {
                            documentoventadetalle_documentoventa_id = ventaCabecera.documentoventa_id,
                            documentoventadetalle_descripcion = detalleItem.documentoDetalle_descripcion,
                            documentoventadetalle_unidadmedida_codigo = "NIU", //CODIGO PARA UNIDAD NO MOVER
                            documentoventadetalle_codigo = detalleItem.documentoDetalle_codigo,
                            documentoventadetalle_cantidad = 1,
                            documentoventadetalle_tipoventasunat = "02",
                            documentoventadetalle_tipoafectacion = "30",//INAFECTA
                            documentoventadetalle_subtotal = detalleItem.documentoDetalle_total,
                            documentoventadetalle_igv = 0,
                            documentoventadetalle_total = detalleItem.documentoDetalle_total,
                            documentoventadetalle_fechacreacion = fechaEmision,
                            documentoventadetalle_usuariocreacion = usuarioId
                        };
                        listaDetalleVenta.Add(detalleVenta);
                    }
                    db.documentosventadetalle.AddRange(listaDetalleVenta);
                    db.SaveChanges();
                    serie.serie_numeracion = serie.serie_numeracion + 1;
                    db.Entry(serie).State = EntityState.Modified;
                    db.SaveChanges();

                    #region emision electronica
                    configuraciones configuracion = db.configuraciones.Where(con => con.configuracion_empresa_id == empresaId).FirstOrDefault();
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
                            new JProperty("dccCondicion", "CON"),
                            new JProperty("dccTotalVentaLetras", ventaCabecera.documentoventa_totalletras),
                            new JProperty("ITEMS",
                            new JArray(
                                from itemdetalle in listaDetalleVenta
                                select new JObject(

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
                                    )))
                        );

                    #endregion
                    string documentoventa = documento.ToString();
                    generarFactura2RequestBody generarfacturaRB = new generarFactura2RequestBody();
                    generarfacturaRB.documento = documentoventa;
                    generarFactura2Request generarfacturaR = new generarFactura2Request();
                    generarfacturaR.Body = generarfacturaRB;
                    wsemision wsemision = new wsemisionClient();
                    generarFactura2Response generarfacturaRSP = new generarFactura2Response();
                    generarfacturaRSP = wsemision.generarFactura2(generarfacturaR);
                    string respuestajson = generarfacturaRSP.Body.@return;
                    JObject jsonrespuesta = JObject.Parse(respuestajson);

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


                    transaction.Complete();
                    respuesta = new JsonResult();
                    respuesta.Data = new
                    {
                        flag = 1,
                        ventaId = ventaCabecera.documentoventa_id
                    };
                    return respuesta;
                }
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

        /// <summary>
        /// Funcion encargada de reenviar el documento a la sunat y actualizar el registro en la base de datos
        /// </summary>
        /// <param name="documentoId"><c>long</c> id del documento a reenviar</param>
        /// <returns><c>json</c> con el resultado de la operacion</returns>
        public ActionResult reenviarSunat(long documentoId)
        {
            try
            {
                resultado = new JsonResult();
                using (TransactionScope transaction = new TransactionScope())
                {

                    long empresaId = long.Parse(Session["empresaId"].ToString());
                    long usuarioId = long.Parse(Session["usuario"].ToString());
                    string codigoPago = "";
                    empresas empresa = db.empresas.Find(empresaId);
                    documentosventa ventaCabecera = db.documentosventa.Find(documentoId);
                    configuraciones configuracion = db.configuraciones.Where(con => con.configuracion_empresa_id == empresaId).FirstOrDefault();
                    List<documentosventadetalle> listaDetalleVenta = db.documentosventadetalle.Where(dvd => dvd.documentoventadetalle_documentoventa_id == documentoId).ToList();
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
                            new JProperty("empRazonSocial", Utilities.reemplazarCaracteres(empresa.empresa_nombre)),
                            new JProperty("empRuc", empresa.empresa_documento),
                            new JProperty("empTipoEntidad", ventaCabecera.documentoventa_empresa_tipodocumentoidentidad_codigo),
                            new JProperty("ubiCodigo", empresa.empresa_ubigeo),
                            new JProperty("empDireccion", Utilities.reemplazarCaracteres(empresa.empresa_direccion)),
                            new JProperty("empZona", empresa.empresa_zona),
                            new JProperty("empDistrito", Utilities.reemplazarCaracteres(empresa.empresa_distrito)),
                            new JProperty("empProvincia", Utilities.reemplazarCaracteres(empresa.empresa_provincia)),
                            new JProperty("empDepartamento", Utilities.reemplazarCaracteres(empresa.empresa_departamento)),
                            new JProperty("empCodigoPais", empresa.empresa_codigopais),
                            new JProperty("tdcCodigo", ventaCabecera.documentoventa_tipodocumentoventa_codigo),
                            new JProperty("dccSerie", ventaCabecera.documentoventa_serie_serie),
                            new JProperty("dccNumero", ventaCabecera.documentoventa_serie_numeracion),
                            new JProperty("entNombre", Utilities.reemplazarCaracteres(ventaCabecera.documentoventa_cliente_nombre)),
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
                                    new JProperty("proNombre", Utilities.reemplazarCaracteres(itemdetalle.documentoventadetalle_descripcion + "(PAGO ANTICIPO)")),
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
                    string respuestaDocumento = "";
                    generarFactura2RequestBody generarfacturaRB = new generarFactura2RequestBody();
                    generarfacturaRB.documento = documentoventa;
                    generarFactura2Request generarfacturaR = new generarFactura2Request();
                    generarfacturaR.Body = generarfacturaRB;
                    wsemision wsemision = new wsemisionClient();
                    generarFactura2Response generarfacturaRSP = new generarFactura2Response();
                    generarfacturaRSP = wsemision.generarFactura2(generarfacturaR);
                    respuestaDocumento = generarfacturaRSP.Body.@return;
                    JObject jsonrespuesta = JObject.Parse(respuestaDocumento);
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
                            resultado.Data = new
                            {
                                flag = 2
                            };
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
                        string error = (string)jsonrespuesta["error"];
                        if (!error.Equals("Client.1033"))
                        {
                            ventaCabecera.documentoventa_estadodocumento_id = 4;// CON ERRORES
                            ventaCabecera.documentoventa_estadosunat = (string)jsonrespuesta["error"];
                        }
                        else
                        {
                            ventaCabecera.documentoventa_estadodocumento_id = 1;//CORRECTO SUNAT
                            ventaCabecera.documentoventa_estadosunat = "0";
                        }
                        db.Entry(ventaCabecera).State = EntityState.Modified;
                        db.SaveChanges();
                        resultado.Data = new
                        {
                            flag = 4
                        };
                    }
                    transaction.Complete();
                }
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

        /// <summary>
        /// Funcion encargada de anular un documento de venta
        /// </summary>
        /// <remarks>
        /// Las anulaciones se realizan siempre con una nota de credito
        /// </remarks>
        /// <param name="documentoId">id del documento</param>
        /// <param name="tipoAnulacion">id con el tipo de anulacion puede ser anulacion de la operacion o devolucion total</param>
        /// <param name="motivo">motivo por el que se anulo la operacion</param>
        /// <returns><c>json</c> con el resultado de la operacion</returns>
        public ActionResult anularDocumento(long documentoId, long tipoAnulacion, string motivo)
        {
            try
            {
                resultado = new JsonResult();
                using (TransactionScope transaction = new TransactionScope())
                {
                    documentosventa venta = db.documentosventa.Find(documentoId);
                    long empresaId = long.Parse(Session["empresaId"].ToString());
                    long usuarioId = long.Parse(Session["usuario"].ToString());
                    configuraciones configuracion = db.configuraciones.Where(con => con.configuracion_empresa_id == empresaId).FirstOrDefault();
                    string rutaDocumento = configuracion.configuracion_rutanotacredito;
                    string rutaCdrDocumento = configuracion.configuracion_rutacdrnotacredito;
                    series serie;
                    if (venta.documentoventa_serie_serie.Equals("F001"))
                    {
                        serie = db.series.Find(5);
                    }
                    else if (venta.documentoventa_serie_serie.Equals("B001"))
                    {
                        serie = db.series.Find(7);
                    }
                    else if (venta.documentoventa_serie_serie.Equals("F002"))
                    {
                        serie = db.series.Find(6);
                    }
                    else
                    {
                        serie = db.series.Find(8);
                    }
                    tiposdocumentoventa tipodocumentoventa = db.tiposdocumentoventa.Find(5);//NOTA CREDITO
                    tiposnotacredito tiponotacredito;
                    if (tipoAnulacion == 1)
                    {
                        tiponotacredito = db.tiposnotacredito.Find(6);
                    }
                    else
                    {
                        tiponotacredito = db.tiposnotacredito.Find(1);
                    }

                    notascredito notacredito = new notascredito();
                    notacredito.notacredito_empresa_id = empresaId;
                    notacredito.notacredito_tipodocumentoventa_id = tipodocumentoventa.tipodocumentoventa_id;//NOTA CREDITO
                    notacredito.notacredito_estadodocumento_id = 2; //PENDIENTE DE ENVIO
                    notacredito.notacredito_documentoventa_id = venta.documentoventa_id;
                    notacredito.notacredito_moneda_id = venta.documentoventa_moneda_id;
                    notacredito.notacredito_tiponotacredito_id = tiponotacredito.tiponotacredito_id;//ANULACION DE LA TRANSACCION
                    notacredito.notacredito_tipoemision = "1"; //SERVIDOR REAL
                    notacredito.notacredito_fechaemision = DateTime.Now.ToString("yyyy-MM-dd");
                    notacredito.notacredito_horaemision = DateTime.Now.ToString("hh:mm:ss");
                    notacredito.notacredito_empresa_razonsocial = venta.documentoventa_empresa_razonsocial;
                    notacredito.notacredito_empresa_nrodocumento = venta.documentoventa_empresa_nrodocumento;
                    notacredito.notacredito_empresa_tipodocumentoideintidad_codigo = venta.documentoventa_empresa_tipodocumentoidentidad_codigo;
                    notacredito.notacredito_empresa_ubigeo = venta.documentoventa_empresa_ubigeo;
                    notacredito.notacredito_empresa_direccion = venta.documentoventa_empresa_direccion;
                    notacredito.notacredito_empresa_zona = venta.documentoventa_empresa_zona;
                    notacredito.notacredito_empresa_distrito = venta.documentoventa_empresa_distrito;
                    notacredito.notacredito_empresa_provincia = venta.documentoventa_empresa_provincia;
                    notacredito.notacredito_empresa_departamento = venta.documentoventa_empresa_departamento;
                    notacredito.notacredito_empresa_codigopais = venta.documentoventa_empresa_codigopais;
                    notacredito.notacredito_tiponotacredito_codigo = tiponotacredito.tiponotacredito_codigo;
                    notacredito.notacredito_tipodocumentoventa_codigo = tipodocumentoventa.tipodocumentoventa_codigosunat;
                    notacredito.notacredito_serie_serie = serie.serie_serie;
                    notacredito.notacredito_serie_numeracion = serie.serie_numeracion.Value.ToString("D8");
                    notacredito.notacredito_cliente_nombre = venta.documentoventa_cliente_nombre;
                    notacredito.notacredito_cliente_nrodocumento = venta.documentoventa_cliente_nrodocumento;
                    notacredito.notacredito_cliente_tipodocumentoidentidad_codigo = venta.documentoventa_cliente_tipodocumentoidentidad_codigo;
                    notacredito.notacredito_moneda_codigo = venta.documentoventa_moneda_codigo;
                    notacredito.notacredito_subtotal = venta.documentoventa_subtotal;
                    notacredito.notacredito_igv = venta.documentoventa_igv;
                    notacredito.notacredito_total = venta.documentoventa_total;
                    notacredito.notacredito_seriereferencia = venta.documentoventa_serie_serie;
                    notacredito.notacredito_numeracionreferencia = venta.documentoventa_serie_numeracion;
                    notacredito.notacredito_tipodocumentoventa_codigoreferencia = venta.documentoventa_tipodocumentoventa_codigo;
                    notacredito.notacredito_descripcionnota = motivo.ToUpper();
                    notacredito.notacredito_totalletras = venta.documentoventa_totalletras;
                    notacredito.notacredito_usuariocreacion = usuarioId;
                    notacredito.notacredito_fechacreacion = DateTime.Now;
                    db.notascredito.Add(notacredito);
                    db.SaveChanges();
                    serie.serie_numeracion = serie.serie_numeracion + 1;
                    db.Entry(serie).State = EntityState.Modified;
                    db.SaveChanges();
                    List<notascreditodetalle> listaDetalleNotas = new List<notascreditodetalle>();
                    List<documentosventadetalle> listaVentaDetalle = db.documentosventadetalle.Where(dvd => dvd.documentoventadetalle_documentoventa_id == venta.documentoventa_id).ToList();

                    foreach (var itemDetalle in listaVentaDetalle)
                    {
                        notascreditodetalle detalleNota = new notascreditodetalle();
                        detalleNota.notacreditodetalle_notadecredito_id = notacredito.notacredito_id;
                        detalleNota.notacreditodetalle_descripcion = itemDetalle.documentoventadetalle_descripcion;
                        detalleNota.notacreditodetalle_unidadmedida_codigo = itemDetalle.documentoventadetalle_unidadmedida_codigo;
                        detalleNota.notacreditodetalle_cantidad = itemDetalle.documentoventadetalle_cantidad.ToString();
                        detalleNota.notacreditodetalle_codigo = itemDetalle.documentoventadetalle_codigo;
                        detalleNota.notacreditodetalle_tipoventasunat = "02";
                        detalleNota.notacreditodetalle_tipoafectacion = "30";
                        detalleNota.notacreditodetalle_subtotal = itemDetalle.documentoventadetalle_subtotal;
                        detalleNota.notacreditodetalle_igv = itemDetalle.documentoventadetalle_igv;
                        detalleNota.notacreditodetalle_total = itemDetalle.documentoventadetalle_total;
                        detalleNota.notacreditodetalle_usuariocreacion = usuarioId;
                        detalleNota.notacreditodetalle_fechacreacion = DateTime.Now;
                        listaDetalleNotas.Add(detalleNota);
                    }
                    db.notascreditodetalle.AddRange(listaDetalleNotas);
                    db.SaveChanges();

                    //ANULACION DE LA TRANSACCION
                    if (venta.documentoventa_transaccion_id != null)
                    {
                        transacciones transaccion = db.transacciones.Find(venta.documentoventa_transaccion_id);
                        transaccion.transaccion_estadotransaccion_id = 4;//ANULADO
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
                    #region emision electronica
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
                            new JProperty("tipoEmision", notacredito.notacredito_tipoemision),//1 Emision sunat , 2 Emision OSE, 3 Emision Pruebas
                            new JProperty("dccTotalGravado", notacredito.notacredito_igv.ToString()),
                            new JProperty("dccFechaEmision", notacredito.notacredito_fechaemision),
                            new JProperty("dccHoraEmision", notacredito.notacredito_horaemision),
                            new JProperty("empRazonSocial", Utilities.reemplazarCaracteres(notacredito.notacredito_empresa_razonsocial)),
                            new JProperty("empRuc", notacredito.notacredito_empresa_nrodocumento),
                            new JProperty("empTipoEntidad", notacredito.notacredito_empresa_tipodocumentoideintidad_codigo),
                            new JProperty("ubiCodigo", notacredito.notacredito_empresa_ubigeo),
                            new JProperty("empDireccion", Utilities.reemplazarCaracteres(notacredito.notacredito_empresa_direccion)),
                            new JProperty("empZona", Utilities.reemplazarCaracteres(notacredito.notacredito_empresa_zona)),
                            new JProperty("empDistrito", Utilities.reemplazarCaracteres(notacredito.notacredito_empresa_distrito)),
                            new JProperty("empProvincia", Utilities.reemplazarCaracteres(notacredito.notacredito_empresa_provincia)),
                            new JProperty("empDepartamento", Utilities.reemplazarCaracteres(notacredito.notacredito_empresa_departamento)),
                            new JProperty("empCodigoPais", notacredito.notacredito_empresa_codigopais),
                            new JProperty("tdcCodigo", notacredito.notacredito_tipodocumentoventa_codigo),
                            new JProperty("dccSerie", notacredito.notacredito_serie_serie),
                            new JProperty("dccNumero", notacredito.notacredito_serie_numeracion),
                            new JProperty("entNombre", Utilities.reemplazarCaracteres(notacredito.notacredito_cliente_nombre)),
                            new JProperty("entDocumento", notacredito.notacredito_cliente_nrodocumento == null ? "00000000" : notacredito.notacredito_cliente_nrodocumento),
                            new JProperty("tdeCodigo", notacredito.notacredito_cliente_nombre == null ? "1" : notacredito.notacredito_cliente_tipodocumentoidentidad_codigo),
                            new JProperty("monCodigo", notacredito.notacredito_moneda_codigo),
                            new JProperty("dccTotalIgv", notacredito.notacredito_igv),
                            new JProperty("dccTotalVenta", notacredito.notacredito_total),
                            new JProperty("dccSerieRef", notacredito.notacredito_seriereferencia),
                            new JProperty("dccNumeroRef", notacredito.notacredito_numeracionreferencia),
                            new JProperty("tdcCodigoRef", notacredito.notacredito_tipodocumentoventa_codigoreferencia),
                            new JProperty("tncCodigo", notacredito.notacredito_tiponotacredito_codigo),
                            new JProperty("dccDescripcionNota", notacredito.notacredito_descripcionnota),
                            //new JProperty("dccGuiaderemision", ""),
                            new JProperty("dccTotalVentaLetras", notacredito.notacredito_totalletras),
                            new JProperty("ITEMS",
                            new JArray(
                                from itemdetalle in listaDetalleNotas
                                select new JObject(
                                    new JProperty("proId", "1"),
                                    new JProperty("proNombre", itemdetalle.notacreditodetalle_descripcion),
                                    new JProperty("uniCodigo", itemdetalle.notacreditodetalle_unidadmedida_codigo),
                                    new JProperty("dcdCantidad", itemdetalle.notacreditodetalle_cantidad.ToString()),
                                    new JProperty("dcdVentaBruto", itemdetalle.notacreditodetalle_total.ToString()),
                                    new JProperty("dcdVenta", itemdetalle.notacreditodetalle_total.ToString()),
                                    new JProperty("dcdPrecioUnitario", itemdetalle.notacreditodetalle_total.ToString()),
                                    new JProperty("tipCodigo", "01"),
                                    new JProperty("dcdAfectacion", itemdetalle.notacreditodetalle_igv.ToString()),
                                    new JProperty("tpaCodigo", itemdetalle.notacreditodetalle_tipoafectacion),
                                    new JProperty("dcdValorUnitario", itemdetalle.notacreditodetalle_total.ToString()),
                                    new JProperty("dcdImporteTotal", itemdetalle.notacreditodetalle_total.ToString())
                                    )))
                        );
                    string documentoventa = documento.ToString();
                    generarNotaCreditoRequestBody generarfacturaRB = new generarNotaCreditoRequestBody();
                    generarfacturaRB.documento = documentoventa;
                    generarNotaCreditoRequest generarfacturaR = new generarNotaCreditoRequest();
                    generarfacturaR.Body = generarfacturaRB;
                    wsemision wsemision = new wsemisionClient();
                    generarNotaCreditoResponse generarfacturaRSP = new generarNotaCreditoResponse();
                    generarfacturaRSP = wsemision.generarNotaCredito(generarfacturaR);
                    string respuesta = generarfacturaRSP.Body.@return;
                    JObject jsonrespuesta = JObject.Parse(respuesta);
                    int status = (int)jsonrespuesta["status"];
                    if (status == 1)
                    {
                        int responseCode = (int)jsonrespuesta["responseCode"];
                        string digestValue = (string)jsonrespuesta["digestValue"];
                        if (responseCode != 0 || digestValue.Equals(""))
                        {
                            notacredito.notacredito_estadodocumento_id = 3;//PENDIENTE DE VERIFICACION
                            notacredito.notacredito_estadosunat = responseCode.ToString();
                            if (digestValue.Equals(""))
                            {
                                notacredito.notacredito_digestvalue = "";
                            }
                            else
                            {
                                notacredito.notacredito_digestvalue = digestValue;
                            }

                            db.Entry(notacredito).State = EntityState.Modified;
                            db.SaveChanges();
                            resultado.Data = new
                            {
                                flag = 2,
                                notaCreditoId = notacredito.notacredito_id
                            };
                        }
                        else
                        {
                            notacredito.notacredito_digestvalue = digestValue;
                            notacredito.notacredito_estadodocumento_id = 1;//CORRECTO SUNAT
                            notacredito.notacredito_estadosunat = responseCode.ToString();
                            db.Entry(notacredito).State = EntityState.Modified;
                            db.SaveChanges();
                            venta.documentoventa_estadodocumento_id = 5; //ANULADO
                            db.Entry(venta).State = EntityState.Modified;
                            db.SaveChanges();
                            resultado.Data = new
                            {
                                flag = 1,
                                notaCreditoId = notacredito.notacredito_id
                            };
                        }
                    }
                    else
                    {
                        notacredito.notacredito_estadodocumento_id = 4;// CON ERRORES
                        notacredito.notacredito_estadosunat = (string)jsonrespuesta["error"];
                        notacredito.notacredito_digestvalue = "";
                        db.Entry(notacredito).State = EntityState.Modified;
                        db.SaveChanges();
                        resultado.Data = new
                        {
                            flag = 4,
                            notaCreditoId = notacredito.notacredito_id
                        };
                    }
                    #endregion
                    transaction.Complete();
                }
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
    }
}