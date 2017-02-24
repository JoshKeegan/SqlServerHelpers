﻿/*
 * Sql Server Helpers Unit Tests
 * Custom UDTT (User Defined Table Types) Tests
 * Authors:
 *  Josh Keegan 24/02/2017
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
    public class CustomUdttTests
    {
        [SetUp]
        public void SetUp()
        {
            // Make the UDTTs to be used
            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText =
                    @"/* Make UDTT for TestSingleParameter */
                    IF TYPE_ID('udtt_UnitTests_int_pk') IS NOT NULL
	                    DROP TYPE udtt_UnitTests_int_pk;

                    CREATE TYPE udtt_UnitTests_int_pk AS TABLE
                    (
	                    i int NOT NULL,
	                    PRIMARY KEY CLUSTERED
	                    (
		                    i ASC
	                    )
                    );";

                // Run the command
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Remove the UDTTs we've now used
            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText =
                    @"/* UDTT for TestSingleParameter */
                    DROP TYPE udtt_UnitTests_int_pk;";

                // Run the command
                command.Prepare();
                command.ExecuteNonQuery();
            }
        }

        [Test]
        public void TestSingleParameter()
        {
            // Could easily use built in generic table type, but might want something custom (like a primary key)
            int[] toAdd = new int[] { 1, 2, 3, 7, 11, -46, int.MinValue, int.MaxValue, 0 };
            SqlDbTypeSize valueField = new SqlDbTypeSize(SqlDbType.Int);

            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText =
                    @"SELECT i 
                    FROM @vals";

                // Make the parameters
                command.Parameters.AddWithValue("@vals", toAdd.Cast<object>(), valueField, "udtt_UnitTests_int_pk",
                    "i");

                // Run the command
                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();

                List<int> actual = new List<int>(toAdd.Length);
                while (reader.Read())
                {
                    actual.Add(reader.GetInt("i"));
                }

                // Has PK ASC, so values will come back out sorted
                IEnumerable<int> expected = toAdd.OrderBy(i => i);

                CollectionAssert.AreEqual(expected, actual);
            }
        }
    }
}
