/*
 * Sql Server Helpers Unit Tests
 * Time Conversation Tests
 * Authors:
 *  Josh Keegan 04/01/2016
 * 
 * Note that these tests could depend on the system clock & its current timezone
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
    public class TimeConversionTests
    {
        [SetUp]
        public void SetUp()
        {
            // Make sure we're considering times to be stored in UTC
            Settings.TimesStoredInUtc = true;
        }

        [Test]
        public void TestToFromUtcScalarDateTime()
        {
            DateTime expected = new DateTime(1993, 8, 23, 12, 0, 0);
            SqlDbTypeSize valueField = new SqlDbTypeSize(SqlDbType.DateTime);

            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText = "SELECT @dt AS dt";

                // Make the parameters
                command.Parameters.AddWithValue("@dt", expected, valueField);

                // Run the command
                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();
                DateTime actual = reader.GetDateTime("dt");

                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void TestToFromUtcScalarDateTime2()
        {
            DateTime expected = new DateTime(1993, 8, 23, 12, 0, 0);
            SqlDbTypeSize valueField = new SqlDbTypeSize(SqlDbType.DateTime2);

            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText = "SELECT @dt AS dt";

                // Make the parameters
                command.Parameters.AddWithValue("@dt", expected, valueField);

                // Run the command
                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();
                DateTime actual = reader.GetDateTime("dt");

                Assert.AreEqual(expected, actual);
            }
        }

        [Test]
        public void TestToFromUtcEnumerableDateTime()
        {
            DateTime[] expected = new DateTime[]
            {
                new DateTime(1993, 8, 23, 12, 0, 0),
                new DateTime(2000, 1, 1)
            };
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
        public void TestToFromUtcEnumerableDateTime2()
        {
            DateTime[] expected = new DateTime[]
            {
                new DateTime(1993, 8, 23, 12, 0, 0),
                new DateTime(2000, 1, 1)
            };
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
    }
}
