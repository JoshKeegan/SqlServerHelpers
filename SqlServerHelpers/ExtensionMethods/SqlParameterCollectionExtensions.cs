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

            return parameters.AddWithValue(paramName, values, typeSize, getTableTypeName(typeSize, nullable));
        }

        public static SqlParameter AddWithValue(this SqlParameterCollection parameters, string paramName,
            IEnumerable<Guid> values, SqlDbTypeSize typeSize)
        {
            return parameters.AddWithValue(paramName, values.Cast<object>(), typeSize, getTableTypeName(typeSize, false));
        }

        public static SqlParameter AddWithValue(this SqlParameterCollection parameters, string paramName,
            IEnumerable<long> values, SqlDbTypeSize typeSize)
        {
            return parameters.AddWithValue(paramName, values.Cast<object>(), typeSize, getTableTypeName(typeSize, false));
        }

        public static SqlParameter AddWithValue(this SqlParameterCollection parameters, string paramName,
            IEnumerable<int> values, SqlDbTypeSize typeSize)
        {
            return parameters.AddWithValue(paramName, values.Cast<object>(), typeSize, getTableTypeName(typeSize, false));
        }

        public static SqlParameter AddWithValue(this SqlParameterCollection parameters, string paramName,
            IEnumerable<DateTime> values, SqlDbTypeSize typeSize)
        {
            return parameters.AddWithValue(paramName, values.Cast<object>(), typeSize, getTableTypeName(typeSize, false));
        }

        /// <summary>
        /// Add a parameter for a custom User Defined Table Type (UDTT) with a single column.
        /// </summary>
        public static SqlParameter AddWithValue(this SqlParameterCollection parameters, string paramName,
            IEnumerable<object> values, SqlDbTypeSize typeSize, string tableTypeName, string fieldName = "v")
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
            if (fieldName == null)
            {
                throw new ArgumentNullException(nameof(fieldName));
            }

            IEnumerable<SqlDataRecord> dataRecords = toSqlDataRecord(values, typeSize, fieldName);
            SqlParameter param = new SqlParameter(paramName, SqlDbType.Structured, -1)
            {
                TypeName = tableTypeName,
                Value = dataRecords
            };

            return parameters.Add(param);
        }

        /// <summary>
        /// Add a parameter for a custom User Defined Table Type (UDTT) with multiple columns
        /// </summary>
        public static SqlParameter AddWithValue(this SqlParameterCollection parameters, string paramName,
            IEnumerable<IEnumerable<object>> rowValues, IEnumerable<SqlDbTypeSize> fieldTypeSizes, string tableTypeName,
            IEnumerable<string> fieldNames)
        {
            // Validation
            if (paramName == null)
            {
                throw new ArgumentNullException(nameof(paramName));
            }
            if (rowValues == null)
            {
                throw new ArgumentNullException(nameof(rowValues));
            }
            if (fieldTypeSizes == null)
            {
                throw new ArgumentNullException(nameof(fieldTypeSizes));
            }
            if (tableTypeName == null)
            {
                throw new ArgumentNullException(nameof(tableTypeName));
            }
            if (fieldNames == null)
            {
                throw new ArgumentNullException(nameof(fieldNames));
            }

            IEnumerable<SqlDataRecord> dataRecords = toSqlDataRecord(rowValues, fieldTypeSizes, fieldNames);
            SqlParameter param = new SqlParameter(paramName, SqlDbType.Structured, -1)
            {
                TypeName = tableTypeName,
                Value = dataRecords
            };

            return parameters.Add(param);
        }

        private static string getTableTypeName(SqlDbTypeSize typeSize, bool nullable)
        {
            return String.Format("dbo.TableType_Generic_{0}{1}", typeSize.SqlDbType, nullable ? "_Nullable" : "");
        }

        /// <summary>
        /// Takes an IEnumerable of values equivelant to a single SQL Server types (int => int, long => bigint etc...) 
        /// and returns an IEnumerable&lt;SqlDataRecord&gt; representing them
        /// </summary>
        private static IEnumerable<SqlDataRecord> toSqlDataRecord(IEnumerable<object> enumerableValues,
            SqlDbTypeSize typeSize, string fieldName)
        {
            // Optimisation: Prevent multiple enumerations
            object[] values = enumerableValues as object[] ?? enumerableValues.ToArray();

            // Convert the values array to one with one column per row (for the one value)
            object[][] rowValues = new object[values.Length][];
            for (int i = 0; i < values.Length; i++)
            {
                rowValues[i] = new object[] { values[i] };
            }

            return toSqlDataRecord(rowValues, new SqlDbTypeSize[] { typeSize }, new string[] { fieldName });
        }

        // Multi-column version of above
        private static IEnumerable<SqlDataRecord> toSqlDataRecord(IEnumerable<IEnumerable<object>> enumerableRowsValues,
            IEnumerable<SqlDbTypeSize> enumerableFieldTypeSizes, IEnumerable<string> enumerableFieldNames)
        {
            // Validation
            if (enumerableRowsValues == null)
            {
                throw new ArgumentNullException(nameof(enumerableRowsValues));
            }
            if (enumerableFieldTypeSizes == null)
            {
                throw new ArgumentNullException(nameof(enumerableFieldTypeSizes));
            }
            if (enumerableFieldNames == null)
            {
                throw new ArgumentNullException(nameof(enumerableFieldNames));
            }

            // Optimisation: Prevent multiple enumerations
            string[] fieldNames = enumerableFieldNames as string[] ?? enumerableFieldNames.ToArray();

            SqlDbTypeSize[] fieldTypeSizes = enumerableFieldTypeSizes as SqlDbTypeSize[] ??
                                             enumerableFieldTypeSizes.ToArray();

            // Additional validation, check number of fields specified match
            if (fieldNames.Length != fieldTypeSizes.Length)
            {
                throw new ArgumentException("Number of fields specified must all match");
            }

            IEnumerable<object>[] rowsEnumerableValues = enumerableRowsValues as IEnumerable<object>[] ??
                                                         enumerableRowsValues.ToArray();
            object[][] rowsValues = new object[rowsEnumerableValues.Length][];
            for (int i = 0; i < rowsValues.Length; i++)
            {
                rowsValues[i] = rowsEnumerableValues[i] as object[] ?? rowsEnumerableValues[i].ToArray();

                // Additional validation, check number of fields specified match
                if (rowsValues[i].Length != fieldNames.Length)
                {
                    throw new ArgumentException("Number of fields specified must all match");
                }
            }

            // SQL Server expects null instead of 0 rows
            if (!rowsValues.Any())
            {
                return null;
            }

            // If we're converting times to UTC before storing them & a field is a DateTime, convert to UTC now
            if (Settings.TimesStoredInUtc)
            {
                List<int> dateTimeFieldIndices = new List<int>();
                for (int i = 0; i < rowsValues[0].Length; i++)
                {
                    if (rowsValues[0][i] is DateTime)
                    {
                        dateTimeFieldIndices.Add(i);
                    }
                }

                // If there are any DateTime fields
                if (dateTimeFieldIndices.Any())
                {
                    // Go through each row converting these indices to UTC
                    for (int i = 0; i < rowsValues.Length; i++)
                    {
                        for (int j = 0; j < dateTimeFieldIndices.Count; j++)
                        {
                            DateTime dt = (DateTime) rowsValues[i][j];
                            rowsValues[i][j] = dt.ToUniversalTime();
                        }
                    }
                }
            }

            // Get values by column so we can convert them if necessary
            object[][] columnValues = new object[rowsValues[0].Length][];
            for (int i = 0; i < columnValues.Length; i++)
            {
                columnValues[i] = new object[rowsValues.Length];

                for (int j = 0; j < rowsValues.Length; j++)
                {
                    columnValues[i][j] = rowsValues[j][i];
                }
            }

            // Calculate the SqlMetaData for each column, also converting the values at the same time (if required)
            object[][] convertedColumnValues = new object[columnValues.Length][];
            SqlMetaData[] columnMetaDatas = new SqlMetaData[columnValues.Length];
            for (int i = 0; i < columnValues.Length; i++)
            {
                IEnumerable<object> enumerableConvertedValues;
                columnMetaDatas[i] = calculateSqlMetaData(columnValues[i], fieldTypeSizes[i],
                    out enumerableConvertedValues, fieldNames[i]);

                // Store converted column values
                convertedColumnValues[i] = enumerableConvertedValues as object[] ?? enumerableConvertedValues.ToArray();
            }

            // Make the SqlDataRecords
            SqlDataRecord[] dataRecords = new SqlDataRecord[rowsValues.Length];
            for (int i = 0; i < dataRecords.Length; i++)
            {
                // Make base record with meta data for all columns
                SqlDataRecord record = new SqlDataRecord(columnMetaDatas);

                // Get the data for this row
                object[] rowData = new object[convertedColumnValues.Length];
                for (int j = 0; j < rowData.Length; j++)
                {
                    rowData[j] = convertedColumnValues[j][i];
                }
                record.SetValues(rowData);

                dataRecords[i] = record;
            }
            return dataRecords;
        }

        private static SqlMetaData calculateSqlMetaData(IEnumerable<object> values, SqlDbTypeSize typeSize,
            out IEnumerable<object> convertedValues, string fieldName)
        {
            SqlMetaData valueMetaData;

            // Handle special cases where the data must be converted
            switch (typeSize.SqlDbType)
            {
                // DateTime2 would get inferred as DateTime, which has a smaller range, so pass them as ISO8601 formatted strings (YYYY-MM-DDThh:mm:ss.nnnnnnn)
                case SqlDbType.DateTime2:
                    valueMetaData = new SqlMetaData(fieldName, SqlDbType.Char, "YYYY-MM-DDThh:mm:ss.nnnnnnn".Length);
                    convertedValues =
                        values.Cast<DateTime>()
                            .Select(dt => dt.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffff", CultureInfo.InvariantCulture));
                    break;

                // Date would get inferred as DateTime, which has a smaller range, so pass YYYY-MM-DD strings
                case SqlDbType.Date:
                    valueMetaData = new SqlMetaData(fieldName, SqlDbType.Char, 10);
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
                    if (!typeSize.tryToSqlMetaData(fieldName, out valueMetaData))
                    {
                        valueMetaData = SqlMetaData.InferFromValue(values.First(), fieldName);
                    }
                    break;
            }

            return valueMetaData;
        }

        #endregion
    }
}
