using MySql.Data.MySqlClient;
using proyecto_vivemas.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using proyecto_vivemas.ViewModels;
using Newtonsoft.Json;
using proyecto_vivemas.Util;
using proyecto_vivemas.Models;
using System.Transactions;
using System.Data.Entity;

namespace proyecto_vivemas.Controllers
{
    /// <summary>
    /// Clase Controlador para las cobranzas
    /// contiene todos los metodos para trabajar con las cobranzas
    /// </summary>
    /// <remarks>
    /// esta clase puede mostrar vistas relacionadas con las cobranzas, puede enviar informacion de las cobranzas o informacion necesaria de las cobranzas
    /// a traves de un JsonResult
    /// </remarks>
    public class CobranzasController : Controller
    {
        /// <value>
        /// Objeto que conecta el modelo con el controlador de la base de datos mysql
        /// </value>
        vivemasDB vivemas = new vivemasDB();
        /// <value>
        /// <c>MySqlConnection</c> conexion con la base de datos 
        /// </value>
        MySqlConnection connection;
        /// <value>
        /// <c>MySqlDataReader</c> data reader de la base de datos 
        /// </value>
        MySqlDataReader reader;
        /// <value>
        /// JsonResult usado para devolver informacion a la vista
        /// </value>
        JsonResult respuesta;
        /// <value>
        /// <c>MySqlCommand</c> command de la base de datos 
        /// </value>
       
        /// <value>
        /// <c>Utilities</c> <typeparamref name="Utilities"/> variable para usar las funciones de Utilities
        /// </value>
        /// <typeparam name="Utilities">Clase con funcionalidades utiles variadas para el sistema</typeparam> 
        Utilities util = new Utilities();
        /// <value>
        /// Objeto que conecta el modelo con el controlador 
        /// </value>
        vivemas_dbEntities db = new vivemas_dbEntities();
        /// <value>
        /// variable para hacer las consultas a la base de datos
        /// </value>
        string query = "";
        // GET: Cobranzas
        /// <summary>crea la instancia de la vista</summary>
        /// <returns>vista de mantenimiento de pagos</returns>
        public ActionResult Pagos()
        {
            tiposcambio tipocambio = db.tiposcambio
                .OrderByDescending(tc => tc.tipocambio_id)
                .FirstOrDefault();
            ViewBag.tipocambio = tipocambio.tipocambio_venta.ToString();
            return View();
        }
        /// <summary>crea la instancia de la vista</summary>
        /// <returns>vista de migracion de pagos</returns>
        public ActionResult MigrarPagos()
        {
            return View();
        }

        /// <summary>crea la instancia de la vista</summary>
        /// <returns>vista de punto de venta</returns>
        public ActionResult PuntoVenta()
        {
            return View();
        }

        /// <summary>crea la instancia de la vista</summary>
        /// <returns>vista de registro de eventos</returns>
        public ActionResult RegistroEventos()
        {
            return View();
        }

        /// <summary>
        /// funcion encargada de devolver los tipos de cambio
        /// </summary>
        /// <returns><c>json</c> convertido en <c>string</c></returns>
        public ActionResult cargarTipoCambio()
        {
            try
            {
                tiposcambio tipocambio = db.tiposcambio
                .OrderByDescending(tc => tc.tipocambio_id)
                .FirstOrDefault();
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    tipoCambioInterno = tipocambio.tipocambio_venta
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de devolver los pagos a la vista
        /// </summary>
        /// <returns><c>json</c> convertido en <c>string</c></returns>
        public string CargarPagos()
        {
            string respuesta = "";
            try
            {
                #region querymysql
                /*ListClientesProforma clientesProformas = new ListClientesProforma();
                clientesProformas.clientesProformas = new List<ClientesProforma>();
                query = "sp_cargarPagos";
                connection = vivemas.iniciarConexion();
                command = new MySqlCommand(query, connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    ClientesProforma clienteProforma = new ClientesProforma
                    {
                        codigo = int.Parse(reader["CODIGO"].ToString()),
                        proyecto = reader["PROYECTO"].ToString(),
                        lote = reader["LOTE"].ToString(),
                        tipDoc = reader["TIPDOC"].ToString(),
                        nroDocumento = reader["NRODOC"].ToString(),
                        cliente = reader["CLIENTE"].ToString(),
                        moneda = reader["MONEDA"].ToString(),
                        fechaVencimiento = reader["fechavencimiento"].ToString(),
                        montoCuota = double.Parse(reader["montocuota"].ToString()),
                        montoTotal = double.Parse(reader["TOTAL"].ToString()),
                        montoPendiente = double.Parse(reader["faltante"].ToString()),
                        cuotasFaltantes = int.Parse(reader["cuotaspendientes"].ToString())
                    };
                    clientesProformas.clientesProformas.Add(clienteProforma);
                }
                respuesta = JsonConvert.SerializeObject(clientesProformas.clientesProformas);*/
                #endregion
                List<sp_cargarContratos_Result> listaContratos = db.sp_cargarContratos().ToList();
                respuesta = JsonConvert.SerializeObject(listaContratos);
                return respuesta;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return respuesta;
            }
        }

        /// <summary>
        /// funcion encargada de cargar los datos de pagos desde la base de datos MySql
        /// </summary>        
        /// <returns><c>json</c> con los datos de los pagos</returns>
        public ActionResult CargarDatosPagos()
        {
            respuesta = new JsonResult();
            try
            {
                FormPagosData data = new FormPagosData();
                data.listTiposPagoModel = new List<TiposPagoModel>();
                data.listBancosOrigenModel = new List<BancosOrigenModel>();
                data.listBancosDestinoModel = new List<BancosDestinoModel>();

                string query = "SELECT id_tipag, destipag FROM tipo_pago ORDER BY orden";
                connection = vivemas.iniciarConexion();
                reader = vivemas.ejecutarQuery(query, connection);
                while (reader.Read())
                {
                    TiposPagoModel tipoPago = new TiposPagoModel
                    {
                        idTipoPago = reader["id_tipag"].ToString(),
                        descripcionTipoPago = reader["destipag"].ToString()
                    };
                    data.listTiposPagoModel.Add(tipoPago);
                }
                reader.Close();
                query = "SELECT id_banco,nombanco FROM banco WHERE estado = 1 ORDER BY id_banco";
                reader = vivemas.ejecutarQuery(query, connection);
                while (reader.Read())
                {
                    BancosOrigenModel bancoOrigen = new BancosOrigenModel
                    {
                        idBancoOrigen = reader["id_banco"].ToString(),
                        descripcionBancoOrigen = reader["nombanco"].ToString()
                    };
                    data.listBancosOrigenModel.Add(bancoOrigen);
                }
                reader.Close();
                query = "SELECT id_banco,nombanco FROM banco WHERE estado = 1 AND tipo = 1 ORDER BY id_banco";
                reader = vivemas.ejecutarQuery(query, connection);
                while (reader.Read())
                {
                    BancosDestinoModel bancoDestino = new BancosDestinoModel
                    {
                        idBancoDestino = reader["id_banco"].ToString(),
                        descripcionBancoDestino = reader["nombanco"].ToString()
                    };
                    data.listBancosDestinoModel.Add(bancoDestino);
                }
                reader.Close();
                respuesta.Data = new
                {
                    flag = 1,
                    data = data
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                string error = ex.StackTrace;
                respuesta.Data = new
                {
                    flag = 0
                };
                return respuesta;
            }
            finally
            {
                reader.Close();
                connection.Close();
            }
        }

        /// <summary>
        /// funcion encargada de cargar los tipos de contacto
        /// </summary>        
        /// <returns><c>json</c> con los tipos de contacto</returns>
        public ActionResult cargarTiposContacto()
        {
            try
            {
                return Json(db.tiposcontacto
                    .Select(tco => new
                    {
                        id = tco.tipocontacto_id,
                        text = tco.tipocontacto_descripcion
                    }));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de cargar las cuentas de destino
        /// </summary> 
        /// <param name="idBanco">id del banco</param>
        /// <returns><c>json</c> con las cuentas de destino</returns>
        public ActionResult CargarCuentasDestino(string idBanco)
        {
            respuesta = new JsonResult();
            try
            {
                FormPagosData data = new FormPagosData();
                data.listCuentasBancoDestinoModel = new List<CuentasBancoDestinoModel>();
                connection = vivemas.iniciarConexion();
                query = "SELECT id_bnccnt, nrocuenta FROM bcuenta WHERE estado = 1 AND id_banco = '" + idBanco + "'";
                reader = vivemas.ejecutarQuery(query, connection);
                while (reader.Read())
                {
                    CuentasBancoDestinoModel cuentaBancoDestino = new CuentasBancoDestinoModel
                    {
                        idCuentaBancoDestino = int.Parse(reader["id_bnccnt"].ToString()),
                        nroCuentaBancoDestino = reader["nrocuenta"].ToString()
                    };
                    data.listCuentasBancoDestinoModel.Add(cuentaBancoDestino);
                }
                reader.Close();
                respuesta.Data = new
                {
                    flag = 1,
                    data = data
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
            finally
            {
                reader.Close();
                connection.Close();
            }
        }

        /// <summary>
        /// funcion encargada de validar los numeros de operacion
        /// </summary>        
        /// <returns><c>json</c> con las respuesta de la validacion</returns>
        public ActionResult ValidarNroOperacion(string nroOperacion)
        {
            try
            {
                transacciones transaccion = db.transacciones.Where(tra => tra.transaccion_nrooperacion.Equals(nroOperacion)).FirstOrDefault();
                if (transaccion == null)
                {
                    respuesta = new JsonResult();
                    respuesta.Data = new
                    {
                        flag = 1
                    };
                    return respuesta;
                }
                else
                {
                    respuesta = new JsonResult();
                    respuesta.Data = new
                    {
                        flag = 2
                    };
                    return respuesta;
                }
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

        /// <summary>
        /// funcion encargada de calcular los pagos de una cobranza
        /// </summary>
        /// <param name="datosPagoCalculo">ViewModel con los datos del pago</param>
        /// <returns><c>json</c> con los datos calculados del pago</returns>        
        public ActionResult CalcularPagos(DatosPagoCalculo datosPagoCalculo)
        {
            respuesta = new JsonResult();
            try
            {
                //lista de las cuotas
                List<sp_cargarCuotas_Result> listaCuotas = db.sp_cargarCuotas(datosPagoCalculo.idContrato).ToList();
                datosPagoCalculo.cuotasPagadas = new List<LetrasPagadas>();
                int contador = 0;
                decimal descuentoCuota = 0;
                decimal mermita = 0;
                decimal montoSobrante = 0;
                decimal montoPago = 0;
                decimal montoDescuento = 0;

                if (datosPagoCalculo.montoDescuento != null)//si el pago tiene descuento
                {
                    montoDescuento = datosPagoCalculo.montoDescuento.Value;
                    //datosPagoCalculo.montoPago = montoPago + montoDescuento;                    
                    //datosPagoCalculo.montoPago = datosPagoCalculo.montoPago - montoDescuento;
                    if (datosPagoCalculo.montoPago != 0) //si el monto pagado es diferente de 0, si es cero significa que es una regularizacion de descuento
                    {
                        montoPago = datosPagoCalculo.montoPago + montoDescuento;
                    }
                    else
                    {
                        montoPago = datosPagoCalculo.montoPago;
                    }

                    foreach (var itemCuota in listaCuotas)//se calcula cuantas cuotas cubre el monto ingresado con el descuento
                    {
                        montoPago = montoPago - (decimal)(itemCuota.cuota_monto - itemCuota.cuota_montopagado);
                        if (montoPago > 0)
                        {
                            contador++;
                        }
                        else
                        {
                            contador++;
                            break;
                        }
                    }
                    descuentoCuota = montoDescuento / contador;//se prorratea el descuento entre las cuotas
                    descuentoCuota = decimal.Parse(descuentoCuota.ToString("N2"));
                    mermita = montoDescuento - (descuentoCuota * contador);//es el sobrante de la division generalmente deben ser centavos
                }

                foreach (var itemCuota in listaCuotas)//recorriendo las cuotas
                {
                    ///se calcula el valor del pago de esa cuota, a veces la cuota esta pagada previamente
                    ///por lo cual se calcula cuanto sobra de esa cuota
                    decimal sobranteCuota = (decimal)(itemCuota.cuota_monto - itemCuota.cuota_montopagado);
                    if (sobranteCuota <= descuentoCuota) //si el sobrante es menor o igual que el descuento
                    {
                        if (datosPagoCalculo.montoPago == 0)//si el monto a pagar es 0 
                        {
                            decimal montoDescuentoTotal = (descuentoCuota + montoSobrante + mermita); //se calcula el monto total de descuento (descuento + merma + monto sobrante del descuento)
                            //decimal montoNetoCuota = sobranteCuota - montoDescuentoTotal;
                            if (sobranteCuota >= montoDescuentoTotal) //si el monto de la cuota es mayor o igual que el monto total del descuento
                            {

                                LetrasPagadas letrapagadad = new LetrasPagadas
                                {
                                    nroCuota = itemCuota.cuota_numeracion,
                                    fechaVencimientoCuota = itemCuota.cuota_fechavencimiento,
                                    montoPagado = datosPagoCalculo.montoPago + montoDescuentoTotal,
                                    montoPendiente = sobranteCuota - (datosPagoCalculo.montoPago + montoDescuentoTotal),
                                    montoDescuento = montoDescuentoTotal,
                                    montoPagadoDescuento = datosPagoCalculo.montoPago,
                                    descripcionPago = util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, datosPagoCalculo.montoPago, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto)
                                };
                                datosPagoCalculo.cuotasPagadas.Add(letrapagadad);
                                break;
                            }
                            else
                            {
                                descuentoCuota = montoDescuentoTotal - sobranteCuota;
                                LetrasPagadas letrapagadad = new LetrasPagadas
                                {
                                    nroCuota = itemCuota.cuota_numeracion,
                                    fechaVencimientoCuota = itemCuota.cuota_fechavencimiento,
                                    montoPagado = sobranteCuota,
                                    montoDescuento = sobranteCuota,
                                    montoPagadoDescuento = 0,
                                    montoPendiente = 0,
                                    descripcionPago = util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto)
                                };
                                datosPagoCalculo.cuotasPagadas.Add(letrapagadad);
                            }
                            montoSobrante = 0;
                            mermita = 0;//se elimina la merma
                        }
                        else
                        {
                            montoSobrante = descuentoCuota - sobranteCuota;
                            //datosPagoCalculo.montoPago = datosPagoCalculo.montoPago - sobranteCuota;
                            LetrasPagadas letrapagada = new LetrasPagadas
                            {
                                nroCuota = itemCuota.cuota_numeracion,
                                fechaVencimientoCuota = itemCuota.cuota_fechavencimiento,
                                montoPagado = sobranteCuota,
                                montoDescuento = sobranteCuota,
                                montoPagadoDescuento = sobranteCuota,
                                montoPendiente = 0,
                                descripcionPago = util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto)
                            };
                            datosPagoCalculo.cuotasPagadas.Add(letrapagada);
                        }
                    }
                    else
                    {
                        decimal montoDescuentoTotal = (descuentoCuota + montoSobrante + mermita);
                        decimal montoNetoCuota = sobranteCuota - montoDescuentoTotal;
                        if (montoNetoCuota >= datosPagoCalculo.montoPago)
                        {

                            LetrasPagadas letrapagada = new LetrasPagadas
                            {
                                nroCuota = itemCuota.cuota_numeracion,
                                fechaVencimientoCuota = itemCuota.cuota_fechavencimiento,
                                montoPagado = datosPagoCalculo.montoPago + montoDescuentoTotal,
                                montoPendiente = sobranteCuota - (datosPagoCalculo.montoPago + montoDescuentoTotal),
                                montoDescuento = montoDescuentoTotal,
                                montoPagadoDescuento = datosPagoCalculo.montoPago,
                                descripcionPago = util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, datosPagoCalculo.montoPago, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto)
                            };
                            datosPagoCalculo.cuotasPagadas.Add(letrapagada);
                            break;
                        }
                        else
                        {
                            datosPagoCalculo.montoPago = datosPagoCalculo.montoPago - montoNetoCuota;
                            LetrasPagadas letrapagada = new LetrasPagadas
                            {
                                nroCuota = itemCuota.cuota_numeracion,
                                fechaVencimientoCuota = itemCuota.cuota_fechavencimiento,
                                montoPagado = sobranteCuota,
                                montoDescuento = montoDescuentoTotal,
                                montoPagadoDescuento = montoNetoCuota,
                                montoPendiente = 0,
                                descripcionPago = util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto)
                            };
                            datosPagoCalculo.cuotasPagadas.Add(letrapagada);
                        }
                        montoSobrante = 0;
                        mermita = 0;
                    }
                }
                if (datosPagoCalculo.montoMora != null)//si hay mora
                {
                    LetrasPagadas letrapagada = new LetrasPagadas
                    {
                        nroCuota = "MORA",
                        fechaVencimientoCuota = "-",
                        montoPagado = (decimal)datosPagoCalculo.montoMora,
                        montoPendiente = 0,
                        montoDescuento = 0,
                        montoPagadoDescuento = 0,
                        descripcionPago = "MORA"
                    };
                    datosPagoCalculo.cuotasPagadas.Add(letrapagada);
                }
                respuesta.Data = new
                {
                    flag = 1,///OK
                    datosPagoCalculo
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                respuesta.Data = new
                {
                    flag = 0//ERROR
                };
                return respuesta;
            }
        }

        /// <summary>
        /// funcion encargada de aplicar los pagos en la base de datos
        /// </summary>
        /// <param name="pago">ViewModel con la informacion del pago</param>
        /// <returns><c>json</c> con la respuesta de la operacion</returns>
        public ActionResult RealizarPago(PagoModelo pago)
        {
            respuesta = new JsonResult();
            try
            {

                using (TransactionScope transaction = new TransactionScope())//crea una instancia de transaccion 
                {

                  

                    long empresaId = long.Parse(Session["empresaId"].ToString());
                    long usuarioId = long.Parse(Session["usuario"].ToString());
                    DateTime fechaDeposito = DateTime.Parse(pago.fechaDeposito);
                    contratos contrato = db.contratos.Find(pago.idContrato);
                    anexoscontratocotizacion anexo = db.anexoscontratocotizacion.Where(aco => aco.anexocontratocotizacion_contrato_id == pago.idContrato).FirstOrDefault();
                    cotizaciones cotizacion = db.cotizaciones.Find(anexo.anexocontratocotizacion_cotizacion_id);
                    tiposcambio tipocambio = db.tiposcambio.Where(tca => tca.tipocambio_fecha >= fechaDeposito || tca.tipocambio_fecha <= fechaDeposito).OrderByDescending(tca => tca.tipocambio_fecha).FirstOrDefault();
                    int contador = 0;
                    decimal descuentoCuota = 0;
                    decimal mermita = 0;
                    decimal montoSobrante = 0;
                    decimal montoPago = 0;
                    decimal montoDescuento = 0;
                    List<sp_cargarCuotas_Result> listaCuotas = db.sp_cargarCuotas(pago.idContrato).ToList();
                    transacciones transaccionPago = new transacciones
                    {
                        transaccion_empresa_id = empresaId,
                        transaccion_tipotransaccion_id = 4, //PAGO CUOTA
                        transaccion_tipomovimiento_id = 1, //INGRESO
                        transaccion_estadotransaccion_id = 2, //VALIDADO
                        transaccion_tipometodopago_id = pago.idMetodoPago,
                        transaccion_banco_id = pago.bancoDestino,
                        transaccion_cuentabanco_id = pago.nroCuentaBancoDestino,
                        transaccion_moneda_id = cotizacion.cotizacion_moneda_id,
                        transaccion_tipocambio_id = cotizacion.cotizacion_tipocambio_id,
                        transaccion_evento_id = pago.idEvento,
                        transaccion_nrooperacion = pago.nroOperacion,
                        transaccion_bancoorigen = pago.bancoOrigen,
                        transaccion_monto = pago.montoPago,
                        transaccion_fecha = DateTime.Now,
                        transaccion_fechadeposito = DateTime.Parse(pago.fechaDeposito),
                        transaccion_tipocambio_fecha = DateTime.Parse(pago.fechaDeposito),
                        transaccion_tipocambio_venta = tipocambio.tipocambio_venta,
                        transaccion_fechacreacion = DateTime.Now,
                        transaccion_usuariocreacion = usuarioId,
                        transaccion_estadoemision = false
                    };
                    if (pago.montoDescuento != null)
                    {
                        //montoDescuento = (decimal)(pago.montoPago * pago.montoDescuento) / 100;
                        montoDescuento = pago.montoDescuento.Value;
                        transaccionPago.transaccion_montototaldescuento = montoDescuento;
                        if (pago.montoPago != 0)
                        {
                            montoPago = pago.montoPago + montoDescuento;
                        }
                        else
                        {
                            montoPago = pago.montoPago;
                        }
                        foreach (var itemCuota in listaCuotas)
                        {
                            montoPago = montoPago - (decimal)(itemCuota.cuota_monto - itemCuota.cuota_montopagado);
                            if (montoPago > 0)
                            {
                                contador++;
                            }
                            else
                            {
                                contador++;
                                break;
                            }
                        }
                        descuentoCuota = montoDescuento / contador;
                        descuentoCuota = decimal.Parse(descuentoCuota.ToString("N2"));
                        mermita = montoDescuento - (descuentoCuota * contador);
                    }
                    transaccionPago.transaccion_montonetototal = pago.montoPago;
                    db.transacciones.Add(transaccionPago);
                    db.SaveChanges();
                    series serie = db.series.Find(9);
                    transaccionPago.transaccion_numeracion = serie.serie_numeracion.Value.ToString("D8");
                    db.Entry(transaccionPago).State = EntityState.Modified;
                    serie.serie_numeracion = serie.serie_numeracion + 1;
                    db.Entry(serie).State = EntityState.Modified;
                    db.SaveChanges();
                    if (pago.idEvento != null)
                    {
                        eventos evento = db.eventos.Find(pago.idEvento);
                        evento.evento_estado = false;
                        evento.evento_estadoevento_id = 3;//TERMINADO
                        db.Entry(evento).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    List<pagos> listaPagos = new List<pagos>();

                    

                    foreach (var itemCuota in listaCuotas) //LISTA DE CUOTAS DESDE LA DB
                    {
                        decimal sobranteCuota = (decimal)(itemCuota.cuota_monto - itemCuota.cuota_montopagado);
                        cuotas cuota = db.cuotas.Find(itemCuota.cuota_id);
                        pagos nuevoPago;
                        if (sobranteCuota <= descuentoCuota)
                        {
                            if (pago.montoPago == 0)
                            {
                                decimal montoDescuentoTotal = (descuentoCuota + montoSobrante + mermita);
                                if (sobranteCuota >= montoDescuentoTotal)
                                {
                                    String detalle = "";
                                    cuota.cuota_montopagado = cuota.cuota_montopagado + montoDescuentoTotal;
                                    if (cuota.cuota_monto == cuota.cuota_montopagado)
                                    {
                                        cuota.cuota_estado = false;
                                    }


                                    foreach (var item in pago.detallePagos)
                                    {
                                        if (itemCuota.cuota_id == long.Parse(item.idPago))
                                        {
                                            detalle = item.descripcion;
                                        }                                      

                                    }

                                    nuevoPago = new pagos
                                    {
                                        pago_cuota_id = cuota.cuota_id,
                                        pago_transaccion_id = transaccionPago.transaccion_id,
                                        pago_cuota_monto = pago.montoPago + montoDescuentoTotal,
                                        pago_montodescuento = montoDescuentoTotal,
                                        pago_montonetopago = pago.montoPago,
                                        pago_descripcion = pago.detallePagos==null? util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto) : descripcion(itemCuota.cuota_numeracion, pago) == null ? util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto) : descripcion(itemCuota.cuota_numeracion, pago),
                                        pago_fechacreacion = DateTime.Now,
                                        pago_usuariocreacion = usuarioId,
                                        pago_estado = true

                                    };
                                    listaPagos.Add(nuevoPago);

                                    db.pagos.AddRange(listaPagos);
                                    db.Entry(cuota).State = EntityState.Modified;
                                    db.SaveChanges();
                                    break;
                                }
                                else
                                {
                                    descuentoCuota = montoDescuentoTotal - sobranteCuota;
                                    cuota.cuota_montopagado = cuota.cuota_monto;
                                    cuota.cuota_estado = false;
                                    nuevoPago = new pagos
                                    {
                                        pago_cuota_id = cuota.cuota_id,
                                        pago_transaccion_id = transaccionPago.transaccion_id,
                                        pago_cuota_monto = sobranteCuota,
                                        pago_montodescuento = sobranteCuota,
                                        pago_montonetopago = 0,
                                        pago_descripcion = pago.detallePagos == null ? util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto) : descripcion(itemCuota.cuota_numeracion, pago) == null ? util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto) : descripcion(itemCuota.cuota_numeracion, pago),
                                        pago_fechacreacion = DateTime.Now,
                                        pago_usuariocreacion = usuarioId,
                                        pago_estado = true
                                    };
                                }
                            }
                            else
                            {
                                montoSobrante = descuentoCuota - sobranteCuota;
                                cuota.cuota_montopagado = cuota.cuota_monto;
                                cuota.cuota_estado = false;
                                nuevoPago = new pagos
                                {
                                    pago_cuota_id = cuota.cuota_id,
                                    pago_transaccion_id = transaccionPago.transaccion_id,
                                    pago_cuota_monto = sobranteCuota,
                                    pago_montodescuento = sobranteCuota,
                                    pago_montonetopago = sobranteCuota,
                                    pago_descripcion = pago.detallePagos == null ? util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto) : descripcion(itemCuota.cuota_numeracion, pago) == null ? util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto) : descripcion(itemCuota.cuota_numeracion, pago),
                                    pago_fechacreacion = DateTime.Now,
                                    pago_usuariocreacion = usuarioId,
                                    pago_estado = true
                                };
                            }

                        }
                        else
                        {
                            decimal montoDescuentoTotal = (descuentoCuota + montoSobrante + mermita);
                            decimal montoNetoCuota = sobranteCuota - montoDescuentoTotal;
                            if (montoNetoCuota >= pago.montoPago)//40 >= 30
                            {
                                cuota.cuota_montopagado = cuota.cuota_montopagado + pago.montoPago + montoDescuentoTotal;
                                if (cuota.cuota_montopagado == cuota.cuota_monto)
                                {
                                    cuota.cuota_estado = false;
                                }
                                

                                nuevoPago = new pagos
                                {
                                    pago_cuota_id = itemCuota.cuota_id,
                                    pago_transaccion_id = transaccionPago.transaccion_id,
                                    pago_cuota_monto = pago.montoPago + montoDescuentoTotal,
                                    pago_montodescuento = montoDescuentoTotal,
                                    pago_montonetopago = pago.montoPago,
                                    pago_descripcion = pago.detallePagos == null ? util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, pago.montoPago, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto) : descripcion(itemCuota.cuota_numeracion, pago) == null ? util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto) : descripcion(itemCuota.cuota_numeracion, pago),
                                    pago_fechacreacion = DateTime.Now,
                                    pago_usuariocreacion = usuarioId,
                                    pago_estado = true
                                };
                                listaPagos.Add(nuevoPago);

                                db.pagos.AddRange(listaPagos);
                                db.Entry(cuota).State = EntityState.Modified;
                                db.SaveChanges();
                                break;
                            }
                            else
                            {
                                pago.montoPago = pago.montoPago - montoNetoCuota;
                                cuota.cuota_montopagado = cuota.cuota_monto;
                                cuota.cuota_estado = false;
                                nuevoPago = new pagos
                                {
                                    pago_cuota_id = cuota.cuota_id,
                                    pago_transaccion_id = transaccionPago.transaccion_id,
                                    pago_cuota_monto = sobranteCuota,
                                    pago_montodescuento = montoDescuentoTotal,
                                    pago_montonetopago = montoNetoCuota,
                                    pago_descripcion = pago.detallePagos == null ? util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto):  descripcion(itemCuota.cuota_numeracion, pago)==null? util.SacarDescripcionPago(itemCuota.contrato_numeracion, itemCuota.cuota_numeracion, sobranteCuota, (decimal)itemCuota.cuota_monto, itemCuota.lote_manzana, itemCuota.lote_numero, itemCuota.proyecto_nombrecorto) : descripcion(itemCuota.cuota_numeracion, pago),
                                    pago_fechacreacion = DateTime.Now,
                                    pago_usuariocreacion = usuarioId,
                                    pago_estado = true
                                };
                            }
                        }
                        montoSobrante = 0;
                        mermita = 0;
                        listaPagos.Add(nuevoPago);
                        db.Entry(cuota).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    if (pago.montoMora != null)
                    {
                        pagos pagoMora = new pagos
                        {
                            pago_transaccion_id = transaccionPago.transaccion_id,
                            pago_cuota_monto = pago.montoMora,
                            pago_descripcion = "MONTO POR MORA",
                            pago_montodescuento = 0,
                            pago_montonetopago = 0,
                            pago_fechacreacion = DateTime.Now,
                            pago_usuariocreacion = usuarioId
                        };
                        listaPagos.Add(pagoMora);
                    }


                    transaction.Complete();
                }
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

        public string descripcion(string idItem, PagoModelo pago)
        {
            foreach (var item in pago.detallePagos)
            {
                if (idItem == item.idPago)
                {
                    return  item.descripcion;
                }

            }

            return "";
        }
        /// <summary>
        /// funcion encargada de cargar los eventos de cobranza
        /// </summary>
        /// <param name="contratoId">id del contrato</param>
        /// <returns><c>json</c> con los eventos </returns>
        public ActionResult cargarEventos(long contratoId)
        {
            try
            {
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 1,
                    data = db.eventos.Where(eve => eve.evento_contrato_id == contratoId).ToList().Select(eve => new
                    {
                        evento_fechacreacion = eve.evento_fechacreacion.ToString("dd/MM/yyyy hh:mm:ss tt"),
                        evento_fechapropuesta = eve.evento_fechapropuesta.ToString("dd/MM/yyyy"),
                        eve.estadosevento.estadoevento_descripcion,
                        eve.evento_estadoevento_id,
                        eve.evento_montopropuesto,
                        eve.evento_descripcion,
                        eve.usuarios.datosusuarios.datosusuario_razonsocial
                    })
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

        /// <summary>
        /// funcion encargada de cargar un unico evento que este activo
        /// </summary>
        /// <param name="contratoId">id del contrato</param>
        /// <returns></returns>
        public ActionResult cargarEvento(long contratoId)
        {
            try
            {
                respuesta = new JsonResult();
                eventos evento = db.eventos.Where(eve => eve.evento_contrato_id == contratoId && eve.evento_estado == true).FirstOrDefault();

                respuesta.Data = new
                {
                    flag = 1,
                    data = new
                    {
                        monto = evento == null ? null : evento.evento_montopropuesto,
                        idEvento = evento == null ? null : evento.evento_id.ToString()
                    }
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

        /// <summary>
        /// funcion encargada de cargar un unico evento que este activo
        /// </summary>
        /// <param name="data">ViewModel con la informacion de un evento de cobranza</param>
        /// <returns><c>json</c> con los resultados de la operacion</returns>
        public ActionResult GuardarEvento(EventoModel data)
        {
            try
            {
                using (TransactionScope transaction = new TransactionScope())
                {
                    long usuarioId = long.Parse(Session["usuario"].ToString());
                    eventos evento = db.eventos.Where(eve => eve.evento_contrato_id == data.eventoContratoId && eve.evento_estado == true).FirstOrDefault();
                    if (evento == null)
                    {
                        evento = new eventos
                        {
                            evento_contrato_id = data.eventoContratoId,
                            evento_descripcion = data.eventoDescripcion,
                            evento_estado = true,
                            evento_estadoevento_id = 1, //EN CURSO
                            evento_fechacreacion = DateTime.Now,
                            evento_fechapropuesta = DateTime.Parse(data.eventoFecha),
                            evento_montopropuesto = data.eventoMonto,
                            evento_usuario_id = usuarioId
                        };
                        db.eventos.Add(evento);
                        db.SaveChanges();

                        List<eventostipocontacto> listaMetodoContacto = new List<eventostipocontacto>();
                        foreach (var itemContacto in data.eventoMetodosContacto)
                        {
                            eventostipocontacto itemMetodoContacto = new eventostipocontacto
                            {
                                eventotipocontacto_evento_id = evento.evento_id,
                                eventotipocontacto_tipocontacto_id = itemContacto.contactoId
                            };
                            listaMetodoContacto.Add(itemMetodoContacto);
                        }

                        db.eventostipocontacto.AddRange(listaMetodoContacto);
                        db.SaveChanges();

                        respuesta = new JsonResult();
                        respuesta.Data = new
                        {
                            flag = 1 //success
                        };
                    }
                    else
                    {
                        evento.evento_estadoevento_id = 4;//CANCELADO
                        evento.evento_estado = false;
                        db.Entry(evento).State = EntityState.Modified;
                        eventos nuevoEvento = new eventos
                        {
                            evento_contrato_id = data.eventoContratoId,
                            evento_descripcion = data.eventoDescripcion,
                            evento_estado = true,
                            evento_estadoevento_id = 1, //EN CURSO
                            evento_fechacreacion = DateTime.Now,
                            evento_fechapropuesta = DateTime.Parse(data.eventoFecha),
                            evento_montopropuesto = data.eventoMonto,
                            evento_usuario_id = usuarioId
                        };
                        db.eventos.Add(nuevoEvento);
                        db.SaveChanges();

                        List<eventostipocontacto> listaMetodoContacto = new List<eventostipocontacto>();
                        foreach (var itemContacto in data.eventoMetodosContacto)
                        {
                            eventostipocontacto itemMetodoContacto = new eventostipocontacto
                            {
                                eventotipocontacto_evento_id = evento.evento_id,
                                eventotipocontacto_tipocontacto_id = itemContacto.contactoId
                            };
                            listaMetodoContacto.Add(itemMetodoContacto);
                        }

                        db.eventostipocontacto.AddRange(listaMetodoContacto);
                        db.SaveChanges();

                        respuesta = new JsonResult();
                        respuesta.Data = new
                        {
                            flag = 2 //Evento reemplazado
                        };
                    }
                    transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                respuesta = new JsonResult();
                respuesta.Data = new
                {
                    flag = 0 //error
                };
            }
            return respuesta;
        }

        /// <summary>
        /// funcion encargada de migrar los pagos de un cliente de la base de datos MySql (no se usa)
        /// </summary>
        /// <param name="data">ViewModel con la informacion para la migracion</param>
        /// <returns><c>json</c> con los resultados de la operacion</returns>
        public ActionResult MigrarPagosAction(DataMigracion data)
        {
            respuesta = new JsonResult();
            try
            {
                string query = "START TRANSACTION; \n\n";
                foreach (FilaPago fila in data.filas)
                {
                    query = query + "\n INSERT INTO pagoscobranza \n" +
                                    "(id_tipag,\n" +
                                    "id_proforma, monto, \n" +
                                    "usuario_creacion, fecha_creacion, pago_estado) \n" +
                                    "VALUES \n" +
                                    "('" + "E" + "', \n" +
                                    fila.idProforma + ", " + fila.Saldocash + ", \n" +
                                    Session["usuario"].ToString() + ", NOW(), 1); \n";

                    query = query + "\n SELECT @idpago:=MAX(id_pagoscobranza) as idpago FROM pagoscobranza; \n";

                    string querypagos = "\n SELECT LET.id_letra, LET.debe \n" +
                                   " FROM proforma PRO \n" +
                                   " INNER JOIN cotizacion COT ON COT.id_cot = PRO.id_cot \n" +
                                   " INNER JOIN lote LOT ON LOT.id_lote = COT.id_lote \n" +
                                   " INNER JOIN proyecto PROY ON PROY.id_proy = LOT.id_proy \n" +
                                   " INNER JOIN cuota CUO ON CUO.id_cot = COT.id_cot \n" +
                                   " INNER JOIN LETRA LET ON LET.id_cuota = CUO.id_cuota \n" +
                                   " WHERE LET.cancelo = 'N' AND PRO.id_proforma = " + fila.idProforma + "\n" +
                                   " ORDER BY CUO.fecven";
                    connection = vivemas.iniciarConexion();
                    reader = vivemas.ejecutarQuery(querypagos, connection);
                    double montoPago = fila.Saldocash;
                    List<int> idsLetra = new List<int>();
                    while (reader.Read())
                    {
                        if (montoPago > double.Parse(reader["debe"].ToString()))
                        {
                            query = query + "\n UPDATE sistema.letra \n" +
                                            "SET \n" +
                                            "cancelo = 'S', feccncl = NOW(), debe = 0, fecmod = NOW(), usumod = " + Session["usuario"].ToString() + "\n" +
                                            "WHERE id_letra =  " + reader["id_letra"].ToString() + "; \n";
                            montoPago = montoPago - double.Parse(reader["debe"].ToString());

                            idsLetra.Add(int.Parse(reader["id_letra"].ToString()));
                        }
                        else
                        {
                            double montoDebe = double.Parse(reader["debe"].ToString()) - montoPago;
                            if (montoDebe == 0)
                            {
                                query = query + "\n UPDATE sistema.letra \n" +
                                            "SET \n" +
                                            "cancelo = 'S', feccncl = NOW(), debe = " + montoDebe + ", fecmod = NOW(), usumod = " + Session["usuario"].ToString() + "\n" +
                                            "WHERE id_letra = " + reader["id_letra"].ToString() + "; \n";
                            }
                            else
                            {
                                query = query + "\n UPDATE sistema.letra \n" +
                                            "SET \n" +
                                            "cancelo = 'N', feccncl = NOW(), debe = " + Math.Round(montoDebe, 2, MidpointRounding.AwayFromZero) + ", fecmod = NOW(), usumod = " + Session["usuario"].ToString() + "\n" +
                                            "WHERE id_letra = " + reader["id_letra"].ToString() + "; \n";
                            }

                            idsLetra.Add(int.Parse(reader["id_letra"].ToString()));
                            break;
                        }
                    }
                    reader.Close();
                    foreach (int idLetra in idsLetra)
                    {
                        query = query + "\n INSERT INTO pagosletras \n" +
                        "(id_pagoscobranza, id_letra, fecha_creacion, usuario_creacion) \n" +
                        "VALUES(@idpago, " + idLetra + ", NOW(), " + Session["usuario"].ToString() + "); \n";
                    }
                }
                query = query + "\n COMMIT; \n";
                string bro = "bro";
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                respuesta.Data = new
                {
                    flag = 0
                };
            }
            return null;
        }

        /// <summary>
        /// funcion encargada de migrar un contrato desde la base de datos Mysql (no se usa)
        /// </summary>
        /// <param name="data">ViewModel con la informacion para la migracion</param>
        /// <returns><c>json</c> con los resultados de la operacion</returns>
        public ActionResult MigrarContratosAction(DataMigracion data)
        {
            try
            {
                foreach (FilaPago fila in data.filas)
                {

                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
    }
}
