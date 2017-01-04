/*
 * SQL Server Helpers Unit Tests
 * Constants
 * Authors:
 *  Josh Keegan 04/01/2017
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public static class Constants
    {
        public const string DATABASE_CONNECTION_STRING =
            //@"Server=josh-pc; Database=KLog; User ID=klogDemoUser; pwd=wow_much_security";
            @"Server=(local)\SQL2016;Database=SqlServerHelpers;User ID=sa;Password=Password12!";
    }
}
