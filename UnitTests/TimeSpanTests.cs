/*
 * Sql Server Helpers Unit Tests
 * TimeSpan tests (SQL Server time type)
 * Authors:
 *  Josh Keegan 27/05/2017
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using SqlServerHelpers;
using SqlServerHelpers.ExtensionMethods;

namespace UnitTests
{
    [TestFixture]
    public class TimeSpanTests
    {
        [Test]
        public void TestToFromScalarTimeSpan()
        {
            TimeSpan expected = new TimeSpan(1, 3, 37);
            SqlDbTypeSize valueField = new SqlDbTypeSize(SqlDbType.Time);

            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText = "SELECT @ts AS ts";

                // Add the parameters
                command.Parameters.AddWithValue("@ts", expected, valueField);

                // Run the command
                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();
                TimeSpan actual = reader.GetTimeSpan("ts");

                Assert.AreEqual(expected, actual);
            }
        }
    }
}
