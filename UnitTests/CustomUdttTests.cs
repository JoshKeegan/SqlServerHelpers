/*
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
                    );

                    /* Make UDTT for TestMultiParameter */
                    IF TYPE_ID('udtt_UnitTests_int_long') IS NOT NULL
	                    DROP TYPE udtt_UnitTests_int_long;

                    CREATE TYPE udtt_UnitTests_int_long AS TABLE
                    (
	                    i int NOT NULL,
                        l bigint NOT NULL
                    );
                    
                    /* Make UDTT for TestNullParameter */
                    IF TYPE_ID('udtt_UnitTests_int_nullable') IS NOT NULL
	                    DROP TYPE udtt_UnitTests_int_nullable;

                    CREATE TYPE udtt_UnitTests_int_nullable AS TABLE
                    (
	                    i int NULL
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
                    DROP TYPE udtt_UnitTests_int_pk;

                    /* UDTT for TestMultiParameter */
                    DROP TYPE udtt_UnitTests_int_long;

                    /* UDTT for TestNullParameter */
                    DROP TYPE udtt_UnitTests_int_nullable;";

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

        [Test]
        public void TestMultiParameter()
        {
            int[] expectedInts = new int[] { 1, 2, 3, 7, 11, -46, int.MinValue, int.MaxValue, 0 };
            long[] expectedLongs = new long[] { -3, 2, 1, 11, 7, -46, 0, long.MaxValue, long.MinValue };
            Assert.AreEqual(expectedInts.Length, expectedLongs.Length);

            object[][] objRows = new object[expectedInts.Length][];
            for (int i = 0; i < objRows.Length; i++)
            {
                objRows[i] = new object[] { expectedInts[i], expectedLongs[i] };
            }

            SqlDbTypeSize[] fieldTypeSizes = new SqlDbTypeSize[]
            {
                new SqlDbTypeSize(SqlDbType.Int),
                new SqlDbTypeSize(SqlDbType.BigInt)
            };

            string[] fieldNames = new string[]
            {
                "i",
                "l"
            };

            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText =
                    @"SELECT *
                    FROM @vals";

                // Make the parameters
                command.Parameters.AddWithValue("@vals", objRows, fieldTypeSizes, "udtt_UnitTests_int_long", fieldNames);

                // Run the command
                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();

                List<int> actualInts = new List<int>();
                List<long> actualLongs = new List<long>();
                while (reader.Read())
                {
                    actualInts.Add(reader.GetInt("i"));
                    actualLongs.Add(reader.GetLong("l"));
                }

                CollectionAssert.AreEqual(expectedInts, actualInts);
                CollectionAssert.AreEqual(expectedLongs, actualLongs);
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

        #region Private Methods

        private void testNullableInt(int?[] expected)
        {
            IEnumerable<object[]> objRows = expected.Select(i => new object[] { i });
            SqlDbTypeSize[] fieldTypeSizes = new SqlDbTypeSize[]
            {
                new SqlDbTypeSize(SqlDbType.Int)
            };
            string[] fieldNames = new string[]
            {
                "i"
            };

            using (SqlConnection conn = new SqlConnection(Constants.DATABASE_CONNECTION_STRING))
            using (SqlCommand command = conn.GetSqlCommand())
            {
                conn.Open();

                // Build the command
                command.CommandText =
                    @"SELECT *
                    FROM @vals";

                // Make the parameters
                command.Parameters.AddWithValue("@vals", objRows, fieldTypeSizes, "udtt_UnitTests_int_nullable", fieldNames);
            }
        }

        #endregion
    }
}
