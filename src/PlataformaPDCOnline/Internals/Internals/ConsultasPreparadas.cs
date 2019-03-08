using OdbcDatabase;
using OdbcDatabase.database;
using OdbcDatabase.excepciones;
using Pdc.Messaging;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Text;

namespace PlataformaPDCOnline.Internals.Internals
{
    public class ConsultasPreparadas
    {
        private InformixOdbcDao Infx;

        private static ConsultasPreparadas consultas;

        public static ConsultasPreparadas Singelton()
        {
            if(consultas == null)
            {
                consultas = new ConsultasPreparadas();
            }
            return consultas;
        }

        private ConsultasPreparadas()
        {
            Infx = new InformixOdbcDao();
        }

        public int EventCommitCommit(Event commitEventCommit, WebEventController controller)
        {
           int result = 0;

           string sql = "SELECT eventcommit, savechanges FROM " + controller.TableName + " WHERE " + controller.UidName + " = ?";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { controller.UidName, commitEventCommit.AggregateId }
            };

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>
            {
                { controller.UidName, OdbcType.VarChar }
            };

            OdbcCommand command = new OdbcCommand(sql, Infx.Database.Connection);

            DatabaseTools.InsertParameters(parameters, types, command);

            List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();
            try
            {
                Infx.Database.Connection.Open();
                table = ExecuteCommadForSelect(command);
                Infx.Database.Connection.Close();
            }
            catch (MyOdbcException e)
            {
                if (Infx.Database.Connection.State == System.Data.ConnectionState.Open) Infx.Database.Connection.Close();
                ErrorDBLog.Write(e.Message);
            }

            if(table.Count == 1)
            {
                sql = "UPDATE " + controller.TableName + " SET changevalue = ? eventcommit = ? WHERE " + controller.UidName + " = ?";

                parameters.Clear();
                parameters.Add("changevalue", (int) table.ToArray()[0].GetValueOrDefault("changevalue"));
                parameters.Add("eventcommit", (int) table.ToArray()[0].GetValueOrDefault("eventcommit"));
                parameters.Add(controller.UidName, commitEventCommit.AggregateId);

                types.Add("changevalue", OdbcType.Int);
                types.Add("eventcommit", OdbcType.Int);

                command = new OdbcCommand(sql, Infx.Database.Connection);

                DatabaseTools.InsertParameters(parameters, types, command);

                try
                {
                    Infx.Database.Connection.Open();
                    result = command.ExecuteNonQuery();
                    Infx.Database.Connection.Close();
                }
                catch(MyOdbcException e)
                {
                    if (Infx.Database.Connection.State == System.Data.ConnectionState.Open) Infx.Database.Connection.Close();
                    ErrorDBLog.Write("Error: " + e.ToString());
                }
            }
            else
            {
                //excepcion??
            }

            return result;
        }

        public List<Dictionary<string, object>> GetEvents()
        {
            string sql = "SELECT eventname, tablename, uidname, suscriptionname FROM webevents WHERE active = ?";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "active", 1 }
            };

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>
            {
                { "active", OdbcType.Int }
            };

            OdbcCommand commandOdbc = new OdbcCommand(sql, Infx.Database.Connection);

            DatabaseTools.InsertParameters(parameters, types, commandOdbc);
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                Infx.Database.Connection.Open();
                result = this.ExecuteCommadForSelect(commandOdbc);
                Infx.Database.Connection.Close();
            }
            catch (MyOdbcException e)
            {
                //Console.WriteLine("cierra conexion Exception " + sql);
                if (Infx.Database.Connection.State == System.Data.ConnectionState.Open) Infx.Database.Connection.Close();
                ErrorDBLog.Write("Error: " + e.ToString());
            }
            return result;
        }

        //ejecuta un command de ODBCCommand, que sea un select y te devuelve una lista de diccionarios
        private List<Dictionary<string, object>> ExecuteCommadForSelect(OdbcCommand command)
        {
            List<Dictionary<string, object>> tablaResult = new List<Dictionary<string, object>>();
            try
            {
                OdbcDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Dictionary<string, object> rowResult = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        rowResult.Add(reader.GetName(i), reader.GetValue(i));
                    }
                    tablaResult.Add(rowResult);
                }

            }
            catch (MyOdbcException e)
            {
                throw new MyOdbcException("Error consultas preparadas" + e.ToString());
            }
            return tablaResult;
        }
    }
}
