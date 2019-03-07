using System;
using System.Collections.Generic;
using System.Text;

namespace PlataformaPDCOnline.Internals.Internals
{
    public class ConsultasPreparadas
    {
        private InformixOdbcDao infx;

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
            infx = new InformixOdbcDao();
        }

        public int EventCommitCommit()
        {
            int result = 0;

            try
            {
                infx.Database.connection.Open();
                OdbcTransaction transaccion;

                try
                {

                }
                catch ()
                {

                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }


            return result;
        }

        //esto no tiene que estar aqui, pero se utilizara para la aplicacion de recibir eventos, asi que por ahora se queda comentado
        //Actualiza los valores de cambios y los eventos commiteados de la base de datos con una transaccion, si algo peta, no se actualizaran ninguno
        /*public int UpdateChangesValuesAndCommitsValues(WebCommandsController controller, Command command, OdbcTransaction tran)
        {

            string sql = "SELECT eventcommit, changevalue FROM " + controller.TableName + " WHERE " + controller.UidTableName + " = ?";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add(controller.UidTableName, command.AggregateId);

            Dictionary<string, OdbcType> types = new Dictionary<string, OdbcType>();
            types.Add(controller.UidTableName, OdbcType.VarChar);

            OdbcCommand selectCommand = new OdbcCommand(sql, infx.Database.Connection, tran);

            DatabaseTools.InsertParameters(parameters, types, selectCommand);

            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            try
            {
                result = this.executeCommadForSelect(selectCommand);
            }
            catch (MyOdbcException e)
            {
                throw new MyOdbcException("error en la select de la transaccion: " + e.ToString());
            }

            if(result.Count == 1)
            {
                parameters.Clear();
               
                foreach(Dictionary<string, object> dic in result)
                {
                    parameters.Add("changevalue", controller.CommandName.IndexOf("Create") == 0 ? 0 : ((int)(dic.GetValueOrDefault("changevalue")) - 1)); //si el command comienza por create
                    parameters.Add("eventcommit", ((int) (dic.GetValueOrDefault("eventcommit")) + 1));
                }


                sql = "UPDATE " + controller.TableName + " SET changevalue = ?, eventcommit = ?  WHERE " + controller.UidTableName + " = ?;";
                
                parameters.Add(controller.UidTableName, command.AggregateId);

                types.Add("changevalue", OdbcType.Int);
                types.Add("eventcommit", OdbcType.Int);

                OdbcCommand updateCommand = new OdbcCommand(sql, infx.Database.Connection, tran);

                DatabaseTools.InsertParameters(parameters, types, updateCommand);
                
                try
                {
                    return updateCommand.ExecuteNonQuery();
                }
                catch (MyOdbcException e)
                {
                    throw new MyOdbcException("Error en la update de la transaccion: " + e.ToString());
                }
            }

            return 0;
        }*/
    }
}
