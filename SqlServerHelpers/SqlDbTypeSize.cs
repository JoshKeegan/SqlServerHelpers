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
    }
}
