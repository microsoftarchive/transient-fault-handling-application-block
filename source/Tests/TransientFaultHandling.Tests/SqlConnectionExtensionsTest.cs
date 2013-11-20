// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Tests
{
    [TestClass]
    public class SqlConnectionExtensionsTest
    {
        [TestInitialize]
        public void Setup()
        {
            RetryPolicyFactory.CreateDefault();
        }

        [TestCleanup]
        public void Cleanup()
        {
            RetryPolicyFactory.SetRetryManager(null, false);
        }

        [Description("F5.1.1")]
        [Priority(1)]
        [TestMethod]
        public void TestSqlConnectionExtensions()
        {
            using (SqlConnection connection = new SqlConnection(TestSqlSupport.SqlDatabaseConnectionString))
            {
                using (SqlCommand command = new SqlCommand("SELECT [ProductCategoryID], [Name] FROM [SalesLT].[ProductCategory]", connection))
                {
                    connection.OpenWithRetry();

                    using (IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);

                            Trace.WriteLine(string.Format("{0}: {1}", id, name));
                        }

                        reader.Close();
                    }

                    connection.Close();
                }
            }
        }
    }
}
