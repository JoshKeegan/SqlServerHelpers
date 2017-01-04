/*
 * SQL Server Helpers Unit Tests
 * Regression Test for Bug 1 - https://github.com/JoshKeegan/SqlServerHelpers/issues/1
 * Authors:
 *  Josh Keegan 04/01/2017
 */

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using NUnit.Framework;

using SqlServerHelpers;
using SqlServerHelpers.ExtensionMethods;

namespace UnitTests.Regression
{
    [TestFixture]
    public class Bug1
    {
        [Test]
        public void Bug1Repro()
        {
            // Strings of lengths longer than the first were being silently trunctaed to the length of the first one
            //  so just test by selecting the strings back out, with some later strings being longer than the first
            string[] expected = new string[] { "abd", "abcdefg", "def", "fghjkm" };
            SqlDbTypeSize valueField = new SqlDbTypeSize(SqlDbType.VarChar);

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

                List<string> actual = new List<string>(expected.Length);
                while (reader.Read())
                {
                    actual.Add(reader.GetString("v"));
                }

                CollectionAssert.AreEqual(expected, actual);
            }
        }
    }
}
