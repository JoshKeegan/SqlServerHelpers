/*
 * SqlServerHelpers
 * SqlParameterCollectionExtensions - Extension methods for SqlParameterCollection
 * Authors:
 *  Josh Keegan 10/02/2015
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.SqlServer.Server;

namespace SqlServerHelpers.ExtensionMethods
{
    public static class SqlParameterCollectionExtensions
    {
        #region Single Value

        public static SqlParameter AddWithValue(this SqlParameterCollection parameters,
            string paramName, object value, SqlDbTypeSize typeSize)
        {
            // Validation
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (paramName == null)
            {
                throw new ArgumentNullException(nameof(paramName));
            }
            if (typeSize == null)
            {
                throw new ArgumentNullException(nameof(typeSize),
                    nameof(typeSize) + String.Format("{0} is null for paramName {1}", nameof(typeSize), paramName));
            }

            return parameters.addWithValue(paramName, value, typeSize.SqlDbType, typeSize.Size);
        }

        private static SqlParameter addWithValue(this SqlParameterCollection parameters,
            string paramName, object value, SqlDbType type, int size)
        {
            SqlParameter param = new SqlParameter(paramName, type, size);

            //If we're converting times to UTC before storing them & this is a DateTime, convert to UTC now
            if (Settings.TimesStoredInUtc && value is DateTime)
            {
                DateTime dtValue = (DateTime) value;
                param.Value = dtValue.ToUniversalTime();
            }
            else //Otherwise pass the value as is to the DB
            {
                param.Value = value ?? DBNull.Value;
            }

            return parameters.Add(param);
        }

        #endregion

        #region Multiple Values

        public static SqlParameter AddWithValue(this SqlParameterCollection parameters, string paramName,
            IEnumerable<object> values, SqlDbTypeSize typeSize, bool nullable = true)
        {
            // TODO: Should this logic be moved to SqlDbTypeSize.ToSqlMetaData() ??
            // Since we don't do anything with the sizes, all table types get declared as max length.
            //  It wouldn't make sense to try and use a char(max) for something that should have a normal
            //  fixed length (e.g. char(3)), so treat all fixed length strings as their variable length equivelants
            switch (typeSize.SqlDbType)
            {
                case SqlDbType.Char:
                    typeSize = new SqlDbTypeSize(SqlDbType.VarChar, typeSize.Size);
                    break;
                case SqlDbType.NChar:
                    typeSize = new SqlDbTypeSize(SqlDbType.NVarChar, typeSize.Size);
                    break;
            }

            return parameters.addWithValue(paramName, values, typeSize, getTableTypeName(typeSize, nullable));
        }

        public static SqlParameter AddWithValue(this SqlParameterCollection parameters, string paramName,
            IEnumerable<long> values, SqlDbTypeSize typeSize)
        {
            return parameters.addWithValue(paramName, values.Cast<object>(), typeSize, getTableTypeName(typeSize, false));
        }

        public static SqlParameter AddWithValue(this SqlParameterCollection parameters, string paramName,
            IEnumerable<int> values, SqlDbTypeSize typeSize)
        {
            return parameters.addWithValue(paramName, values.Cast<object>(), typeSize, getTableTypeName(typeSize, false));
        }

        public static SqlParameter AddWithValue(this SqlParameterCollection parameters, string paramName,
            IEnumerable<DateTime> values, SqlDbTypeSize typeSize)
        {
            return parameters.addWithValue(paramName, values.Cast<object>(), typeSize, getTableTypeName(typeSize, false));
        }

        private static string getTableTypeName(SqlDbTypeSize typeSize, bool nullable)
        {
            return String.Format("dbo.TableType_Generic_{0}{1}", typeSize.SqlDbType, nullable ? "_Nullable" : "");
        }

        private static SqlParameter addWithValue(this SqlParameterCollection parameters, string paramName,
            IEnumerable<object> values, SqlDbTypeSize typeSize, string tableTypeName)
        {
            // Validation
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (paramName == null)
            {
                throw new ArgumentNullException(nameof(paramName));
            }
            if (typeSize == null)
            {
                throw new ArgumentNullException(nameof(typeSize));
            }
            if (tableTypeName == null)
            {
                throw new ArgumentNullException(nameof(tableTypeName));
            }

            IEnumerable<SqlDataRecord> dataRecords = toSqlDataRecord(values, typeSize);
            SqlParameter param = new SqlParameter(paramName, SqlDbType.Structured, -1)
            {
                TypeName = tableTypeName,
                Value = dataRecords
            };

            return parameters.Add(param);
        }

        /// <summary>
        /// Takes an IEnumerable of values equivelant to a single SQL Server types (int => int, long => bigint etc...) 
        /// and returns an IEnumerable&lt;SqlDataRecord&gt; representing them
        /// </summary>
        private static IEnumerable<SqlDataRecord> toSqlDataRecord(IEnumerable<object> enumerableValues,
            SqlDbTypeSize typeSize)
        {
            // Validation
            if (enumerableValues == null)
            {
                throw new ArgumentNullException(nameof(enumerableValues));

            }

            // Optimisation: Prevent multiple enumerations
            object[] values = enumerableValues as object[] ?? enumerableValues.ToArray();

            // SQL Server expects null instead of 0 rows
            if (!values.Any())
            {
                return null;
            }

            // If we're converting times to UTC before storing them & these are DateTimes, convert to UTC now
            if (Settings.TimesStoredInUtc && values[0] is DateTime)
            {
                object[] utcValues = new object[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    DateTime dt = (DateTime) values[i];
                    utcValues[i] = dt.ToUniversalTime();
                }
                values = utcValues;
            }

            IEnumerable<object> convertedValues;
            SqlMetaData valueMetaData = calculateSqlMetaData(values, typeSize, out convertedValues);
            return convertedValues.Select(value =>
            {
                SqlDataRecord record = new SqlDataRecord(valueMetaData);
                record.SetValues(value);
                return record;
            });
        }

        private static SqlMetaData calculateSqlMetaData(IEnumerable<object> values, SqlDbTypeSize typeSize,
            out IEnumerable<object> convertedValues)
        {
            SqlMetaData valueMetaData;

            // Handle special cases where the data must be converted
            switch (typeSize.SqlDbType)
            {
                // DateTime2 would get inferred as DateTime, which has a smaller range, so pass them as ISO8601 formatted strings (YYYY-MM-DDThh:mm:ss.nnnnnnn)
                case SqlDbType.DateTime2:
                    valueMetaData = new SqlMetaData("v", SqlDbType.Char, "YYYY-MM-DDThh:mm:ss.nnnnnnn".Length);
                    convertedValues =
                        values.Cast<DateTime>()
                            .Select(dt => dt.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture));
                    break;

                // Date would get inferred as DateTime, which has a smaller range, so pass YYYY-MM-DD strings
                case SqlDbType.Date:
                    valueMetaData = new SqlMetaData("v", SqlDbType.Char, 10);
                    convertedValues =
                        values.Cast<DateTime>().Select(dt => dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    break;

                // Not a special case
                default:
                    // No value conversion necessary
                    convertedValues = values;

                    // Try and convert the specified Type Size to an SqlMetaData object, but some types (e.g. Int) 
                    //  we can't pass to the constructor, but we can let SqlMetaData infer them from the data,
                    //  so do that.
                    if (!typeSize.tryToSqlMetaData("v", out valueMetaData))
                    {
                        valueMetaData = SqlMetaData.InferFromValue(values.First(), "v");
                    }
                    break;
            }

            return valueMetaData;
        }

        #endregion
    }
}
