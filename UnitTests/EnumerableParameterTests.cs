/*
 * Sql Server Helpers Unit Tests
 * Enumerable Parameter Tests
 * Authors:
 *  Josh Keegan 04/01/2017
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
    public class EnumerableParameterTests
    {
        [Test]
        public void TestIntParameters()
        {
            int[] expected = new int[] { 1, 2, 3, 7, 11, -46, int.MinValue, int.MaxValue, 0 };
            SqlDbTypeSize valueField = new SqlDbTypeSize(SqlDbType.Int);

            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText =
                    @"SELECT v 
                    FROM @vals";

                // Make the parameters
                command.Parameters.AddWithValue("@vals", expected, valueField);

                // Run the command
                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();

                List<int> actual = new List<int>(expected.Length);
                while (reader.Read())
                {
                    actual.Add(reader.GetInt("v"));
                }

                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void TestDateTimeParameters()
        {
            DateTime[] expected = new DateTime[] { new DateTime(2000, 1, 1), new DateTime(2017, 1, 4, 13, 51, 33) };
            SqlDbTypeSize valueField = new SqlDbTypeSize(SqlDbType.DateTime);

            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText =
                    @"SELECT v 
                    FROM @vals";

                // Make the parameters
                command.Parameters.AddWithValue("@vals", expected, valueField);

                // Run the command
                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();

                List<DateTime> actual = new List<DateTime>(expected.Length);
                while (reader.Read())
                {
                    actual.Add(reader.GetDateTime("v"));
                }

                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void TestDateTime2Parameters()
        {
            DateTime[] expected = new DateTime[] { new DateTime(2000, 1, 1), new DateTime(2017, 1, 4, 13, 51, 33), DateTime.MinValue, DateTime.MaxValue };
            SqlDbTypeSize valueField = new SqlDbTypeSize(SqlDbType.DateTime2);

            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText =
                    @"SELECT v 
                    FROM @vals";

                // Make the parameters
                command.Parameters.AddWithValue("@vals", expected, valueField);

                // Run the command
                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();

                List<DateTime> actual = new List<DateTime>(expected.Length);
                while (reader.Read())
                {
                    actual.Add(reader.GetDateTime("v"));
                }

                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void TestDateParameters()
        {
            DateTime[] expected = new DateTime[] { new DateTime(2000, 1, 1), new DateTime(2017, 1, 4), new DateTime(1, 1, 1), new DateTime(9999, 12, 31) };
            SqlDbTypeSize valueField = new SqlDbTypeSize(SqlDbType.Date);

            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText =
                    @"SELECT v 
                    FROM @vals";

                // Make the parameters
                command.Parameters.AddWithValue("@vals", expected, valueField);

                // Run the command
                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();

                List<DateTime> actual = new List<DateTime>(expected.Length);
                while (reader.Read())
                {
                    actual.Add(reader.GetDateTime("v"));
                }

                CollectionAssert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void TestNullParameter()
        {
            int?[] expected = new int?[] { 3, 5, 9, null };
            testNullableInt(expected);
        }

        [Test]
        public void TestNullParameterFirst()
        {
            int?[] expected = new int?[] { null, 3, 5, 9 };
            testNullableInt(expected);

        }

        [Test]
        public void TestNullParameterAll()
        {
            int?[] expected = new int?[] { null, null };
            testNullableInt(expected);

        }

        #region Private Helpers

        private void testNullableInt(int?[] expected)
        {
            SqlDbTypeSize valueField = new SqlDbTypeSize(SqlDbType.Int);

            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText =
                    @"SELECT v
                    FROM @vals";

                // Make the parameters
                command.Parameters.AddWithValue("@vals", expected.Cast<object>(), valueField);

                // Run the command
                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();

                List<int?> actual = new List<int?>();
                while (reader.Read())
                {
                    actual.Add(reader.GetNullableInt("v"));
                }

                CollectionAssert.AreEqual(expected, actual);
            }
        }

        #endregion
    }
}
