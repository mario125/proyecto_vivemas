using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using proyecto_vivemas.Models;
using proyecto_vivemas.Util;
using proyecto_vivemas.Connection;
using MySql.Data.MySqlClient;
using BCrypt.Net;


namespace proyecto_vivemas.Controllers
{
    public class UsuariosController : Controller
    {
        vivemas_dbEntities db = new vivemas_dbEntities();
        vivemasDB vivemas = new vivemasDB();
        MySqlConnection connection;
        MySqlDataReader reader;
        JsonResult respuesta;
        public ActionResult Autenticar(UsuariosModel usuarioModelo)
        {
            respuesta = new JsonResult();
            try
            {
                int contador = 0;
                string pass = new Utilities().EncodText(usuarioModelo.password);
                string query = "SELECT COUNT(*) as resultado FROM USUARIO WHERE LOGIN='" + usuarioModelo.usuario + "' AND PASS='" + pass + "' AND ID_EST=1";
                connection = vivemas.iniciarConexion();
                reader = vivemas.ejecutarQuery(query, connection);
                while (reader.Read())
                {
                    contador = Convert.ToInt32(reader["resultado"].ToString());
                }
                reader.Close();
                if (contador != 0)
                {
                    query = "SELECT CONCAT(EMP.prinom, ' ', EMP.apepat, ' ', EMP.apemat) Nombrecompleto, \n" +
                            "numdoc, \n" +
                            "CAR.nomcargo, \n" +
                            "USU.id_user \n" +
                            "FROM sistema.empleado EMP \n" +
                            "INNER JOIN sistema.usuario USU ON USU.id_user = EMP.id_usu \n" +
                            "INNER JOIN sistema.cargo CAR ON EMP.id_car = CAR.id_cargo \n" +
                            "WHERE USU.LOGIN = '" + usuarioModelo.usuario + "' AND USU.PASS = '" + pass + "'";
                    reader = vivemas.ejecutarQuery(query, connection);
                    while (reader.Read())
                    {
                        respuesta.Data = new
                        {
                            flag = 1
                        };
                        Session["usuario"] = reader["id_user"].ToString();
                        Session["nombre"] = reader["Nombrecompleto"].ToString();
                        Session["documento"] = reader["numdoc"].ToString();
                        Session["cargo"] = reader["nomcargo"].ToString();
                    }
                }
                else
                {
                    respuesta.Data = new
                    {
                        flag = 0
                    };
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                respuesta.Data = new
                {
                    flag = 2
                };
                return respuesta;
            }
            finally
            {
                reader.Close();
                connection.Close();
            }
        }

        public ActionResult AutenticarNuevo(UsuariosModel usuarioModelo)
        {
            respuesta = new JsonResult();
            try
            {
                //string hashpassword = BCrypt.Net.BCrypt.EnhancedHashPassword(usuarioModelo.password);
                List<usuarios> usuarios = db.usuarios.Where(usu => usu.usuario_usuario == usuarioModelo.usuario && usu.usuario_estado == true).ToList();
                if(usuarios.Count == 0)
                {
                    respuesta.Data = new { flag = 3 };
                    return respuesta;
                }
                foreach (var usuarioItem in usuarios)
                {
                    bool checkPassword = BCrypt.Net.BCrypt.EnhancedVerify(usuarioModelo.password, usuarioItem.usuario_pass);
                    if (checkPassword)
                    {
                        respuesta.Data = new
                        {
                            flag = 1,
                        };
                        configuraciones tipoCambioInterno = db.configuraciones.Where(con => con.configuracion_empresa_id == usuarioItem.roles.rol_empresa_id).FirstOrDefault();
                        Session["usuario"] = usuarioItem.usuario_id;
                        Session["empresaId"] = usuarioItem.roles.rol_empresa_id;
                        Session["cargoId"] = usuarioItem.usuario_rol_id;
                        Session["empresaNombre"] = usuarioItem.roles.empresas.empresa_nombrecomercial;
                        Session["nombre"] = usuarioItem.datosusuarios.datosusuario_razonsocial;
                        Session["documento"] = usuarioItem.datosusuarios.datosusuario_nrodocumento;
                        Session["cargo"] = usuarioItem.roles.rol_nombre;
                        Session["tipoCambioInterno"] = tipoCambioInterno.configuracion_tipocambiointerno;
                    }
                    else
                    {
                        respuesta.Data = new
                        {
                            flag = 0,
                        };
                    }
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                respuesta.Data = new
                {
                    flag = 2,
                };
                return respuesta;
            }
        }

        public ActionResult CerrarSesion()
        {
            Session.Abandon();
            return Redirect("~/");
        }


    }
}
