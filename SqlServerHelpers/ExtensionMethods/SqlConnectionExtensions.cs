/* 
 * SqlServerHelpers
 * SqlConnectionExtensions - Extension methods for SqlConnection
 * Authors:
 *  Josh Keegan 26/05/2015
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerHelpers.ExtensionMethods
{
    public static class SqlConnectionExtensions
    {
        public static SqlCommand GetSqlCommand(this SqlConnection conn, SqlTransaction trans = null)
        {
            return getSqlCommand(null, conn, trans);
        }

        public static SqlCommand GetSqlCommand(this SqlConnection conn, string txtCmd, SqlTransaction trans = null)
        {
            return getSqlCommand(txtCmd, conn, trans);
        }

        private static SqlCommand getSqlCommand(string txtCmd, SqlConnection conn, SqlTransaction trans)
        {
            SqlCommand command = new SqlCommand(txtCmd, conn, trans);

            // Apply default command settings
            if (Settings.Command != null)
            {
                if (Settings.Command.CommandTimeout != null)
                {
                    command.CommandTimeout = (int) Settings.Command.CommandTimeout;
                }
                // TODO: Support more default settings as they're added to CommandSettings
            }

            return command;
        }
    }
}
