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
    /// <summary>
    /// Clase Controlador para los clientes
    /// contiene todos los metodos para trabajar con los clientes
    /// </summary>
    /// <remarks>
    /// esta clase puede mostrar vistas relacionadas con los clientes, puede enviar informacion de los clientes o informacion necesaria de los clientes
    /// a traves de un JsonResult
    /// </remarks>
    public class ClientesController : Controller
    {
        /// <value>
        /// Objeto que conecta el modelo con el controlador
        /// </value>
        vivemas_dbEntities db = new vivemas_dbEntities();
        /// <value>
        /// JsonResult usado para devolver informacion a la vista
        /// </value>
        JsonResult respuesta;
        
        /// <summary>crea la instancia de la vista</summary>
        /// <returns>vista de mantenimiento de clientes</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>crea la instancia de la vista</summary>
        /// <returns>vista de Planificador de actividades</returns>
        public ActionResult Planificador()
        {
            return View();
        }

        /// <summary>
        /// funcion encargada de devolver los clientes a la vista
        /// </summary>
        /// <returns><c>json</c> convertido en <c>string</c></returns>
        public string CargarClientes()
        {
            string respuestajson = "";
            try
            {
                long empresaId = long.Parse(Session["empresaId"].ToString());
                long usuarioId = long.Parse(Session["usuario"].ToString());
                usuarios usuario = db.usuarios.Find(usuarioId);
                List<object> dataClientes = new List<object>();
                List<clientes> listaClientes;
                if (usuario.roles.rol_nombre.Equals("ADMINISTRADOR") || usuario.roles.rol_nombre.Equals("GERENTE COMERCIAL") || usuario.roles.rol_nombre.Equals("JEFE DE VENTAS"))
                {
                    listaClientes = db.clientes.Where(cli => cli.cliente_empresa_id == empresaId)
                    .OrderByDescending(cli => cli.cliente_id)
                    .ToList();
                }
                else
                {
                    listaClientes = db.clientes.Where(cli => cli.cliente_empresa_id == empresaId && cli.cliente_promotor_id == usuarioId)
                    .OrderByDescending(cli => cli.cliente_id)
                    .ToList();
                }
                foreach (var cliente in listaClientes)
                {
                    object itemcliente = new
                    {
                        cliente.cliente_id,
                        cliente.cliente_nrodocumento,
                        cliente.cliente_razonsocial,
                        cliente.cliente_nrocontacto,
                        cliente.cliente_nrocontactoauxiliar,
                        cliente.cliente_email
                    };
                    dataClientes.Add(itemcliente);
                }
                respuestajson = JsonConvert.SerializeObject(dataClientes);
                return respuestajson;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        /// <summary>
        /// funcion encargada de cargar eventos y enviarlos a la vista
        /// </summary>
        /// <param name="mes">parametro de tipo <c>long</c> indicando el mes</param>
        /// <returns><c>json</c> con los eventos</returns>
        public ActionResult cargarEventos(long mes)
        {
            respuesta = new JsonResult();
            try
            {
                List<eventos> listaEventos = db.eventos.Where(eve => eve.evento_fechacreacion.Month == mes).ToList();
                List<object> listaRespuesta = new List<object>();
                foreach (var itemEvento in listaEventos)
                {
                    if (itemEvento.evento_estado == false)
                    {
                        object itemRespuesta = new
                        {
                            id = itemEvento.evento_id,
                            title = itemEvento.evento_descripcion,
                            start = itemEvento.evento_fechacreacion.ToString("yyyy-MM-dd"),
                            end = itemEvento.evento_fechapropuesta.ToString("yyyy-MM-dd"),
                            color = "red"
                        };
                        listaRespuesta.Add(itemRespuesta);
                    }
                    if (DateTime.Compare(itemEvento.evento_fechacreacion, DateTime.Now) == 0 && DateTime.Compare(itemEvento.evento_fechapropuesta, DateTime.Now) == 0)
                    {
                        object itemRespuesta = new
                        {
                            id = itemEvento.evento_id,
                            title = itemEvento.evento_descripcion,
                            start = itemEvento.evento_fechacreacion.ToString("yyyy-MM-dd"),
                            end = itemEvento.evento_fechapropuesta.ToString("yyyy-MM-dd"),
                            color = "red"
                        };
                        listaRespuesta.Add(itemRespuesta);
                    }
                    else
                    {
                        object itemRespuesta = new
                        {
                            id = itemEvento.evento_id,
                            title = itemEvento.evento_descripcion,
                            start = itemEvento.evento_fechacreacion.ToString("yyyy-MM-dd"),
                            end = itemEvento.evento_fechapropuesta.ToString("yyyy-MM-dd")
                        };
                        listaRespuesta.Add(itemRespuesta);
                    }
                }
                respuesta.Data = new
                {
                    flag = 1,
                    dataEventos = listaRespuesta
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

        /// <summary>
        /// funcion encargada de cargar los tipos de personas y enviarlos a la vista
        /// </summary>
        /// <returns><c>json</c> con los tipos de persona</returns>
        public ActionResult cargarTiposPersona()
        {
            respuesta = new JsonResult();
            try
            {
                List<object> dataRespuesta = new List<object>();
                List<tipospersona> listaTiposPersona = db.tipospersona.ToList();
                foreach (var itemTiposPersona in listaTiposPersona)
                {
                    object data = new
                    {
                        id = itemTiposPersona.tipopersona_id,
                        text = itemTiposPersona.tipopersona_descripcion
                    };
                    dataRespuesta.Add(data);
                }
                respuesta.Data = new
                {
                    results = dataRespuesta
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de cargar los tipos de documento y enviarlos a la vista
        /// </summary>
        /// <returns><c>json</c> con los tipos de documentos</returns>
        public ActionResult cargarTiposDocumento()
        {
            respuesta = new JsonResult();
            try
            {
                List<object> dataRespuesta = new List<object>();
                List<tiposdocumentoidentidad> listaTiposDocumento = db.tiposdocumentoidentidad.ToList();
                foreach (var itemTiposDocumento in listaTiposDocumento)
                {
                    object data = new
                    {
                        id = itemTiposDocumento.tipodocumentoidentidad_id,
                        text = itemTiposDocumento.tipodocumentoidentidad_nombrecorto
                    };
                    dataRespuesta.Add(data);
                }
                respuesta.Data = new
                {
                    results = dataRespuesta
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de cargar los departamentos y enviarlos a la vista
        /// </summary>
        /// <returns><c>json</c> con los departamentos</returns>
        public ActionResult cargarDepartamentos()
        {
            respuesta = new JsonResult();
            try
            {
                List<object> dataDepartamentos = new List<object>();
                List<departamentos> listaDepartamentos = db.departamentos.ToList();
                foreach (var itemDepartamento in listaDepartamentos)
                {
                    object dataDepartamento = new
                    {
                        id = itemDepartamento.departamento_id,
                        text = itemDepartamento.departamento_descripcion
                    };
                    dataDepartamentos.Add(dataDepartamento);
                }
                respuesta.Data = new
                {
                    results = dataDepartamentos
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de cargar las provincias y enviarlos a la vista
        /// </summary>
        /// <returns><c>json</c> con las provincias</returns>
        public ActionResult cargarProvincias(long idDepartamento)
        {
            respuesta = new JsonResult();
            try
            {
                List<object> dataProvincias = new List<object>();
                List<provincias> listaProvincias = db.provincias.Where(pro => pro.provincia_departamento_id == idDepartamento).ToList();

                foreach (var itemProvincia in listaProvincias)
                {
                    object dataProvincia = new
                    {
                        id = itemProvincia.provincia_id,
                        text = itemProvincia.provincia_descripcion
                    };
                    dataProvincias.Add(dataProvincia);
                }
                respuesta.Data = new
                {
                    results = dataProvincias
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de cargar los distritos y enviarlos a la vista
        /// </summary>
        /// <returns><c>json</c> con los distritos</returns>
        public ActionResult cargarDistritos(long idProvincia)
        {
            respuesta = new JsonResult();
            try
            {

                List<object> dataDistritos = new List<object>();
                List<distritos> listaDistritos = db.distritos.Where(dis => dis.distrito_provincia_id == idProvincia).ToList();

                foreach (var itemDistrito in listaDistritos)
                {
                    object dataDistrito = new
                    {
                        id = itemDistrito.distrito_id,
                        text = itemDistrito.distrito_descripcion
                    };
                    dataDistritos.Add(dataDistrito);
                }
                respuesta.Data = new
                {
                    results = dataDistritos
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de cargar los proyectos y enviarlos a la vista
        /// </summary>
        /// <returns><c>json</c> con los proyectos</returns>
        public ActionResult cargarProyectos()
        {
            respuesta = new JsonResult();
            try
            {

                List<object> dataProyectos = new List<object>();
                List<proyectos> listaProyectos = db.proyectos.ToList();

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

        /// <summary>
        /// funcion encargada de cargar los medios de contacto y enviarlos a la vista
        /// </summary>
        /// <returns><c>json</c> con los proyectos</returns>
        public ActionResult cargarMedios()
        {
            respuesta = new JsonResult();
            long usuarioId = long.Parse(Session["usuario"].ToString());
            try
            {
                List<object> dataMedios = new List<object>();
                List<medioscontacto> listaMedios = db.medioscontacto.ToList();

                foreach (var itemMedio in listaMedios)
                {
                    object dataMedio = new
                    {
                        id = itemMedio.mediocontacto_id,
                        text = itemMedio.mediocontacto_descripcion
                    };
                    dataMedios.Add(dataMedio);
                }
                respuesta.Data = new
                {
                    results = dataMedios
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de cargar los canales de contacto y enviarlos a la vista
        /// </summary>
        /// <returns><c>json</c> con los canales de contacto</returns>
        public ActionResult cargarCanales(long idMedio)
        {
            respuesta = new JsonResult();
            try
            {

                List<object> dataCanales = new List<object>();
                List<canalescontacto> listaCanales = db.canalescontacto.Where(can => can.canalcontacto_mediocontacto_id == idMedio).ToList();

                foreach (var itemCanal in listaCanales)
                {
                    object dataCanal = new
                    {
                        id = itemCanal.canalcontacto_id,
                        text = itemCanal.canalcontacto_descripcion
                    };
                    dataCanales.Add(dataCanal);
                }
                respuesta.Data = new
                {
                    results = dataCanales
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de cargar los promotores y enviarlos a la vista
        /// </summary>
        /// <returns><c>json</c> con los promotores</returns>
        public ActionResult cargarPromotores()
        {
            respuesta = new JsonResult();
            try
            {
                List<object> dataPromotores = new List<object>();
                long usuarioId = long.Parse(Session["usuario"].ToString());
                usuarios usuario = db.usuarios.Find(usuarioId);
                if (usuario.roles.rol_nombre.Equals("ADMINISTRADOR") || usuario.roles.rol_nombre.Equals("GERENTE COMERCIAL") || usuario.roles.rol_nombre.Equals("JEFE DE VENTAS"))
                {
                    List<usuarios> listaUsuarios = db.usuarios.Where(usu => usu.usuario_rol_id == 3 || usu.usuario_rol_id == 5 || usu.usuario_rol_id == 12 || usu.usuario_rol_id == 2).ToList();
                    foreach (var itemUsuario in listaUsuarios)
                    {
                        datosusuarios datoUsuario = db.datosusuarios.Find(itemUsuario.usuario_datosusuario_id);
                        object dataUsuario = new
                        {
                            id = itemUsuario.usuario_id,
                            text = datoUsuario.datosusuario_razonsocial
                        };
                        dataPromotores.Add(dataUsuario);
                    }
                }
                else
                {
                    object dataUsuario = new
                    {
                        id = usuario.usuario_id,
                        text = usuario.datosusuarios.datosusuario_razonsocial
                    };
                }
                respuesta.Data = new
                {
                    results = dataPromotores
                };
                return respuesta;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de validar el numero de telefono de un cliente que no este repetido
        /// </summary>
        /// <returns><c>json</c> con el flag del resultado</returns>
        public ActionResult validarNroContacto(string nroContacto)
        {
            respuesta = new JsonResult();
            try
            {
                int respuestaValidacion = validadorNroContacto(nroContacto);
                if (respuestaValidacion != 2)
                {
                    respuesta.Data = new
                    {
                        flag = respuestaValidacion
                    };
                    return respuesta;
                }
                else
                {
                    clientes cliente = db.clientes.Where(cli => cli.cliente_nrocontacto == nroContacto).FirstOrDefault();
                    respuesta.Data = new
                    {
                        flag = respuestaValidacion,
                        promotor = cliente.usuarios.datosusuarios.datosusuario_razonsocial
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
        /// funcion encargada de crear un nuevo cliente
        /// </summary>
        /// <param name="clienteNuevo">Viewmodel de clientes</param>
        /// <returns><c>json</c> con las respuesta de la actividad</returns>
        public ActionResult crearCliente(DataClientes clienteNuevo)
        {
            respuesta = new JsonResult();
            try
            {
                long empresaId = long.Parse(Session["empresaId"].ToString());
                long usuarioId = long.Parse(Session["usuario"].ToString());
                int respuestaValidador = validadorNroContacto(clienteNuevo.clienteNroContacto);
                if (respuestaValidador == 1)
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        clientes cliente = new clientes
                        {
                            cliente_canalcontacto_id = clienteNuevo.clienteCanalContacto,
                            cliente_tipopersona_id = clienteNuevo.clienteTipoPersona,
                            cliente_tipodocumentoidentidad_id = clienteNuevo.clienteTipoDocumento,
                            cliente_nrodocumento = clienteNuevo.clienteNroDocumento,
                            cliente_razonsocial = clienteNuevo.clienteRazonSocial,
                            cliente_nrocontacto = clienteNuevo.clienteNroContacto,
                            cliente_nrocontactoauxiliar = clienteNuevo.clienteNroContactoAuxiliar,
                            cliente_email = clienteNuevo.clienteCorreoElectronico,
                            cliente_departamento_id = clienteNuevo.clienteDepartamento,
                            cliente_provincia_id = clienteNuevo.clienteProvincia,
                            cliente_distrito_id = clienteNuevo.clienteDistrito,
                            cliente_direccion = clienteNuevo.clienteDireccion,
                            cliente_empresa_id = empresaId,
                            cliente_promotor_id = clienteNuevo.clientePromotor,
                            cliente_estadoproceso_id = 1,//POR CONTACTAR
                            cliente_estadocaptacioncliente_id = 1, //EN PROCESO
                            cliente_fechacreacion = DateTime.Now,
                            cliente_usuariocreacion = usuarioId
                        };
                        db.clientes.Add(cliente);
                        db.SaveChanges();
                        foreach (var itemInteresProyecto in clienteNuevo.clienteInteresesProyecto)
                        {
                            interesesclienteproyecto interesClienteProyecto = new interesesclienteproyecto
                            {
                                interesclienteproyecto_cliente_id = cliente.cliente_id,
                                interesclienteproyecto_proyecto_id = itemInteresProyecto.idInteresProyecto
                            };
                            db.interesesclienteproyecto.Add(interesClienteProyecto);
                            db.SaveChanges();
                        }
                        transaction.Complete();
                    }
                    respuesta.Data = new
                    {
                        flag = 1
                    };
                    return respuesta;
                }
                else
                {
                    respuesta.Data = new
                    {
                        flag = 2
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
        /// funcion encargada de actualizar la informacion de un cliente
        /// </summary>
        /// <param name="cliente">ViewModel de clientes</param>
        /// <returns><c>json</c> con las respuesta de la actividad</returns>
        public ActionResult actualizarCliente(DataClientes cliente)
        {
            respuesta = new JsonResult();
            try
            {
                long empresaId = long.Parse(Session["empresaId"].ToString());
                long usuarioId = long.Parse(Session["usuario"].ToString());
                int respuestaValidador = validadorNroContacto(cliente.clienteNroContacto);
                clientes clienteActualizar = db.clientes.Find(cliente.clienteId);
                if (respuestaValidador == 1 || clienteActualizar.cliente_nrocontacto.Equals(cliente.clienteNroContacto))
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        clienteActualizar.cliente_canalcontacto_id = cliente.clienteCanalContacto;
                        clienteActualizar.cliente_tipopersona_id = cliente.clienteTipoPersona;
                        clienteActualizar.cliente_tipodocumentoidentidad_id = cliente.clienteTipoDocumento;
                        clienteActualizar.cliente_nrodocumento = cliente.clienteNroDocumento;
                        clienteActualizar.cliente_razonsocial = cliente.clienteRazonSocial;
                        clienteActualizar.cliente_nrocontacto = cliente.clienteNroContacto;
                        clienteActualizar.cliente_nrocontactoauxiliar = cliente.clienteNroContactoAuxiliar;
                        clienteActualizar.cliente_email = cliente.clienteCorreoElectronico;
                        clienteActualizar.cliente_departamento_id = cliente.clienteDepartamento;
                        clienteActualizar.cliente_provincia_id = cliente.clienteProvincia;
                        clienteActualizar.cliente_distrito_id = cliente.clienteDistrito;
                        clienteActualizar.cliente_direccion = cliente.clienteDireccion;
                        clienteActualizar.cliente_empresa_id = empresaId;
                        clienteActualizar.cliente_promotor_id = cliente.clientePromotor;
                        clienteActualizar.cliente_fechamodificacion = DateTime.Now;
                        clienteActualizar.cliente_usuariomodificacion = usuarioId;

                        db.Entry(clienteActualizar).State = EntityState.Modified;
                        db.SaveChanges();
                        /*foreach (var itemInteresProyecto in clienteNuevo.clienteInteresesProyecto)
                        {
                            interesesclienteproyecto interesClienteProyecto = new interesesclienteproyecto
                            {
                                interesclienteproyecto_cliente_id = cliente.cliente_id,
                                interesclienteproyecto_proyecto_id = itemInteresProyecto.idInteresProyecto
                            };
                            db.interesesclienteproyecto.Add(interesClienteProyecto);
                            db.SaveChanges();
                        }*/
                        transaction.Complete();
                    }
                    respuesta.Data = new
                    {
                        flag = 1
                    };
                    return respuesta;
                }
                else
                {
                    respuesta.Data = new
                    {
                        flag = 2
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
        /// funcion encargada de crear un nuevo evento
        /// </summary>
        /// <param name="nuevoEvento">ViewModel de eventos</param>
        /// <returns><c>json</c> con las respuesta de la actividad</returns>
        public ActionResult crearEvento(DataEvento nuevoEvento)
        {
            respuesta = new JsonResult();
            try
            {
                long usuarioId = long.Parse(Session["usuario"].ToString());
                using (TransactionScope transaction = new TransactionScope())
                {
                    DateTime fechaInicio = DateTime.Parse(nuevoEvento.eventoFechaInicio);
                    DateTime fechaFin = DateTime.Parse(nuevoEvento.eventoFechaFin);
                    eventos evento = new eventos
                    {
                        evento_usuario_id = usuarioId,
                        evento_fechacreacion = fechaInicio,
                        evento_fechapropuesta = fechaFin,
                        evento_descripcion = nuevoEvento.eventoDescripcion,
                        evento_estado = true
                    };
                    db.eventos.Add(evento);
                    db.SaveChanges();
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

        /// <summary>
        /// funcion encargada de buscar un cliente segun su id
        /// </summary>
        /// <param name="idCliente">id del cliente</param>
        /// <returns><c>json</c> con las respuesta de la actividad</returns>
        public ActionResult buscarCliente(long idCliente)
        {
            respuesta = new JsonResult();
            try
            {

                clientes cliente = db.clientes.Find(idCliente);
                canalescontacto canalContacto = db.canalescontacto.Where(cco => cco.canalcontacto_id == cliente.cliente_canalcontacto_id).FirstOrDefault();
                medioscontacto medio = db.medioscontacto.Where(mco => mco.mediocontacto_id == canalContacto.canalcontacto_mediocontacto_id).FirstOrDefault();
                List<interesesclienteproyecto> listaInteresesProyecto = db.interesesclienteproyecto.Where(inp => inp.interesclienteproyecto_cliente_id == idCliente).ToList();
                DataClientes dataClientes = new DataClientes
                {
                    clienteMedioContacto = medio.mediocontacto_id,
                    clienteCanalContacto = canalContacto.canalcontacto_id,
                    clienteTipoPersona = cliente.cliente_tipopersona_id,
                    clienteTipoDocumento = cliente.cliente_tipodocumentoidentidad_id,
                    clienteNroDocumento = cliente.cliente_nrodocumento,
                    clienteRazonSocial = cliente.cliente_razonsocial,
                    clienteNroContacto = cliente.cliente_nrocontacto,
                    clienteNroContactoAuxiliar = cliente.cliente_nrocontactoauxiliar,
                    clienteCorreoElectronico = cliente.cliente_email,
                    clienteDepartamento = cliente.cliente_departamento_id,
                    clienteProvincia = cliente.cliente_provincia_id,
                    clienteDistrito = cliente.cliente_distrito_id,
                    clienteDireccion = cliente.cliente_direccion,
                    clientePromotor = cliente.cliente_promotor_id
                };
                List<DataInteresesProyecto> listaDataIntereses = new List<DataInteresesProyecto>();
                foreach (var itemInteresProyecto in listaInteresesProyecto)
                {
                    DataInteresesProyecto interesProyecto = new DataInteresesProyecto
                    {
                        idInteresProyecto = itemInteresProyecto.interesclienteproyecto_proyecto_id
                    };
                    listaDataIntereses.Add(interesProyecto);
                }
                dataClientes.clienteInteresesProyecto = listaDataIntereses;
                respuesta.Data = new
                {
                    flag = 1,
                    dataClientes

                };
                respuesta.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
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

        /// <summary>
        /// funcion encargada de buscar un cliente por un termino o parcialmente un texto
        /// </summary>
        /// <param name="term">texto por el que se va a realizar la busqueda</param>
        /// <returns><c>json</c> con los datos de la busqueda</returns>
        public ActionResult buscarClientesPorDocumento(string term)
        {
            respuesta = new JsonResult();
            try
            {
                return Json(db.clientes
                     .Where(cli => cli.cliente_nrodocumento.Contains(term))
                     .Select(cli => new
                     {
                         id = cli.cliente_id,
                         label = cli.cliente_nrodocumento + "-" + cli.cliente_razonsocial,
                         nombre = cli.cliente_razonsocial,
                         nroDocumento = cli.cliente_nrodocumento,
                         nroContacto = cli.cliente_nrocontacto,
                         promotorId = cli.cliente_promotor_id,
                         tipoPersona = cli.cliente_tipopersona_id,
                         tipoDocumento = cli.cliente_tipodocumentoidentidad_id
                     }).Take(10), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de buscar clientes por un termino o parcialmente un texto y el tipo de documento que tiene
        /// </summary>
        /// <param name="term">texto por el que se va a realizar la busqueda</param>
        /// <param name="param1">id del tipo de documento que tiene el cliente</param>
        /// <returns><c>json</c> con los datos de la busqueda</returns>
        public ActionResult buscarClientesAutocomplete(string term, string param1)
        {
            respuesta = new JsonResult();
            try
            {
                long tipodocumento = long.Parse(param1);
                if (tipodocumento == 2)
                {
                    return Json(db.clientes
                      .Where(cli => (cli.cliente_nrodocumento.Contains(term) || cli.cliente_razonsocial.Contains(term)) && cli.cliente_tipodocumentoidentidad_id == 4)
                      .Select(cli => new
                      {
                          id = cli.cliente_id,
                          label = cli.cliente_nrodocumento + "-" + cli.cliente_razonsocial
                      }).Take(10), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(db.clientes
                     .Where(cli => cli.cliente_nrodocumento.Contains(term) || cli.cliente_razonsocial.Contains(term))
                     .Select(cli => new
                     {
                         id = cli.cliente_id,
                         label = cli.cliente_nrodocumento + "-" + cli.cliente_razonsocial
                     }).Take(10), JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de buscar clientes por un termino o parcialmente un texto y el tipo de documento que tiene (solo para casuarinas)
        /// </summary>
        /// <param name="term">texto por el que se va a realizar la busqueda</param>
        /// <param name="param1">id del tipo de documento que tiene el cliente</param>
        /// <returns><c>json</c> con los datos de la busqueda</returns>
        public ActionResult buscarClientesAutocompleteCasuarinas(string term, string param1)
        {
            respuesta = new JsonResult();
            try
            {
                long tipodocumento = long.Parse(param1);
                if (tipodocumento == 2)
                {
                    return Json(db.clientes
                      .Where(cli => (cli.cliente_nrodocumento.Contains(term) || cli.cliente_razonsocial.Contains(term)) && cli.cliente_tipodocumentoidentidad_id == 4 && cli.cliente_escasuarinas == true)
                      .Select(cli => new
                      {
                          id = cli.cliente_id,
                          label = cli.cliente_nrodocumento + "-" + cli.cliente_razonsocial
                      }).Take(10), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(db.clientes
                     .Where(cli => cli.cliente_nrodocumento.Contains(term) || cli.cliente_razonsocial.Contains(term) && cli.cliente_escasuarinas == true)
                     .Select(cli => new
                     {
                         id = cli.cliente_id,
                         label = cli.cliente_nrodocumento + "-" + cli.cliente_razonsocial
                     }).Take(10), JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// funcion encargada de buscar clientes por un termino o parcialmente un texto y el proyecto al que pertenece
        /// </summary>
        /// <param name="term">texto por el que se va a realizar la busqueda</param>
        /// <param name="param1">id del proyecto</param>
        /// <returns><c>json</c> con los datos de la busqueda</returns>
        public ActionResult buscarClientesContrato(string term, string param1)
        {
            respuesta = new JsonResult();
            try
            {
                long proyectoId = long.Parse(param1);
                if(proyectoId == 0)
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

        /// <summary>
        /// funcion encargada de buscar clientes por un termino o parcialmente un texto y el proyecto al que pertenece (este busca por contrato al cliente)
        /// </summary>
        /// <param name="term">texto por el que se va a realizar la busqueda</param>
        /// <param name="param1">id del proyecto</param>
        /// <returns><c>json</c> con los datos de la busqueda</returns>
        public ActionResult buscarClientesContrato2(string term, string param1)
        {
            respuesta = new JsonResult();
            try
            {
                long proyectoId = long.Parse(param1);
                if(proyectoId == 0)
                {
                    return Json(db.vw_busquedaContratosAutocomplete
                    .Where(vw => vw.contrato_numeracion.Contains(term))
                    .Select(vw => new {
                        id = vw.contrato_id,
                        label = vw.contrato_numeracion + "-" + vw.cliente_razonsocial,
                        cliente = vw.cliente_nrodocumento + "-" + vw.cliente_razonsocial,
                        numeracion = vw.contrato_numeracion,
                        lote = vw.lote_nombre + "-" + vw.proyecto_nombrecorto
                    }).Take(10), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(db.vw_busquedaContratosAutocomplete
                    .Where(vw => vw.contrato_numeracion.Contains(term) && vw.lote_proyecto_id == proyectoId)
                    .Select(vw => new {
                        id = vw.contrato_id,
                        label = vw.contrato_numeracion + "-" + vw.cliente_razonsocial,
                        cliente = vw.cliente_nrodocumento + "-" + vw.cliente_razonsocial,
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

        /// <summary>
        /// Funcion encargada de validar el numero de contacto (funciona dentro de la funcion de crear cliente)
        /// </summary>
        /// <param name="nroContacto">numero de contacto del cliente</param>
        /// <returns>flag que indica el estado del nro de contacto 3 para cuando no hay nro de contacto, 1 para cuando no existe en la base de datos y 2 para cunado existe</returns>
        public int validadorNroContacto(string nroContacto)
        {
            if (nroContacto.Equals(""))
            {
                return 3;
            }
            else
            {
                clientes cliente = db.clientes.Where(cli => cli.cliente_nrocontacto == nroContacto).FirstOrDefault();
                if (cliente == null)
                {
                    return 1;

                }
                else
                {
                    return 2;
                }
            }
        }
    }
}
