using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace proyecto_vivemas.Connection
{
    public class vivemasDB
    {
        public MySqlConnection iniciarConexion()
        {
            //MySqlConnection connection = new MySqlConnection("Database=sistema;Data Source=localhost;User Id=root;Password=Killmebaby123;Allow User Variables=True");
            MySqlConnection connection = new MySqlConnection("Database=sistema;Data Source=192.168.0.100;User Id=root;Password=$VIVEINCO159753$;Allow User Variables=True");
            connection.Open();
            return connection;
        }

        public MySqlDataReader ejecutarQuery(string query, MySqlConnection connection)
        {
            MySqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            MySqlDataReader reader = command.ExecuteReader();
            return reader;
        }

        public void cerrarConexion(MySqlConnection connection)
        {
            connection.Close();
        }
    }
}