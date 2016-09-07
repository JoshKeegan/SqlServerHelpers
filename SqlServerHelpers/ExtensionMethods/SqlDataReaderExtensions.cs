/*
 * SqlServerHelpers
 * SqlDataReaderExtensions
 * Authors:
 *  Josh Keegan 10/02/2015
 *  
 *  TODO: No need to be getting all types via string
 */

using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerHelpers.ExtensionMethods
{
    public static class SqlDataReaderExtensions
    {
        #region Has Helpers

        public static bool HasField(this SqlDataReader reader, string fieldName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Get Field Value Methods

        public static bool GetBool(this SqlDataReader reader, string fieldName)
        {
            return reader[fieldName].ToString() == "True";
        }

        public static byte GetByte(this SqlDataReader reader, string fieldName)
        {
            return byte.Parse(reader[fieldName].ToString());
        }

        public static int GetInt(this SqlDataReader reader, string fieldName)
        {
            return int.Parse(reader[fieldName].ToString());
        }

        public static long GetLong(this SqlDataReader reader, string fieldName)
        {
            return long.Parse(reader[fieldName].ToString());
        }

        public static double GetDouble(this SqlDataReader reader, string fieldName)
        {
            return double.Parse(reader[fieldName].ToString());
        }

        public static DateTime GetDateTime(this SqlDataReader reader, string fieldName)
        {
            object field = reader[fieldName];
            return getDateTime(field);
        }

        public static DbGeography GetDbGeographyFromString(this SqlDataReader reader, string fieldName, int coordinateSystemId = -1)
        {
            // If no coordinate system ID is supplied, use the default
            if (coordinateSystemId == -1)
            {
                coordinateSystemId = Settings.CoordinateSystemId;
            }

            string strField = reader[fieldName].ToString();
            if (strField == null || strField == "")
            {
                return null;
            }

            return DbGeography.FromText(strField, coordinateSystemId);
        }

        public static byte[] GetByteArr(this SqlDataReader reader, string fieldName)
        {
            byte[] blob = (byte[])reader[fieldName];
            return blob;
        }

        //returns null for null fields rather than String.Empty
        public static string GetString(this SqlDataReader reader, string fieldName)
        {
            object field = reader[fieldName];
            if (field == DBNull.Value)
            {
                return null;
            }
            return field.ToString();
        }

        public static bool? GetNullableBool(this SqlDataReader reader, string fieldName)
        {
            string strField = reader[fieldName].ToString();
            if (strField == null || strField == "")
            {
                return null;
            }
            return strField == "True";
        }

        public static int? GetNullableInt(this SqlDataReader reader, string fieldName)
        {
            string strField = reader[fieldName].ToString();
            if (strField == null || strField == "")
            {
                return null;
            }
            return int.Parse(strField);
        }

        public static double? GetNullableDouble(this SqlDataReader reader, string fieldName)
        {
            string strField = reader[fieldName].ToString();
            if (strField == null || strField == "")
            {
                return null;
            }
            return double.Parse(strField);
        }

        public static long? GetNullableLong(this SqlDataReader reader, string fieldName)
        {
            string strField = reader[fieldName].ToString();
            if (strField == null || strField == "")
            {
                return null;
            }
            return long.Parse(strField);
        }

        public static DateTime? GetNullableDateTime(this SqlDataReader reader, string fieldName)
        {
            object field = reader[fieldName];
            if (field == DBNull.Value)
            {
                return null;
            }
            return getDateTime(field);
        }

        #endregion

        #region Private Helpers

        private static DateTime getDateTime(object fieldVal)
        {
            // Timezone doesn't get stored in MS SQLs datetime2 type, so Kind will be unspecified
            DateTime raw = (DateTime)fieldVal;

            DateTime local;
            // If times in the DB are in UTC, convert it to local time when retrieved
            if (Settings.TimesStoredInUtc)
            {
                DateTime utc = new DateTime(raw.Ticks, DateTimeKind.Utc);
                // Can use local time internally for business logic
                local = utc.ToLocalTime();
                return local;
            }
            else // Otherwise, the database stores times in the local timezone
            {
                local = new DateTime(raw.Ticks, DateTimeKind.Local);
            }

            return local;
        }

        #endregion
    }
}
