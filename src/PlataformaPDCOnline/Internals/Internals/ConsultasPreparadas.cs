using OdbcDatabase;
using OdbcDatabase.database;
using OdbcDatabase.excepciones;
using Pdc.Messaging;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Text;
using System.Threading.Tasks;

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

        public async Task RunEventCommitCommit(Event commitEventCommit, WebEventController controller)
        {
            if (controller != null)
            {
                OdbcTransaction transaccion = null;
                try
                {
                    Infx.Database.Connection.Open();
                    try
                    {
                        transaccion = Infx.Database.Connection.BeginTransaction();

                        this.EventCommitCommit(commitEventCommit, controller, transaccion);

                        transaccion.Commit();
                    }
                    catch (MyOdbcException e)
                    {
                        transaccion.Rollback();
                        ErrorDBLog.Write("Error: " + e.ToString());
                    }
                }
                catch (MyOdbcException ex)
                {
                    ErrorDBLog.Write("Error: " + ex.ToString());
                }
                finally
                {
                    if (Infx.Database.Connection.State == System.Data.ConnectionState.Open) Infx.Database.Connection.Close();
                }
            }
        }

        private int EventCommitCommit(Event commitEventCommit, WebEventController controller, OdbcTransaction transaccion)
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

            OdbcCommand command = new OdbcCommand(sql, Infx.Database.Connection)
            {
                Transaction = transaccion
            };

            DatabaseTools.InsertParameters(parameters, types, command);

            List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();
            try
            {
                table = ExecuteCommadForSelect(command);
            }
            catch (OdbcException e)
            {
                ErrorDBLog.Write(e.Message);
                throw new MyOdbcException(e.Message);
            }

            if(table.Count == 1)
            {
                sql = "UPDATE " + controller.TableName + " SET changevalue = ? eventcommit = ? WHERE " + controller.UidName + " = ?";

                parameters.Clear();
                parameters.Add("changevalue", (int) table.ToArray()[0].GetValueOrDefault("changevalue") - 1);
                parameters.Add("eventcommit", (int) table.ToArray()[0].GetValueOrDefault("eventcommit") + 1);
                parameters.Add(controller.UidName, commitEventCommit.AggregateId);

                types.Add("changevalue", OdbcType.Int);
                types.Add("eventcommit", OdbcType.Int);

                command = new OdbcCommand(sql, Infx.Database.Connection);

                DatabaseTools.InsertParameters(parameters, types, command);

                try
                {
                    result = command.ExecuteNonQuery();
                }
                catch(OdbcException e)
                {
                    ErrorDBLog.Write("Error: " + e.ToString());
                    throw new MyOdbcException(e.Message);
                }
            }

            return result;
        }

        public List<Dictionary<string, object>> GetEvents()
        {
            string sql = "SELECT eventname, tablename, uidname FROM webevents WHERE activo = ?"; //cambiar el activo a active en la base de datos

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "activo", 1 }
            };

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>
            {
                { "activo", OdbcType.Int }
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
            catch (OdbcException e)
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
            catch (OdbcException e)
            {
                throw new MyOdbcException("Error consultas preparadas" + e.ToString());
            }
            return tablaResult;
        }
    }
}
