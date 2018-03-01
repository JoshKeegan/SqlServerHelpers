/*
 * SqlServerHelpers
 * Logging
 * Authors:
 *  Josh Keegan 01/03/2018
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerHelpers
{
    public static class Logging
    {
        #region Public

        public delegate void LogMessageWriter(string message);

        #endregion

        #region Internal Methods

        internal static void Write(string message)
        {
            // Fetch log writer from settings to prevent it being removed between checking & using
            LogMessageWriter logWriter = Settings.LogWriter;

            // If there's a log writer set up, use it
            if (logWriter != null)
            {
                tryWrite(logWriter, message);
            }
        }

        internal static void Write(string messageFormat, params object[] args)
        {
            // Fetch log writer from settings to prevent it being removed between checking & using
            LogMessageWriter logWriter = Settings.LogWriter;

            // If there's a log writer set up, use it
            if (logWriter != null)
            {
                // Process the message format and arguments to be written into an actual string message
                string message = String.Format(messageFormat, args);

                tryWrite(logWriter, message);
            }
        }

        #endregion

        #region Private Methods

        private static void tryWrite(LogMessageWriter logWriter, string message)
        {
            // Prevent erorrs within the users implementation of log writing to stop the library from running
            try
            {
                logWriter(message);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch {  }
        }

        #endregion
    }
}
