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

        // Public Methods
        public SqlMetaData ToSqlMetaData(string name = "v")
        {
            // SqlMetaData can only work with the following DbTypes (from https://msdn.microsoft.com/en-us/library/ms127243(v=vs.110).aspx):
            //  Binary, Char, Image, NChar, Ntext, NVarChar, Text, VarBinary, VarChar
            //  It will get strongly typed by SQL Server later because of the table type being specified.

            // However, SqlMetaData.InferFromValue appears to also work with other DbTypes.
            //  Inferred types appear to be:
            //  BigInt, Bit, DateTime, DateTimeOffset, Decimal, Float, Int, Money, NVarChar, Real, SmallInt, Time, TinyInt, 
            //  UniqueIdentifier, VarBinary, Variant, Xml

            // Combined with the documented types, this gives an allowed list of:
            //  BigInt, Binary, Bit, Char, DateTime, DateTimeOffset, Decimal, Float, Image, Int, Money, NChar, Ntext, 
            //  NVarChar, Real, SmallInt, Text, Time, TinyInt, UniqueIdentifier, VarBinary, VarChar, Variant, Xml
            
            // The following is based on the above list, the observed behaviour of SqlMetaData.InferFromValue
            //  and some educated guesses:
            SqlDbType type = SqlDbType;
            int size = Size;
            switch (type)
            {
                case SqlDbType.SmallDateTime:
                    type = SqlDbType.DateTime;
                    break;
                case SqlDbType.SmallMoney:
                    type = SqlDbType.Money;
                    break;
                case SqlDbType.Timestamp: // Note that timestamp has been renamed to rowversion in SQL Server 2008
                    type = SqlDbType.Binary;
                    size = 8; // https://msdn.microsoft.com/en-GB/library/ms182776.aspx
                    break;
                case SqlDbType.Udt:
                    type = SqlDbType.VarBinary; // This is a guess & is untested
                    break;
                case SqlDbType.Structured:
                    // Structured is used to send DataTables from .NET to SQL Server as a User-defined table type
                    throw new InvalidOperationException(
                        "Cannot use Structured SqlDbType in SqlMetaData and there is no equivelant");
                case SqlDbType.Date:
                    // Don't use DateTime for Date, as the full range of Date is greater
                    // TODO: Test if datetime2 is supported. If so, use that
                    type = SqlDbType.VarChar;
                    break;
                case SqlDbType.DateTime2:
                    // TODO: Check if this is supported. Docs don't say it is & SqlMetaData.InferFromValue
                    //  returns DateTime, but that could just be for compatibility with now deprecated versions of SQL Server,
                    //  which we don't need to worry about
                    type = SqlDbType.VarChar;
                    break;
            }

            return new SqlMetaData(name, type, size);
        }
    }
}
