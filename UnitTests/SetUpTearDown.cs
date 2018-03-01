/*
 * Sql Server Helpers Unit Tests
 * SetUptearDown - contains code that runs set up & tear down at namespace level
 * Authors:
 *  Josh Keegan 01/03/2018
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using SqlServerHelpers;

namespace UnitTests
{
    [SetUpFixture]
    public class SetUpTearDown
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Settings.LogWriter = Console.WriteLine;
        }
    }
}
