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
            IEnumerable<long> values, SqlDbTypeSize typeSize)
        {
            return parameters.addWithValue(paramName, values.Cast<object>(), typeSize, "dbo.TableType_Generic_BigInt");
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

            // SqlMetaData can only work with the following DbTypes (from https://msdn.microsoft.com/en-us/library/ms127243(v=vs.110).aspx):
            //  Binary, Char, Image, NChar, Ntext, NVarChar, Text, VarBinary, VarChar
            //  Ite will get strongly typed by SQL Server later because of the table type being specified.
            //  Just let it infer it for now
            // TODO: Could write our own logic here based on typeSize.SqlDbType?? Don't know if there would be any real benefit though as
            //  the limitations of SqlMetaData would never let us get the types right anyway so SQL Server will always coerce them :s
            //SqlMetaData valueMetaData = new SqlMetaData("v", typeSize.SqlDbType, typeSize.Size);
            SqlMetaData valueMetaData = SqlMetaData.InferFromValue(values.First(), "v");
            return values.Select(value =>
            {
                SqlDataRecord record = new SqlDataRecord(valueMetaData);
                record.SetValues(value);
                return record;
            });
        }

        #endregion
    }
}
