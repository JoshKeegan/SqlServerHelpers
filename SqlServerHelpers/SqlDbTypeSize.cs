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
        internal bool tryToSqlMetaData(string name, out SqlMetaData metaData)
        {
            // SqlMetaData can only work with the following DbTypes (from https://msdn.microsoft.com/en-us/library/ms127243(v=vs.110).aspx):
            //  Binary, Char, Image, NChar, Ntext, NVarChar, Text, VarBinary, VarChar
            //  It will get strongly typed by SQL Server later because of the table type being specified.

            // However, SqlMetaData.InferFromValue appears to also work with other DbTypes.
            //  Inferred types appear to be:
            //  BigInt, Bit, DateTime, DateTimeOffset, Decimal, Float, Int, Money, NVarChar, Real, SmallInt, Time, TinyInt, 
            //  UniqueIdentifier, VarBinary, Variant, Xml

            // So this method tries to convert types where possible to the first list, except
            //  where it would be preferable to let SqlMetaData.InferFromValue handle it to get a type from the 
            //  second list

            bool success = false;
            SqlDbType type = SqlDbType;
            int size = Size;
            switch (type)
            {
                // Any input type already accepted is fine
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
                // Note that timestamp has been renamed to rowversion in SQL Server 2008
                case SqlDbType.Timestamp: 
                    type = SqlDbType.Binary;
                    size = 8; // https://msdn.microsoft.com/en-GB/library/ms182776.aspx
                    success = true;
                    break;
                // This is a guess & is untested
                case SqlDbType.Udt:
                    type = SqlDbType.VarBinary; 
                    success = true;
                    break;
                // Structured is used to send DataTables from .NET to SQL Server as a User-defined table type
                case SqlDbType.Structured:
                    throw new InvalidOperationException(
                        "Cannot use Structured SqlDbType in SqlMetaData and there is no equivelant");
            }

            metaData = success ? new SqlMetaData(name, type, size) : null;
            return success;
        }
    }
}
