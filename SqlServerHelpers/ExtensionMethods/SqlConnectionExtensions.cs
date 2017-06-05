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
        #region Public Methods

        public static SqlCommand GetSqlCommand(this SqlConnection conn, SqlTransaction trans = null,
            CommandSettings commandSettings = null)
        {
            return getSqlCommand(null, conn, trans, commandSettings);
        }

        public static SqlCommand GetSqlCommand(this SqlConnection conn, string txtCmd, SqlTransaction trans = null,
            CommandSettings commandSettings = null)
        {
            return getSqlCommand(txtCmd, conn, trans, commandSettings);
        }

        public static SqlCommand GetSqlCommand(this SqlConnection conn, CommandSettings commandSettings)
        {
            return GetSqlCommand(conn, null, commandSettings);
        }

        #endregion

        #region Private Methods

        private static SqlCommand getSqlCommand(string txtCmd, SqlConnection conn, SqlTransaction trans,
            CommandSettings commandSettings)
        {
            // If no command settings have been supplied, use the default ones as defined statically in Settings
            if (commandSettings == null)
            {
                commandSettings = Settings.Command;
            }

            // Make the command
            SqlCommand command = new SqlCommand(txtCmd, conn, trans);

            // Apply command settings
            if (commandSettings != null)
            {
                if (commandSettings.CommandTimeout != null)
                {
                    command.CommandTimeout = (int) commandSettings.CommandTimeout;
                }
                // TODO: Support more default settings as they're added to CommandSettings
            }

            return command;
        }

        #endregion
    }
}
