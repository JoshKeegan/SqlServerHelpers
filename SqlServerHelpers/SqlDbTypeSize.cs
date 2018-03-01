/*
 * SqlServerHelpers
 * SqlDbTypeSize - Holds an SqlDbType and a size for each DB field
 * Authors:
 *  Josh Keegan 05/09/2016
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.SqlServer.Server;

namespace SqlServerHelpers
{
    public class SqlDbTypeSize
    {
        // Private Variables
        private int TypeMaxLength
        {
            get
            {
                //TODO: Fields will be missing here, add as necessary.
                //  Reference: https://msdn.microsoft.com/en-us/library/system.data.sqldbtype%28v=vs.110%29.aspx
                switch (SqlDbType)
                {
                    case SqlDbType.Char:
                        return 8000;
                    case SqlDbType.NChar:
                        return 4000;
                    case SqlDbType.NText:
                        return 1073741823;
                    case SqlDbType.NVarChar:
                        return 4000;
                    case SqlDbType.Text:
                        return 2147483647;
                    case SqlDbType.VarChar:
                        return 8000;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        // Public variables
        public readonly SqlDbType SqlDbType;
        public readonly int Size;
        public int MaxLength => Size == -1 ? TypeMaxLength : Size;

        // Constructors
        public SqlDbTypeSize(SqlDbType sqlDbType, int size = -1)
        {
            SqlDbType = sqlDbType;
            Size = size;
        }

        // Internal Methods
        /// <summary>
        /// Try to get the SQL Metadata for this SqlDbTypeSize.
        /// Note that for some types it would be possible for a metadata to be returned here, but they are not.
        /// This is because it makes more sense to send it to SQL Server as varchar in order to not lose precision or scale.
        /// These should be handled before calling this method.
        /// </summary>
        internal bool tryToSqlMetaData(string name, out SqlMetaData metaData)
        {
            // SqlMetaData requires a size the following DbTypes (from https://msdn.microsoft.com/en-us/library/ms127243(v=vs.110).aspx):
            //  Binary, Char, Image, NChar, Ntext, NVarChar, Text, VarBinary, VarChar
            //  It will get strongly typed by SQL Server later because of the table type being specified.

            // The following must not be supplied with a length
            //  Bit, BigInt, DateTime, Decimal, Float, Int, Money, Numeric (doesn't exist in .NET), SmallDateTime, 
            //  SmallInt, SmallMoney, TimeStamp, TinyInt, UniqueIdentifier, Xml

            //  Inferred types (that aren't in either of the lists above) appear to be:
            //  DateTimeOffset, Time, Variant

            // So this method tries to convert types where possible to the first or second list, except
            //  where it would be preferable to let SqlMetaData.InferFromValue handle it to get a type from the 
            //  second list

            bool success = false;
            SqlDbType type = SqlDbType;
            int size = Size;
            bool useSize = true;
            object inferFrom = null;
            switch (type)
            {
                /*
                 * Any input type accepted with size
                 */
                case SqlDbType.Binary:
                case SqlDbType.Char:
                case SqlDbType.Image:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarBinary:
                case SqlDbType.VarChar:
                    success = true;
                    break;
                // This is a guess & is untested
                case SqlDbType.Udt:
                    type = SqlDbType.VarBinary; 
                    success = true;
                    break;
                /*
                 * Any input type already accepted without size
                 */
                case SqlDbType.Bit:
                case SqlDbType.BigInt:
                case SqlDbType.DateTime:
                case SqlDbType.Decimal:
                case SqlDbType.Float:
                case SqlDbType.Int:
                case SqlDbType.Money:
                case SqlDbType.SmallDateTime:
                case SqlDbType.SmallInt:
                case SqlDbType.SmallMoney:
                case SqlDbType.Timestamp:
                case SqlDbType.TinyInt:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.Xml:
                    success = true;
                    useSize = false;
                    break;
                // Real is equivelant to float
                case SqlDbType.Real:
                    type = SqlDbType.Float;
                    success = true;
                    useSize = false;
                    break;
                /*
                 * Inferred
                 */
                case SqlDbType.DateTimeOffset:
                    success = true;
                    inferFrom = new DateTimeOffset();
                    break;
                case SqlDbType.Time:
                    success = true;
                    inferFrom = new TimeSpan(1, 0, 0);
                    break;
                case SqlDbType.Variant:
                    success = true;
                    inferFrom = new object();
                    break;
                /*
                 * Unsupported
                 */
                // Structured is used to send DataTables from .NET to SQL Server as a User-defined table type
                case SqlDbType.Structured:
                    throw new InvalidOperationException(
                        "Cannot use Structured SqlDbType in SqlMetaData and there is no equivelant");
            }

            // If we've successfully determined what to do to get the meta data, do so
            if (success)
            {
                // If we're inferring from a value
                if (inferFrom != null)
                {
                    metaData = SqlMetaData.InferFromValue(inferFrom, name);
                }
                // Else if we can constuct the SqlMetaData and must supply a length
                else if (useSize)
                {
                    metaData = new SqlMetaData(name, type, size);
                }
                // Otherwise we can construct the SqlMetaData and must not supply a length
                else
                {
                    metaData = new SqlMetaData(name, type);
                }
            }
            else // Otherwise, set to null, and log
            {
                metaData = null;

                Logging.Write("Couldn't calculate the SqlMetaData for SqlDbType {0}. " +
                              "Please open a bug at https://github.com/JoshKeegan/SqlServerHelpers/issues with the SqlDbType in question and code to reproduce this.",
                    type);
            }
            return success;
        }
    }
}
