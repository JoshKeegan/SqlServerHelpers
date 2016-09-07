/*
 * SqlServerHelpers
 * Settings
 * Authors:
 *  Josh Keegan 05/09/2016
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerHelpers
{
    public static class Settings
    {
        /// <summary>
        /// Default settings to apply to each new SqlCommand.
        /// </summary>
        public static CommandSettings Command = null;

        /// <summary>
        /// Coordinate System ID to be used by default when reading Geography from the DB.
        /// </summary>
        public static int CoordinateSystemId = 4326;

        /// <summary>
        /// Whether times should be stored in the database in UTC.
        /// Will automatically convert all times read from the DB from UTC to local time & vice versa when writing.
        /// </summary>
        public static bool TimesStoredInUtc = true;
    }
}
