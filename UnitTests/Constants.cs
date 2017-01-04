/*
 * SQL Server Helpers Unit Tests
 * Constants
 * Authors:
 *  Josh Keegan 04/01/2017
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public static class Constants
    {
        public static readonly string DATABASE_CONNECTION_STRING =
            ConfigurationManager.ConnectionStrings["db"].ConnectionString;
    }
}
