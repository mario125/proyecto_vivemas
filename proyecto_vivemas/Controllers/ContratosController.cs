using MySql.Data.MySqlClient;
using proyecto_vivemas.Connection;
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
    /// <summary>
    /// Clase controlador para los contratos 
    /// <remarks>
    /// contiene metodos que permiten la migracion de contratos 
    /// esta clase puede mostrar vistas relacionadas con los contratos,
    /// </remarks>
    /// </summary>
    public class ContratosController : Controller
    {
        /// <value>
        /// Objeto que conecta el modelo con el controlador de la base de datos mysql
        /// </value>
        private vivemasDB vivemas = new vivemasDB();
        /// <value>
        /// <c>MySqlConnection</c> conexion con la base de datos 
        /// </value>
        private MySqlConnection connection;
        /// <value>
        /// <c>MySqlDataReader</c> data reader de la base de datos 
        /// </value>
        private MySqlDataReader reader;
        /// <value>
        /// <c>MySqlCommand</c> command de la base de datos 
        /// </value>
        private MySqlCommand command;
        /// <value>
        /// Objeto que conecta el modelo con el controlador 
        /// </value>
        private vivemas_dbEntities db = new vivemas_dbEntities();
        // GET: Contratos
        /// <summary>
        /// crea la instancia de la vista
        /// </summary>
        /// <returns>vista de migracion de contratos</returns>
        public ActionResult MigracionContratos()
        {
            return View();
        }
        /// <summary>
        /// funcion encargada de migrar un solo contrato
        /// </summary>
        /// <param name="contratoId">id del contrato que esta en la base de datos MySql</param>
        /// <returns><c>string</c> con un estado de respuesta</returns>
        public string migrarContratoIndividual(long contratoId)
        {
            JsonResult resultado = new JsonResult();
            try
            {
                connection = vivemas.iniciarConexion();
                //contratos contrato = db.contratos.Where(con => con.contrato_idanterior == contratoItem.idContrato).FirstOrDefault();
                if (db.contratos.Where(con => con.contrato_idanterior == contratoId).FirstOrDefault() == null)//si el contrato no existe en la base de datos realiza la migracion, sino retorna un estado de que ya existe
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        string query = "sp_obtenerDataContratoCliente";
                        command = new MySqlCommand(query, connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("proformaId", contratoId);
                        reader = command.ExecuteReader();
                        clientes contratoCliente = new clientes();
                        proformasuif proformaTitular = new proformasuif();
                        transacciones transaccionCash = new transacciones();
                        contratos contrato = new contratos();
                        anexoscontratocotizacion anexocontratocotizacion = new anexoscontratocotizacion();
                        cotizaciones cotizacionContrato = new cotizaciones();
                        while (reader.Read())
                        {
                            string id = reader["proforma_estadouif_id"].ToString();
                            //CREACION DE CONTRATO
                            contrato = new contratos
                            {
                                contrato_estadocontrato_id = 3,//APROBADDO
                                contrato_numeracion = reader["proforma_numeracion"].ToString(),
                                contrato_idanterior = long.Parse(reader["proforma_idanterior"].ToString())
                            };
                            db.contratos.Add(contrato);
                            db.SaveChanges();
                            //FIN CREACION DE CONTRATO
                            //CREACION/ACTUALIZACION DE CLIENTES
                            long idCliente = long.Parse(reader["id_cli"].ToString());
                            if (db.clientes.Where(cli => cli.cliente_codigoanterior == idCliente).FirstOrDefault() == null)
                            {

                                contratoCliente.cliente_empresa_id = 1;
                                if (reader["cliente_tipodocumento"].ToString().Equals("RUC"))
                                {
                                    contratoCliente.cliente_tipopersona_id = 2; //PERSONA JURIDICA
                                    contratoCliente.cliente_tipodocumentoidentidad_id = 4; //RUC 
                                }
                                else
                                {
                                    contratoCliente.cliente_tipopersona_id = 1; //PERSONA NATURAL
                                    contratoCliente.cliente_tipodocumentoidentidad_id = 2; //DNI
                                }
                                contratoCliente.cliente_codigoanterior = long.Parse(reader["id_cli"].ToString());
                                contratoCliente.cliente_razonsocial = reader["cliente_razonsocial"].ToString();
                                contratoCliente.cliente_nrodocumento = reader["numdoc"].ToString();
                                contratoCliente.cliente_nrocontacto = reader["cliente_nrocontacto"].ToString();
                                db.clientes.Add(contratoCliente);
                                db.SaveChanges();
                            }
                            else
                            {
                                contratoCliente = db.clientes.Where(cli => cli.cliente_codigoanterior == idCliente).FirstOrDefault();
                                if (reader["cliente_tipodocumento"].ToString().Equals("RUC"))
                                {
                                    contratoCliente.cliente_tipopersona_id = 2; //PERSONA JURIDICA
                                    contratoCliente.cliente_tipodocumentoidentidad_id = 4; //RUC 
                                }
                                else
                                {
                                    contratoCliente.cliente_tipopersona_id = 1; //PERSONA NATURAL
                                    contratoCliente.cliente_tipodocumentoidentidad_id = 2; //DNI
                                }
                                contratoCliente.cliente_razonsocial = reader["cliente_razonsocial"].ToString();
                                contratoCliente.cliente_nrodocumento = reader["numdoc"].ToString();
                                contratoCliente.cliente_nrocontacto = reader["cliente_nrocontacto"].ToString();
                                db.Entry(contratoCliente).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            //FIN CREACION/ACTUALIZACION DE CLIENTES
                        }

                        //CREACION COTIZACION
                        query = "sp_obtenerDataCotizacion";
                        command = new MySqlCommand(query, connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("proformaId", contratoId);
                        reader.Close();
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {

                            long cotizacion_proyecto_idanterior = long.Parse(reader["cotizacion_proyecto_idanterior"].ToString());
                            long cotizacion_lote_idanterior = long.Parse(reader["cotizacion_lote_idanterior"].ToString());
                            long cotizacion_promotor_idanterior = long.Parse(reader["cotizacion_promotor_idanterior"].ToString());
                            string cotizacion_moneda_idanterior = reader["cotizacion_moneda_idanterior"].ToString();
                            proyectos proyectoCotizacion = db.proyectos.Where(pro => pro.proyecto_codigoanterior == cotizacion_proyecto_idanterior).FirstOrDefault();
                            lotes loteCotizacion = db.lotes.Where(lot => lot.lote_idanterior == cotizacion_lote_idanterior).FirstOrDefault();
                            usuarios usuarioPromotorCotizacion = db.usuarios.Where(usu => usu.usuario_idanterior == cotizacion_promotor_idanterior).FirstOrDefault();
                            monedas monedaCotizacion = db.monedas.Where(mon => mon.moneda_caracter.Equals(cotizacion_moneda_idanterior)).FirstOrDefault();
                            cotizacionContrato.cotizacion_proyecto_id = proyectoCotizacion.proyecto_id;
                            cotizacionContrato.cotizacion_cliente_id = contratoCliente.cliente_id;
                            cotizacionContrato.cotizacion_lote_id = loteCotizacion.lote_id;
                            cotizacionContrato.cotizacion_promotor_id = usuarioPromotorCotizacion.usuario_id;
                            cotizacionContrato.cotizacion_moneda_id = monedaCotizacion.moneda_id;
                            cotizacionContrato.cotizacion_proyecto_nombre = proyectoCotizacion.proyecto_nombre;
                            cotizacionContrato.cotizacion_promotor_nombre = usuarioPromotorCotizacion.datosusuarios.datosusuario_razonsocial;
                            cotizacionContrato.cotizacion_tipocambio_precioventa = decimal.Parse(reader["cotizacion_tipocambio_precioventa"].ToString());
                            cotizacionContrato.cotizacion_tipocambio_fecha = DateTime.Parse(reader["cotizacion_tipocambio_fecha"].ToString());
                            cotizacionContrato.cotizacion_cliente_razonsocial = contratoCliente.cliente_razonsocial;
                            cotizacionContrato.cotizacion_lote_nombre = loteCotizacion.lote_nombre;
                            cotizacionContrato.cotizacion_lote_areatotal = loteCotizacion.lote_areatotal;
                            cotizacionContrato.cotizacion_lote_preciometro = loteCotizacion.lote_preciometro;
                            cotizacionContrato.cotizacion_preciototal = decimal.Parse(reader["cotizacion_preciototal"].ToString());
                            cotizacionContrato.cotizacion_porcentajeinicial = decimal.Parse(reader["cotizacion_porcentajeinicial"].ToString());
                            cotizacionContrato.cotizacion_montoinicial = decimal.Parse(reader["cotizacion_montoinicial"].ToString());
                            cotizacionContrato.cotizacion_porcentajefinanciado = decimal.Parse(reader["cotizacion_porcentajefinanciado"].ToString());
                            cotizacionContrato.cotizacion_montofinanciado = decimal.Parse(reader["cotizacion_montofinanciado"].ToString());
                            cotizacionContrato.cotizacion_cantidadcuotas = int.Parse(reader["cotizacion_cantidadcuotas"].ToString());
                            cotizacionContrato.cotizacion_montocuotas = decimal.Parse(reader["cotizacion_montocuotas"].ToString());
                            cotizacionContrato.cotizacion_fechainiciopagos = DateTime.Parse(reader["cotizacion_fechainiciopagos"].ToString());
                            cotizacionContrato.cotizacion_idanterior = long.Parse(reader["cotizacion_idanterior"].ToString());
                            db.cotizaciones.Add(cotizacionContrato);
                            db.SaveChanges();
                            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            anexocontratocotizacion = new anexoscontratocotizacion
                            {
                                anexocontratocotizacion_contrato_id = contrato.contrato_id,
                                anexocontratocotizacion_cotizacion_id = cotizacionContrato.cotizacion_id,
                                anexocontratocotizacion_fechapagoefectivo = DateTime.Parse(reader["anexocontratocotizacion_fechapagoefectivo"].ToString()),
                                anexocontratocotizacion_montoefectivo = decimal.Parse(reader["anexocontratocotizacion_montoefectivo"].ToString()),
                                anexocontratocotizacion_montoefectivofinanciado = decimal.Parse(reader["anexocontratocotizacion_montoefectivofinanciado"].ToString()),
                                anexocontratocotizacion_montoefectivo_cuotas = int.Parse(reader["anexocontratocotizacion_montoefectivo_cuotas"].ToString())
                            };
                            db.anexoscontratocotizacion.Add(anexocontratocotizacion);
                            db.SaveChanges();
                        }

                        //FIN CREACION COTIZACION
                        //CREACION CUOTAS
                        query = "sp_obtenerDataContratoCuotas";
                        command = new MySqlCommand(query, connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("proformaId", contratoId);
                        reader.Close();
                        reader = command.ExecuteReader();
                        List<cuotas> listaCuotas = new List<cuotas>();
                        while (reader.Read())
                        {
                            cuotas cuota = new cuotas();
                            cuota.cuota_anexocontratocotizacion_id = anexocontratocotizacion.anexocontratocotizacion_id;
                            cuota.cuota_idanterior = long.Parse(reader["cuota_idanterior"].ToString());
                            cuota.cuota_numeracion = reader["cuota_numeracion"].ToString();
                            cuota.cuota_fechavencimiento = DateTime.Parse(reader["cuota_fechavencimiento"].ToString());
                            cuota.cuota_monto = decimal.Parse(reader["cuota_monto"].ToString());
                            if (reader["cuota_numeracion"].ToString().Equals("CASH"))
                            {
                                cuota.cuota_montopagado = decimal.Parse(reader["cuota_monto"].ToString());
                                cuota.cuota_estado = false;
                            }
                            else
                            {
                                cuota.cuota_montopagado = 0;
                                cuota.cuota_estado = true;
                            }
                            listaCuotas.Add(cuota);
                        }
                        db.cuotas.AddRange(listaCuotas);
                        db.SaveChanges();

                        //FIN CREACION CUOTAS
                        //CREACION DE TRANSACCION DE CASH
                        cuotas cuotaCash = listaCuotas.Where(cuo => cuo.cuota_numeracion.Equals("CASH")).FirstOrDefault();
                        transaccionCash.transaccion_empresa_id = 1;
                        transaccionCash.transaccion_tipotransaccion_id = 3; //PAGO CASH
                        transaccionCash.transaccion_tipomovimiento_id = 1; //INGRESO
                        transaccionCash.transaccion_estadotransaccion_id = 2; //ACTIVA
                        transaccionCash.transaccion_tipometodopago_id = 1; //EFECTIVO
                        transaccionCash.transaccion_moneda_id = cotizacionContrato.cotizacion_moneda_id;
                        /*if (reader["id_moneda"].ToString().Equals("$"))
                        {
                            transaccionCash.tra
                        }*/
                        transaccionCash.transaccion_monto = anexocontratocotizacion.anexocontratocotizacion_montoefectivo;
                        transaccionCash.transaccion_fecha = cuotaCash.cuota_fechavencimiento;
                        transaccionCash.transaccion_tipocambio_fecha = cotizacionContrato.cotizacion_tipocambio_fecha;
                        transaccionCash.transaccion_idproformacomodin = contrato.contrato_idanterior;
                        db.transacciones.Add(transaccionCash);
                        db.SaveChanges();
                        pagos pagoCash = new pagos();
                        pagoCash.pago_transaccion_id = transaccionCash.transaccion_id;
                        pagoCash.pago_cuota_id = cuotaCash.cuota_id;
                        pagoCash.pago_cuota_monto = cuotaCash.cuota_monto;
                        pagoCash.pago_descripcion = "PAGO MIGRACION CASH";
                        db.pagos.Add(pagoCash);
                        db.SaveChanges();
                        //FIN CREACION DE TRANSACCION DE CASH
                        //CREACION PROFORMA TITULAR
                        query = "sp_cargarDataContratoProformaTitular";
                        command = new MySqlCommand(query, connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("proformaId", contratoId);
                        reader.Close();
                        reader = command.ExecuteReader();
                        long empresaId = 1;
                        empresas empresa = db.empresas.Find(empresaId);
                        long tipoSociedad = 0;
                        while (reader.Read())
                        {
                            string estadoCivilCodigo = reader["proformauif_estadocivil_id"].ToString();
                            string estadoCivilRepresentante = reader["proformauif_representante_estadocivil_id"].ToString();
                            estadoscivil estadocivil = db.estadoscivil.Where(eci => eci.estadocivil_codigo.Equals(estadoCivilCodigo)).FirstOrDefault();
                            estadoscivil estadocivilRepresentante = new estadoscivil();
                            tiposdocumentoidentidad tipoDocumentoTitular = db.tiposdocumentoidentidad.Find(contratoCliente.cliente_tipodocumentoidentidad_id);
                            if (contratoCliente.cliente_tipodocumentoidentidad_id == 4)//RUC
                            {
                                estadocivilRepresentante = db.estadoscivil.Where(eci => eci.estadocivil_codigo.Equals(estadoCivilRepresentante)).FirstOrDefault();
                                proformaTitular.proformauif_representante_estadocivil_id = estadocivilRepresentante.estadocivil_id;
                                proformaTitular.proformauif_representante_estadocivil_descripcion = estadocivilRepresentante.estadocivil_descripcion;
                                proformaTitular.proformauif_representante_nrocontacto = reader["proformauif_representante_nrocontacto"].ToString();
                                proformaTitular.proformauif_representante_nrodocumento = reader["proformauif_representante_nrodocumento"].ToString();
                                proformaTitular.proformauif_representante_razonsocial = reader["proformauif_representante_razonsocial"].ToString();
                            }
                            else
                            {
                                proformaTitular.proformauif_estadocivil_id = estadocivil.estadocivil_id;
                                proformaTitular.proformauif_estadocivil_descripcion = estadocivil.estadocivil_descripcion;
                                if (reader["proformauif_cliente_fechanacimiento"].ToString().Equals(""))
                                {
                                    proformaTitular.proformauif_fechanacimiento = null;
                                }
                                else
                                {
                                    proformaTitular.proformauif_fechanacimiento = DateTime.Parse(reader["proformauif_cliente_fechanacimiento"].ToString());
                                }
                            }
                            proformaTitular.proformauif_estadouif_id = 1;//USADO
                            proformaTitular.proformauif_cliente_id = contratoCliente.cliente_id;
                            proformaTitular.proformauif_transaccion_id = transaccionCash.transaccion_id;
                            proformaTitular.proformauif_tipodocumentoidentidad_id = contratoCliente.cliente_tipodocumentoidentidad_id;
                            proformaTitular.proformauif_lote_id = cotizacionContrato.cotizacion_lote_id;
                            proformaTitular.proformauif_empresa_id = empresa.empresa_id;
                            proformaTitular.proformauif_tiposociedad_id = long.Parse(reader["proformauif_tiposociedad_id"].ToString());
                            proformaTitular.proformauif_idanterior = contrato.contrato_idanterior;
                            proformaTitular.proformauif_numeracion = contrato.contrato_numeracion;
                            proformaTitular.proformauif_tipodocumentoidentidad_descripcion = tipoDocumentoTitular.tipodocumentoidentidad_nombrecorto;
                            proformaTitular.proformauif_cliente_nrodocumento = contratoCliente.cliente_nrodocumento;
                            proformaTitular.proformauif_cliente_razonsocial = contratoCliente.cliente_razonsocial;
                            proformaTitular.proformauif_cliente_direccion = reader["proformauif_cliente_direccion"].ToString();
                            proformaTitular.proformauif_cliente_referenciadomicilio = reader["proformauif_cliente_referenciadomicilio"].ToString();
                            proformaTitular.proformauif_cliente_nrocontacto = contratoCliente.cliente_nrocontacto;
                            proformaTitular.proformauif_nacionalidad = reader["proformauif_nacionalidad"].ToString();
                            proformaTitular.proformauif_empresa_nombre = empresa.empresa_nombre;
                            proformaTitular.proformauif_empresa_documento = empresa.empresa_documento;
                            proformaTitular.proformauif_empresa_direccion = empresa.empresa_direccion;
                            db.proformasuif.Add(proformaTitular);
                            db.SaveChanges();
                            anexoscontratoproforma anexocontratoproformaTitular = new anexoscontratoproforma
                            {
                                anexocontratoproforma_contrato_id = contrato.contrato_id,
                                anexocontratoproforma_proformauif_id = proformaTitular.proformauif_id,
                                anexocontratoproforma_tipopropietario_id = 1 //TITULAR
                            };
                            db.anexoscontratoproforma.Add(anexocontratoproformaTitular);
                            db.SaveChanges();
                            tipoSociedad = long.Parse(reader["proformauif_tiposociedad_id"].ToString());
                        }
                        if (tipoSociedad != 1) //DIFERENTE A INDIVIDUAL
                        {
                            if (tipoSociedad == 2) //CONYUGES
                            {
                                query = "sp_cargarDataContratoProformaConyuge";
                                command = new MySqlCommand(query, connection);
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("proformaId", contratoId);
                                reader.Close();
                                reader = command.ExecuteReader();
                                while (reader.Read())
                                {
                                    string estadoCivilCodigo = reader["proformauif_estadocivil_id"].ToString();
                                    string tipoDocumentoIdentidadConyuge = reader["proformauif_tipodocumentoidentidad_id"].ToString();
                                    estadoscivil estadocivil = db.estadoscivil.Where(eci => eci.estadocivil_codigo.Equals(estadoCivilCodigo)).FirstOrDefault();
                                    tiposdocumentoidentidad tipodocumento = db.tiposdocumentoidentidad.Where(tdi => tdi.tipodocumentoidentidad_nombrecorto.Equals(tipoDocumentoIdentidadConyuge)).FirstOrDefault();
                                    proformasuif proformaConyuge = new proformasuif();
                                    proformaConyuge.proformauif_estadouif_id = 1;//USADO                                    
                                    proformaConyuge.proformauif_transaccion_id = transaccionCash.transaccion_id;
                                    proformaConyuge.proformauif_tipodocumentoidentidad_id = tipodocumento.tipodocumentoidentidad_id;
                                    proformaConyuge.proformauif_estadocivil_id = estadocivil.estadocivil_id;
                                    proformaConyuge.proformauif_lote_id = cotizacionContrato.cotizacion_lote_id;
                                    proformaConyuge.proformauif_empresa_id = empresa.empresa_id;
                                    proformaConyuge.proformauif_tiposociedad_id = tipoSociedad;
                                    proformaConyuge.proformauif_idanterior = contrato.contrato_idanterior;
                                    proformaConyuge.proformauif_numeracion = contrato.contrato_numeracion;
                                    proformaConyuge.proformauif_tipodocumentoidentidad_descripcion = tipodocumento.tipodocumentoidentidad_nombrecorto;
                                    proformaConyuge.proformauif_cliente_nrodocumento = reader["proformauif_cliente_nrodocumento"].ToString();
                                    proformaConyuge.proformauif_cliente_razonsocial = reader["proformauif_cliente_razonsocial"].ToString();
                                    proformaConyuge.proformauif_estadocivil_descripcion = estadocivil.estadocivil_descripcion;
                                    proformaConyuge.proformauif_cliente_direccion = reader["proformauif_cliente_direccion"].ToString();
                                    proformaConyuge.proformauif_cliente_referenciadomicilio = reader["proformauif_cliente_referenciadomicilio"].ToString();
                                    proformaConyuge.proformauif_cliente_nrocontacto = reader["proformauif_cliente_nrocontacto"].ToString();
                                    if (reader["proformauif_cliente_fechanacimiento"].ToString().Equals(""))
                                    {
                                        proformaConyuge.proformauif_fechanacimiento = null;
                                    }
                                    else
                                    {
                                        proformaConyuge.proformauif_fechanacimiento = DateTime.Parse(reader["proformauif_cliente_fechanacimiento"].ToString());
                                    }
                                    proformaConyuge.proformauif_nacionalidad = reader["proformauif_nacionalidad"].ToString();
                                    proformaConyuge.proformauif_empresa_nombre = empresa.empresa_nombre;
                                    proformaConyuge.proformauif_empresa_documento = empresa.empresa_documento;
                                    proformaConyuge.proformauif_empresa_direccion = empresa.empresa_direccion;
                                    db.proformasuif.Add(proformaConyuge);
                                    db.SaveChanges();
                                    anexoscontratoproforma anexocontratoproformaConyuge = new anexoscontratoproforma
                                    {
                                        anexocontratoproforma_contrato_id = contrato.contrato_id,
                                        anexocontratoproforma_proformauif_id = proformaConyuge.proformauif_id,
                                        anexocontratoproforma_tipopropietario_id = 2 //CONYUGE
                                    };
                                    db.anexoscontratoproforma.Add(anexocontratoproformaConyuge);
                                    db.SaveChanges();
                                }
                            }
                            else //COPROPIETARIOS
                            {
                                query = "sp_cargarDataContratoProformaCoPropietario";
                                command = new MySqlCommand(query, connection);
                                command.CommandType = System.Data.CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("proformaId", contratoId);
                                reader.Close();
                                reader = command.ExecuteReader();
                                while (reader.Read())
                                {
                                    string estadoCivilCodigo = reader["proformauif_estadocivil_id"].ToString();
                                    string tipoDocumentoIdentidadConyuge = reader["proformauif_tipodocumentoidentidad_id"].ToString();
                                    estadoscivil estadocivil = db.estadoscivil.Where(eci => eci.estadocivil_codigo.Equals(estadoCivilCodigo)).FirstOrDefault();
                                    tiposdocumentoidentidad tipodocumento = db.tiposdocumentoidentidad.Where(tdi => tdi.tipodocumentoidentidad_nombrecorto.Equals(tipoDocumentoIdentidadConyuge)).FirstOrDefault();
                                    proformasuif proformaConyuge = new proformasuif();
                                    proformaConyuge.proformauif_estadouif_id = 1;//USADO                                    
                                    proformaConyuge.proformauif_transaccion_id = transaccionCash.transaccion_id;
                                    proformaConyuge.proformauif_tipodocumentoidentidad_id = tipodocumento.tipodocumentoidentidad_id;
                                    proformaConyuge.proformauif_estadocivil_id = estadocivil.estadocivil_id;
                                    proformaConyuge.proformauif_lote_id = cotizacionContrato.cotizacion_lote_id;
                                    proformaConyuge.proformauif_empresa_id = empresa.empresa_id;
                                    proformaConyuge.proformauif_tiposociedad_id = tipoSociedad;
                                    proformaConyuge.proformauif_idanterior = contrato.contrato_idanterior;
                                    proformaConyuge.proformauif_numeracion = contrato.contrato_numeracion;
                                    proformaConyuge.proformauif_tipodocumentoidentidad_descripcion = tipodocumento.tipodocumentoidentidad_nombrecorto;
                                    proformaConyuge.proformauif_cliente_nrodocumento = reader["proformauif_cliente_nrodocumento"].ToString();
                                    proformaConyuge.proformauif_cliente_razonsocial = reader["proformauif_cliente_razonsocial"].ToString();
                                    proformaConyuge.proformauif_estadocivil_descripcion = estadocivil.estadocivil_descripcion;
                                    proformaConyuge.proformauif_cliente_direccion = reader["proformauif_cliente_direccion"].ToString();
                                    proformaConyuge.proformauif_cliente_referenciadomicilio = reader["proformauif_cliente_referenciadomicilio"].ToString();
                                    proformaConyuge.proformauif_cliente_nrocontacto = reader["proformauif_cliente_nrocontacto"].ToString();
                                    if (reader["proformauif_cliente_fechanacimiento"].ToString().Equals(""))
                                    {
                                        proformaConyuge.proformauif_fechanacimiento = null;
                                    }
                                    else
                                    {
                                        proformaConyuge.proformauif_fechanacimiento = DateTime.Parse(reader["proformauif_cliente_fechanacimiento"].ToString());
                                    }
                                    proformaConyuge.proformauif_nacionalidad = reader["proformauif_nacionalidad"].ToString();
                                    proformaConyuge.proformauif_empresa_nombre = empresa.empresa_nombre;
                                    proformaConyuge.proformauif_empresa_documento = empresa.empresa_documento;
                                    proformaConyuge.proformauif_empresa_direccion = empresa.empresa_direccion;
                                    db.proformasuif.Add(proformaConyuge);
                                    db.SaveChanges();
                                    anexoscontratoproforma anexocontratoproformaConyuge = new anexoscontratoproforma
                                    {
                                        anexocontratoproforma_contrato_id = contrato.contrato_id,
                                        anexocontratoproforma_proformauif_id = proformaConyuge.proformauif_id,
                                        anexocontratoproforma_tipopropietario_id = 3 //CO-PROPIETARIO
                                    };
                                    db.anexoscontratoproforma.Add(anexocontratoproformaConyuge);
                                    db.SaveChanges();
                                }
                            }
                        }
                        reader.Close();
                        transaction.Complete();

                        //FIN CREACION PROFORMA TITULAR

                    }
                    connection.Close();
                    return "1";
                }
                else
                {
                    connection.Close();
                    return "2";
                }
            }
            catch (Exception ex)
            {
                return "0";
            }

        }

        /// <summary>
        /// Funcion encargada de migrar un grupo de contratos
        /// </summary>
        /// <remarks>
        /// esta funcion requiere que se la revise cuando migra ya que podrian ocurrir algunos errores, por lo general relacionados con los usuarios
        /// </remarks>
        /// <param name="data">lista con los ids de contratos de la base de datos MySql</param>
        /// <returns>retorna un vacio ya que no requiere una respuesta</returns>
        public ActionResult migrarContratos(DataMigracionContratos data)
        {
            long contratoaa = 0;
            try
            {
                connection = vivemas.iniciarConexion();
                foreach (dataContrato contratoItem in data.filas)
                {
                    //contratos contrato = db.contratos.Where(con => con.contrato_idanterior == contratoItem.idContrato).FirstOrDefault();
                    if (db.contratos.Where(con => con.contrato_idanterior == contratoItem.idContrato).FirstOrDefault() == null)
                    {
                        contratoaa = contratoItem.idContrato;
                        using (TransactionScope transaction = new TransactionScope())
                        {
                            string query = "sp_obtenerDataContratoCliente";
                            #region
                            command = new MySqlCommand(query, connection);
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("proformaId", contratoItem.idContrato);
                            reader = command.ExecuteReader();
                            clientes contratoCliente = new clientes();
                            proformasuif proformaTitular = new proformasuif();
                            transacciones transaccionCash = new transacciones();
                            contratos contrato = new contratos();
                            anexoscontratocotizacion anexocontratocotizacion = new anexoscontratocotizacion();
                            cotizaciones cotizacionContrato = new cotizaciones();
                            while (reader.Read())
                            {
                                string id = reader["proforma_estadouif_id"].ToString();
                                //CREACION DE CONTRATO
                                contrato = new contratos
                                {
                                    contrato_estadocontrato_id = 3,//APROBADDO
                                    contrato_numeracion = reader["proforma_numeracion"].ToString(),
                                    contrato_idanterior = long.Parse(reader["proforma_idanterior"].ToString())
                                };
                                db.contratos.Add(contrato);
                                db.SaveChanges();
                                //FIN CREACION DE CONTRATO
                                //CREACION/ACTUALIZACION DE CLIENTES
                                long idCliente = long.Parse(reader["id_cli"].ToString());
                                if (db.clientes.Where(cli => cli.cliente_codigoanterior == idCliente).FirstOrDefault() == null)
                                {

                                    contratoCliente.cliente_empresa_id = 1;
                                    if (reader["cliente_tipodocumento"].ToString().Equals("RUC"))
                                    {
                                        contratoCliente.cliente_tipopersona_id = 2; //PERSONA JURIDICA
                                        contratoCliente.cliente_tipodocumentoidentidad_id = 4; //RUC 
                                    }
                                    else
                                    {
                                        contratoCliente.cliente_tipopersona_id = 1; //PERSONA NATURAL
                                        contratoCliente.cliente_tipodocumentoidentidad_id = 2; //DNI
                                    }
                                    contratoCliente.cliente_codigoanterior = long.Parse(reader["id_cli"].ToString());
                                    contratoCliente.cliente_razonsocial = reader["cliente_razonsocial"].ToString();
                                    contratoCliente.cliente_nrodocumento = reader["numdoc"].ToString();
                                    contratoCliente.cliente_nrocontacto = reader["cliente_nrocontacto"].ToString();
                                    db.clientes.Add(contratoCliente);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    contratoCliente = db.clientes.Where(cli => cli.cliente_codigoanterior == idCliente).FirstOrDefault();
                                    if (reader["cliente_tipodocumento"].ToString().Equals("RUC"))
                                    {
                                        contratoCliente.cliente_tipopersona_id = 2; //PERSONA JURIDICA
                                        contratoCliente.cliente_tipodocumentoidentidad_id = 4; //RUC 
                                    }
                                    else
                                    {
                                        contratoCliente.cliente_tipopersona_id = 1; //PERSONA NATURAL
                                        contratoCliente.cliente_tipodocumentoidentidad_id = 2; //DNI
                                    }
                                    contratoCliente.cliente_razonsocial = reader["cliente_razonsocial"].ToString();
                                    contratoCliente.cliente_nrodocumento = reader["numdoc"].ToString();
                                    contratoCliente.cliente_nrocontacto = reader["cliente_nrocontacto"].ToString();
                                    db.Entry(contratoCliente).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                //FIN CREACION/ACTUALIZACION DE CLIENTES
                            }
                            #endregion

                            //CREACION COTIZACION
                            query = "sp_obtenerDataCotizacion";
                            #region
                            command = new MySqlCommand(query, connection);
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("proformaId", contratoItem.idContrato);
                            reader.Close();
                            reader = command.ExecuteReader();
                            while (reader.Read())
                            {

                                long cotizacion_proyecto_idanterior = long.Parse(reader["cotizacion_proyecto_idanterior"].ToString());
                                long cotizacion_lote_idanterior = long.Parse(reader["cotizacion_lote_idanterior"].ToString());
                                long cotizacion_promotor_idanterior = long.Parse(reader["cotizacion_promotor_idanterior"].ToString());
                                string cotizacion_moneda_idanterior = reader["cotizacion_moneda_idanterior"].ToString();
                                proyectos proyectoCotizacion = db.proyectos.Where(pro => pro.proyecto_codigoanterior == cotizacion_proyecto_idanterior).FirstOrDefault();
                                lotes loteCotizacion = db.lotes.Where(lot => lot.lote_idanterior == cotizacion_lote_idanterior).FirstOrDefault();
                                usuarios usuarioPromotorCotizacion = db.usuarios.Where(usu => usu.usuario_idanterior == cotizacion_promotor_idanterior).FirstOrDefault();
                                monedas monedaCotizacion = db.monedas.Where(mon => mon.moneda_caracter.Equals(cotizacion_moneda_idanterior)).FirstOrDefault();
                                cotizacionContrato.cotizacion_proyecto_id = proyectoCotizacion.proyecto_id;
                                cotizacionContrato.cotizacion_cliente_id = contratoCliente.cliente_id;
                                cotizacionContrato.cotizacion_lote_id = loteCotizacion.lote_id;
                                cotizacionContrato.cotizacion_promotor_id = usuarioPromotorCotizacion.usuario_id;
                                cotizacionContrato.cotizacion_moneda_id = monedaCotizacion.moneda_id;
                                cotizacionContrato.cotizacion_proyecto_nombre = proyectoCotizacion.proyecto_nombre;
                                cotizacionContrato.cotizacion_promotor_nombre = usuarioPromotorCotizacion.datosusuarios.datosusuario_razonsocial;
                                cotizacionContrato.cotizacion_tipocambio_precioventa = decimal.Parse(reader["cotizacion_tipocambio_precioventa"].ToString());
                                cotizacionContrato.cotizacion_tipocambio_fecha = DateTime.Parse(reader["cotizacion_tipocambio_fecha"].ToString());
                                cotizacionContrato.cotizacion_cliente_razonsocial = contratoCliente.cliente_razonsocial;
                                cotizacionContrato.cotizacion_lote_nombre = loteCotizacion.lote_nombre;
                                cotizacionContrato.cotizacion_lote_areatotal = loteCotizacion.lote_areatotal;
                                cotizacionContrato.cotizacion_lote_preciometro = loteCotizacion.lote_preciometro;
                                cotizacionContrato.cotizacion_preciototal = decimal.Parse(reader["cotizacion_preciototal"].ToString());
                                cotizacionContrato.cotizacion_porcentajeinicial = decimal.Parse(reader["cotizacion_porcentajeinicial"].ToString());
                                cotizacionContrato.cotizacion_montoinicial = decimal.Parse(reader["cotizacion_montoinicial"].ToString());
                                cotizacionContrato.cotizacion_porcentajefinanciado = decimal.Parse(reader["cotizacion_porcentajefinanciado"].ToString());
                                cotizacionContrato.cotizacion_montofinanciado = decimal.Parse(reader["cotizacion_montofinanciado"].ToString());
                                cotizacionContrato.cotizacion_cantidadcuotas = int.Parse(reader["cotizacion_cantidadcuotas"].ToString());
                                cotizacionContrato.cotizacion_montocuotas = decimal.Parse(reader["cotizacion_montocuotas"].ToString());
                                cotizacionContrato.cotizacion_fechainiciopagos = DateTime.Parse(reader["cotizacion_fechainiciopagos"].ToString());
                                cotizacionContrato.cotizacion_idanterior = long.Parse(reader["cotizacion_idanterior"].ToString());
                                db.cotizaciones.Add(cotizacionContrato);
                                db.SaveChanges();
                                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                anexocontratocotizacion = new anexoscontratocotizacion
                                {
                                    anexocontratocotizacion_contrato_id = contrato.contrato_id,
                                    anexocontratocotizacion_cotizacion_id = cotizacionContrato.cotizacion_id,
                                    anexocontratocotizacion_fechapagoefectivo = DateTime.Parse(reader["anexocontratocotizacion_fechapagoefectivo"].ToString()),
                                    anexocontratocotizacion_montoefectivo = decimal.Parse(reader["anexocontratocotizacion_montoefectivo"].ToString()),
                                    anexocontratocotizacion_montoefectivofinanciado = decimal.Parse(reader["anexocontratocotizacion_montoefectivofinanciado"].ToString()),
                                    anexocontratocotizacion_montoefectivo_cuotas = int.Parse(reader["anexocontratocotizacion_montoefectivo_cuotas"].ToString())
                                };
                                db.anexoscontratocotizacion.Add(anexocontratocotizacion);
                                db.SaveChanges();
                            }
                            #endregion
                            //FIN CREACION COTIZACION

                            //CREACION CUOTAS
                            query = "sp_obtenerDataContratoCuotas";
                            #region
                            command = new MySqlCommand(query, connection);
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("proformaId", contratoItem.idContrato);
                            reader.Close();
                            reader = command.ExecuteReader();
                            List<cuotas> listaCuotas = new List<cuotas>();
                            while (reader.Read())
                            {
                                cuotas cuota = new cuotas();
                                cuota.cuota_anexocontratocotizacion_id = anexocontratocotizacion.anexocontratocotizacion_id;
                                cuota.cuota_idanterior = long.Parse(reader["cuota_idanterior"].ToString());
                                cuota.cuota_numeracion = reader["cuota_numeracion"].ToString();
                                cuota.cuota_fechavencimiento = DateTime.Parse(reader["cuota_fechavencimiento"].ToString());
                                cuota.cuota_monto = decimal.Parse(reader["cuota_monto"].ToString());
                                if (reader["cuota_numeracion"].ToString().Equals("CASH"))
                                {
                                    cuota.cuota_montopagado = decimal.Parse(reader["cuota_monto"].ToString());
                                    cuota.cuota_estado = false;
                                }
                                else
                                {
                                    cuota.cuota_montopagado = 0;
                                    cuota.cuota_estado = true;
                                }
                                listaCuotas.Add(cuota);
                            }
                            db.cuotas.AddRange(listaCuotas);
                            db.SaveChanges();

                            //FIN CREACION CUOTAS
                            //CREACION DE TRANSACCION DE CASH
                            cuotas cuotaCash = listaCuotas.Where(cuo => cuo.cuota_numeracion.Equals("CASH")).FirstOrDefault();
                            transaccionCash.transaccion_empresa_id = 1;
                            transaccionCash.transaccion_tipotransaccion_id = 3; //PAGO CASH
                            transaccionCash.transaccion_tipomovimiento_id = 1; //INGRESO
                            transaccionCash.transaccion_estadotransaccion_id = 2; //ACTIVA
                            transaccionCash.transaccion_tipometodopago_id = 1; //EFECTIVO
                            transaccionCash.transaccion_moneda_id = cotizacionContrato.cotizacion_moneda_id;
                            /*if (reader["id_moneda"].ToString().Equals("$"))
                            {
                                transaccionCash.tra
                            }*/
                            transaccionCash.transaccion_monto = anexocontratocotizacion.anexocontratocotizacion_montoefectivo;
                            transaccionCash.transaccion_fecha = cuotaCash.cuota_fechavencimiento;
                            transaccionCash.transaccion_tipocambio_fecha = cotizacionContrato.cotizacion_tipocambio_fecha;
                            transaccionCash.transaccion_idproformacomodin = contrato.contrato_idanterior;
                            db.transacciones.Add(transaccionCash);
                            db.SaveChanges();
                            pagos pagoCash = new pagos();
                            pagoCash.pago_transaccion_id = transaccionCash.transaccion_id;
                            pagoCash.pago_cuota_id = cuotaCash.cuota_id;
                            pagoCash.pago_cuota_monto = cuotaCash.cuota_monto;
                            pagoCash.pago_descripcion = "PAGO MIGRACION CASH";
                            db.pagos.Add(pagoCash);
                            db.SaveChanges();
                            //FIN CREACION DE TRANSACCION DE CASH
                            #endregion

                            //CREACION PROFORMA TITULAR
                            query = "sp_cargarDataContratoProformaTitular";
                            #region
                            command = new MySqlCommand(query, connection);
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("proformaId", contratoItem.idContrato);
                            reader.Close();
                            reader = command.ExecuteReader();
                            long empresaId = 1;
                            empresas empresa = db.empresas.Find(empresaId);
                            long tipoSociedad = 0;
                            while (reader.Read())
                            {
                                string estadoCivilCodigo = reader["proformauif_estadocivil_id"].ToString();
                                string estadoCivilRepresentante = reader["proformauif_representante_estadocivil_id"].ToString();
                                estadoscivil estadocivil = db.estadoscivil.Where(eci => eci.estadocivil_codigo.Equals(estadoCivilCodigo)).FirstOrDefault();
                                estadoscivil estadocivilRepresentante = new estadoscivil();
                                tiposdocumentoidentidad tipoDocumentoTitular = db.tiposdocumentoidentidad.Find(contratoCliente.cliente_tipodocumentoidentidad_id);
                                if (contratoCliente.cliente_tipodocumentoidentidad_id == 4)//RUC
                                {
                                    estadocivilRepresentante = db.estadoscivil.Where(eci => eci.estadocivil_codigo.Equals(estadoCivilRepresentante)).FirstOrDefault();
                                    proformaTitular.proformauif_representante_estadocivil_id = estadocivilRepresentante.estadocivil_id;
                                    proformaTitular.proformauif_representante_estadocivil_descripcion = estadocivilRepresentante.estadocivil_descripcion;
                                    proformaTitular.proformauif_representante_nrocontacto = reader["proformauif_representante_nrocontacto"].ToString();
                                    proformaTitular.proformauif_representante_nrodocumento = reader["proformauif_representante_nrodocumento"].ToString();
                                    proformaTitular.proformauif_representante_razonsocial = reader["proformauif_representante_razonsocial"].ToString();
                                }
                                else
                                {
                                    proformaTitular.proformauif_estadocivil_id = estadocivil.estadocivil_id;
                                    proformaTitular.proformauif_estadocivil_descripcion = estadocivil.estadocivil_descripcion;
                                    if (reader["proformauif_cliente_fechanacimiento"].ToString().Equals(""))
                                    {
                                        proformaTitular.proformauif_fechanacimiento = null;
                                    }
                                    else
                                    {
                                        proformaTitular.proformauif_fechanacimiento = DateTime.Parse(reader["proformauif_cliente_fechanacimiento"].ToString());
                                    }
                                }
                                proformaTitular.proformauif_estadouif_id = 1;//USADO
                                proformaTitular.proformauif_cliente_id = contratoCliente.cliente_id;
                                proformaTitular.proformauif_transaccion_id = transaccionCash.transaccion_id;
                                proformaTitular.proformauif_tipodocumentoidentidad_id = contratoCliente.cliente_tipodocumentoidentidad_id;
                                proformaTitular.proformauif_lote_id = cotizacionContrato.cotizacion_lote_id;
                                proformaTitular.proformauif_empresa_id = empresa.empresa_id;
                                proformaTitular.proformauif_tiposociedad_id = long.Parse(reader["proformauif_tiposociedad_id"].ToString());
                                proformaTitular.proformauif_idanterior = contrato.contrato_idanterior;
                                proformaTitular.proformauif_numeracion = contrato.contrato_numeracion;
                                proformaTitular.proformauif_tipodocumentoidentidad_descripcion = tipoDocumentoTitular.tipodocumentoidentidad_nombrecorto;
                                proformaTitular.proformauif_cliente_nrodocumento = contratoCliente.cliente_nrodocumento;
                                proformaTitular.proformauif_cliente_razonsocial = contratoCliente.cliente_razonsocial;
                                proformaTitular.proformauif_cliente_direccion = reader["proformauif_cliente_direccion"].ToString();
                                proformaTitular.proformauif_cliente_referenciadomicilio = reader["proformauif_cliente_referenciadomicilio"].ToString();
                                proformaTitular.proformauif_cliente_nrocontacto = contratoCliente.cliente_nrocontacto;
                                proformaTitular.proformauif_nacionalidad = reader["proformauif_nacionalidad"].ToString();
                                proformaTitular.proformauif_empresa_nombre = empresa.empresa_nombre;
                                proformaTitular.proformauif_empresa_documento = empresa.empresa_documento;
                                proformaTitular.proformauif_empresa_direccion = empresa.empresa_direccion;
                                db.proformasuif.Add(proformaTitular);
                                db.SaveChanges();
                                anexoscontratoproforma anexocontratoproformaTitular = new anexoscontratoproforma
                                {
                                    anexocontratoproforma_contrato_id = contrato.contrato_id,
                                    anexocontratoproforma_proformauif_id = proformaTitular.proformauif_id,
                                    anexocontratoproforma_tipopropietario_id = 1 //TITULAR
                                };
                                db.anexoscontratoproforma.Add(anexocontratoproformaTitular);
                                db.SaveChanges();
                                tipoSociedad = long.Parse(reader["proformauif_tiposociedad_id"].ToString());
                            }
                            if (tipoSociedad != 1) //DIFERENTE A INDIVIDUAL
                            {
                                if (tipoSociedad == 2) //CONYUGES
                                {
                                    query = "sp_cargarDataContratoProformaConyuge";
                                    command = new MySqlCommand(query, connection);
                                    command.CommandType = System.Data.CommandType.StoredProcedure;
                                    command.Parameters.AddWithValue("proformaId", contratoItem.idContrato);
                                    reader.Close();
                                    reader = command.ExecuteReader();
                                    while (reader.Read())
                                    {
                                        string estadoCivilCodigo = reader["proformauif_estadocivil_id"].ToString();
                                        string tipoDocumentoIdentidadConyuge = reader["proformauif_tipodocumentoidentidad_id"].ToString();
                                        estadoscivil estadocivil = db.estadoscivil.Where(eci => eci.estadocivil_codigo.Equals(estadoCivilCodigo)).FirstOrDefault();
                                        tiposdocumentoidentidad tipodocumento = db.tiposdocumentoidentidad.Where(tdi => tdi.tipodocumentoidentidad_nombrecorto.Equals(tipoDocumentoIdentidadConyuge)).FirstOrDefault();
                                        proformasuif proformaConyuge = new proformasuif();
                                        proformaConyuge.proformauif_estadouif_id = 1;//USADO                                    
                                        proformaConyuge.proformauif_transaccion_id = transaccionCash.transaccion_id;
                                        proformaConyuge.proformauif_tipodocumentoidentidad_id = tipodocumento.tipodocumentoidentidad_id;
                                        proformaConyuge.proformauif_estadocivil_id = estadocivil.estadocivil_id;
                                        proformaConyuge.proformauif_lote_id = cotizacionContrato.cotizacion_lote_id;
                                        proformaConyuge.proformauif_empresa_id = empresa.empresa_id;
                                        proformaConyuge.proformauif_tiposociedad_id = tipoSociedad;
                                        proformaConyuge.proformauif_idanterior = contrato.contrato_idanterior;
                                        proformaConyuge.proformauif_numeracion = contrato.contrato_numeracion;
                                        proformaConyuge.proformauif_tipodocumentoidentidad_descripcion = tipodocumento.tipodocumentoidentidad_nombrecorto;
                                        proformaConyuge.proformauif_cliente_nrodocumento = reader["proformauif_cliente_nrodocumento"].ToString();
                                        proformaConyuge.proformauif_cliente_razonsocial = reader["proformauif_cliente_razonsocial"].ToString();
                                        proformaConyuge.proformauif_estadocivil_descripcion = estadocivil.estadocivil_descripcion;
                                        proformaConyuge.proformauif_cliente_direccion = reader["proformauif_cliente_direccion"].ToString();
                                        proformaConyuge.proformauif_cliente_referenciadomicilio = reader["proformauif_cliente_referenciadomicilio"].ToString();
                                        proformaConyuge.proformauif_cliente_nrocontacto = reader["proformauif_cliente_nrocontacto"].ToString();
                                        if (reader["proformauif_cliente_fechanacimiento"].ToString().Equals(""))
                                        {
                                            proformaConyuge.proformauif_fechanacimiento = null;
                                        }
                                        else
                                        {
                                            proformaConyuge.proformauif_fechanacimiento = DateTime.Parse(reader["proformauif_cliente_fechanacimiento"].ToString());
                                        }
                                        proformaConyuge.proformauif_nacionalidad = reader["proformauif_nacionalidad"].ToString();
                                        proformaConyuge.proformauif_empresa_nombre = empresa.empresa_nombre;
                                        proformaConyuge.proformauif_empresa_documento = empresa.empresa_documento;
                                        proformaConyuge.proformauif_empresa_direccion = empresa.empresa_direccion;
                                        db.proformasuif.Add(proformaConyuge);
                                        db.SaveChanges();
                                        anexoscontratoproforma anexocontratoproformaConyuge = new anexoscontratoproforma
                                        {
                                            anexocontratoproforma_contrato_id = contrato.contrato_id,
                                            anexocontratoproforma_proformauif_id = proformaConyuge.proformauif_id,
                                            anexocontratoproforma_tipopropietario_id = 2 //CONYUGE
                                        };
                                        db.anexoscontratoproforma.Add(anexocontratoproformaConyuge);
                                        db.SaveChanges();
                                    }

                                }
                                else //COPROPIETARIOS
                                {
                                    query = "sp_cargarDataContratoProformaCoPropietario";
                                    command = new MySqlCommand(query, connection);
                                    command.CommandType = System.Data.CommandType.StoredProcedure;
                                    command.Parameters.AddWithValue("proformaId", contratoItem.idContrato);
                                    reader.Close();
                                    reader = command.ExecuteReader();
                                    while (reader.Read())
                                    {
                                        string estadoCivilCodigo = reader["proformauif_estadocivil_id"].ToString();
                                        string tipoDocumentoIdentidadConyuge = reader["proformauif_tipodocumentoidentidad_id"].ToString();
                                        estadoscivil estadocivil = db.estadoscivil.Where(eci => eci.estadocivil_codigo.Equals(estadoCivilCodigo)).FirstOrDefault();
                                        tiposdocumentoidentidad tipodocumento = db.tiposdocumentoidentidad.Where(tdi => tdi.tipodocumentoidentidad_nombrecorto.Equals(tipoDocumentoIdentidadConyuge)).FirstOrDefault();
                                        proformasuif proformaConyuge = new proformasuif();
                                        proformaConyuge.proformauif_estadouif_id = 1;//USADO                                    
                                        proformaConyuge.proformauif_transaccion_id = transaccionCash.transaccion_id;
                                        proformaConyuge.proformauif_tipodocumentoidentidad_id = tipodocumento.tipodocumentoidentidad_id;
                                        proformaConyuge.proformauif_estadocivil_id = estadocivil.estadocivil_id;
                                        proformaConyuge.proformauif_lote_id = cotizacionContrato.cotizacion_lote_id;
                                        proformaConyuge.proformauif_empresa_id = empresa.empresa_id;
                                        proformaConyuge.proformauif_tiposociedad_id = tipoSociedad;
                                        proformaConyuge.proformauif_idanterior = contrato.contrato_idanterior;
                                        proformaConyuge.proformauif_numeracion = contrato.contrato_numeracion;
                                        proformaConyuge.proformauif_tipodocumentoidentidad_descripcion = tipodocumento.tipodocumentoidentidad_nombrecorto;
                                        proformaConyuge.proformauif_cliente_nrodocumento = reader["proformauif_cliente_nrodocumento"].ToString();
                                        proformaConyuge.proformauif_cliente_razonsocial = reader["proformauif_cliente_razonsocial"].ToString();
                                        proformaConyuge.proformauif_estadocivil_descripcion = estadocivil.estadocivil_descripcion;
                                        proformaConyuge.proformauif_cliente_direccion = reader["proformauif_cliente_direccion"].ToString();
                                        proformaConyuge.proformauif_cliente_referenciadomicilio = reader["proformauif_cliente_referenciadomicilio"].ToString();
                                        proformaConyuge.proformauif_cliente_nrocontacto = reader["proformauif_cliente_nrocontacto"].ToString();
                                        if (reader["proformauif_cliente_fechanacimiento"].ToString().Equals(""))
                                        {
                                            proformaConyuge.proformauif_fechanacimiento = null;
                                        }
                                        else
                                        {
                                            proformaConyuge.proformauif_fechanacimiento = DateTime.Parse(reader["proformauif_cliente_fechanacimiento"].ToString());
                                        }
                                        proformaConyuge.proformauif_nacionalidad = reader["proformauif_nacionalidad"].ToString();
                                        proformaConyuge.proformauif_empresa_nombre = empresa.empresa_nombre;
                                        proformaConyuge.proformauif_empresa_documento = empresa.empresa_documento;
                                        proformaConyuge.proformauif_empresa_direccion = empresa.empresa_direccion;
                                        db.proformasuif.Add(proformaConyuge);
                                        db.SaveChanges();
                                        anexoscontratoproforma anexocontratoproformaConyuge = new anexoscontratoproforma
                                        {
                                            anexocontratoproforma_contrato_id = contrato.contrato_id,
                                            anexocontratoproforma_proformauif_id = proformaConyuge.proformauif_id,
                                            anexocontratoproforma_tipopropietario_id = 3 //CO-PROPIETARIO
                                        };
                                        db.anexoscontratoproforma.Add(anexocontratoproformaConyuge);
                                        db.SaveChanges();
                                    }
                                }
                            }
                            reader.Close();
                            #endregion
                            transaction.Complete();
                            //FIN CREACION PROFORMA TITULAR

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        /// <summary>
        /// Funcion encargada de migrar clientes por id de cliente desde la base de datos MySql
        /// </summary>
        /// <remarks>
        /// esta funcion ya no se usa
        /// </remarks>
        /// <param name="data">listado de clientes </param>
        /// <returns>vacio ya que no se requiere respuesta</returns>
        public ActionResult migrarClientes(DataMigracionContratos data)
        {
            try
            {
                connection = vivemas.iniciarConexion();
                foreach (dataContrato contratoItem in data.filas)
                {
                    if (db.contratos.Where(con => con.contrato_idanterior == contratoItem.idContrato).FirstOrDefault() == null)
                    {
                        using (TransactionScope transaction = new TransactionScope())
                        {
                            string query = "sp_obtenerDataContratoCliente";
                            #region
                            command = new MySqlCommand(query, connection);
                            command.CommandType = System.Data.CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("proformaId", contratoItem.idContrato);
                            reader = command.ExecuteReader();
                            clientes contratoCliente = new clientes();
                            while (reader.Read())
                            {
                                string id = reader["proforma_estadouif_id"].ToString();
                                //CREACION/ACTUALIZACION DE CLIENTES
                                long idCliente = long.Parse(reader["id_cli"].ToString());
                                if (db.clientes.Where(cli => cli.cliente_codigoanterior == idCliente).FirstOrDefault() == null)
                                {

                                    contratoCliente.cliente_empresa_id = 1;
                                    if (reader["cliente_tipodocumento"].ToString().Equals("RUC"))
                                    {
                                        contratoCliente.cliente_tipopersona_id = 2; //PERSONA JURIDICA
                                        contratoCliente.cliente_tipodocumentoidentidad_id = 4; //RUC 
                                    }
                                    else
                                    {
                                        contratoCliente.cliente_tipopersona_id = 1; //PERSONA NATURAL
                                        contratoCliente.cliente_tipodocumentoidentidad_id = 2; //DNI
                                    }
                                    contratoCliente.cliente_codigoanterior = long.Parse(reader["id_cli"].ToString());
                                    contratoCliente.cliente_razonsocial = reader["cliente_razonsocial"].ToString();
                                    contratoCliente.cliente_nrodocumento = reader["numdoc"].ToString();
                                    contratoCliente.cliente_nrocontacto = reader["cliente_nrocontacto"].ToString();
                                    contratoCliente.cliente_escasuarinas = true;
                                    db.clientes.Add(contratoCliente);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    contratoCliente = db.clientes.Where(cli => cli.cliente_codigoanterior == idCliente).FirstOrDefault();
                                    if (reader["cliente_tipodocumento"].ToString().Equals("RUC"))
                                    {
                                        contratoCliente.cliente_tipopersona_id = 2; //PERSONA JURIDICA
                                        contratoCliente.cliente_tipodocumentoidentidad_id = 4; //RUC 
                                    }
                                    else
                                    {
                                        contratoCliente.cliente_tipopersona_id = 1; //PERSONA NATURAL
                                        contratoCliente.cliente_tipodocumentoidentidad_id = 2; //DNI
                                    }
                                    contratoCliente.cliente_razonsocial = reader["cliente_razonsocial"].ToString();
                                    contratoCliente.cliente_nrodocumento = reader["numdoc"].ToString();
                                    contratoCliente.cliente_nrocontacto = reader["cliente_nrocontacto"].ToString();
                                    contratoCliente.cliente_escasuarinas = true;
                                    db.Entry(contratoCliente).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                //FIN CREACION/ACTUALIZACION DE CLIENTES
                            }
                            #endregion
                            reader.Close();
                            transaction.Complete();

                        }
                    }
                    
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
            return null;
        }

        public string resolverContrato(long contratoId)
        {
            try
            {
                contratos contrato = db.contratos.Where(con => con.contrato_idanterior == contratoId).FirstOrDefault();
                if (contrato == null)//contrato no existe
                {
                    return "2";
                }
                else
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        contrato.contrato_estadocontrato_id = 5;//RESUELTO
                        contrato.contrato_fecharesolucion = DateTime.Now;
                        db.Entry(contrato).State = EntityState.Modified;
                        db.SaveChanges();
                        transaction.Complete();

                    }
                    return "1";
                }
            }
            catch (Exception ex)
            {
                return "0";
            }
        }
    }
}