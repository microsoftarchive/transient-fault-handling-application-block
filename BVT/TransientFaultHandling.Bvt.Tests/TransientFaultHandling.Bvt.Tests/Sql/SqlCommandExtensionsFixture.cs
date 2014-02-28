// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests.TestObjects;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests.Sql
{
    [TestClass]
    public class SqlCommandExtensionsFixture
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString;
        private SystemConfigurationSource configurationSource;
        private RetryPolicyConfigurationSettings retryPolicySettings;
        private RetryManager retryManager;

        [TestInitialize]
        public void Initialize()
        {
            this.configurationSource = new SystemConfigurationSource();
            this.retryPolicySettings = RetryPolicyConfigurationSettings.GetRetryPolicySettings(this.configurationSource);
            this.retryManager = retryPolicySettings.BuildRetryManager();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (this.configurationSource != null)
            {
                this.configurationSource.Dispose();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ThrowsExceptionWhenAllRetriesFailDuringReaderExecutionWithRetryPolicy()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            try
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ErrorRaisingReader";
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 7 });
                command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                using (var reader = command.ExecuteReaderWithRetry(policy))
                {
                    while (reader.Read())
                    {
                    }
                }
            }
            catch (Exception)
            {
                Assert.AreEqual(5, count);
                connection.Close();
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
                throw;
            }

            Assert.Fail("Test should throw");
        }

        [TestMethod]
        public void ExecutesReaderWithRetryPolicyWhenSomeRetriesFailAndThenSucceeds()
        {   
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            int rowcount = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ErrorRaisingReader";
            command.Connection = connection;
            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 4 });
            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
            using (var reader = command.ExecuteReaderWithRetry(policy))
            {
                while (reader.Read())
                {
                    rowcount++;
                }
            }

            Assert.AreEqual(3, count);
            connection.Close();
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
            Assert.AreEqual(10, rowcount);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ThrowsExceptionWhenAllRetriesFailDuringReaderExecutionWithRetryPolicyAndConnectionRetryPolicy()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = RetryPolicyFactory.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            try
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ErrorRaisingReader";
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 7 });
                command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                using (var reader = command.ExecuteReaderWithRetry(policy, policy))
                {
                    while (reader.Read())
                    {
                    }
                }
            }
            catch (Exception)
            {
                Assert.AreEqual(5, count);
                connection.Close();
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
                throw;
            }

            Assert.Fail("Test should throw");
        }

        [TestMethod]
        public void ExecutesReaderWithRetryPolicyAndConnectionRetryPolicyWhenSomeRetriesFailAndThenSucceeds()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            int rowcount = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ErrorRaisingReader";
            command.Connection = connection;
            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 3 });
            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
            using (var reader = command.ExecuteReaderWithRetry(policy, policy))
            {
                while (reader.Read())
                {
                    rowcount++;
                }
            }

            Assert.AreEqual(2, count);
            connection.Close();
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
            Assert.AreEqual(10, rowcount);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ThrowsExceptionWhenAllRetriesFailDuringReaderExecutionWithRetryPolicyAndSqlCommandBehavior()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };
            
            try
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ErrorRaisingReader";
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 7 });
                command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                using (var reader = command.ExecuteReaderWithRetry(CommandBehavior.CloseConnection, policy))
                {
                    while (reader.Read())
                    {
                    }
                }
            }
            catch (Exception)
            {
                Assert.AreEqual(5, count);
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
                throw;
            }

            Assert.Fail("Test should throw");
        }

        [TestMethod]
        public void ExecutesReaderWithRetryPolicyAndSqlCommandBehaviorWhenSomeRetriesFailAndThenSucceeds()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            int rowCount = 0;
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ErrorRaisingReader";
            command.Connection = connection;
            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 4 });
            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
            using (var reader = command.ExecuteReaderWithRetry(CommandBehavior.CloseConnection, policy))
            {
                while (reader.Read())
                {
                    rowCount++;
                }
            }

            Assert.AreEqual(3, count);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
            Assert.AreEqual(10, rowCount);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ThrowsExceptionWhenAllRetriesFailDuringReaderExecutionWithRetryPolicyAndSqlCommandBehaviorAndConnectionRetryPolicy()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            try
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ErrorRaisingReader";
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 7 });
                command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                using (var reader = command.ExecuteReaderWithRetry(CommandBehavior.CloseConnection, policy, policy))
                {
                    while (reader.Read())
                    {
                    }
                }
            }
            catch (Exception)
            {
                Assert.AreEqual(5, count);
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
                throw;
            }

            Assert.Fail("Test should throw");
        }

        [TestMethod]
        public void ExecutesReaderWithRetryPolicyAndSqlCommandBehaviorAndConnectionRetryPolicyWhenSomeRetriesFailAndThenSucceeds()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            int rowcount = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ErrorRaisingReader";
            command.Connection = connection;
            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 3 });
            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
            using (var reader = command.ExecuteReaderWithRetry(CommandBehavior.CloseConnection, policy, policy))
            {
                while (reader.Read())
                {
                    rowcount++;
                }
            }

            Assert.AreEqual(2, count);
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
            Assert.AreEqual(10, rowcount);
        }

        [TestMethod]
        public void ExecutesNonQueryWithRetryPolicyWhenSomeRetriesFailAndThenSucceeds()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            int rowCount = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ErrorRaisingForExecuteNonQuery";
            command.Connection = connection;
            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 4 });
            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });

            using (connection)
            {
                using (command)
                {
                    rowCount = (int)command.ExecuteNonQueryWithRetry(policy);
                }

                connection.Close();
            }

            Assert.AreEqual(1, rowCount);
            Assert.AreEqual(3, count);
            connection.Close();
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ThrowsExceptionWhenAllRetriesFailDuringNonQueryExecutionWithRetryPolicy()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            int rowCount = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            try
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ErrorRaisingForExecuteNonQuery";
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 7 });
                command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });

                using (connection)
                {
                    using (command)
                    {
                        rowCount = (int)command.ExecuteNonQueryWithRetry(policy);
                    }

                    connection.Close();
                }

                Assert.AreEqual(1, rowCount);
            }
            catch (Exception)
            {
                Assert.AreEqual(5, count);

                connection.Close();
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
                throw;
            }

            Assert.Fail("Test should throw");
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ThrowsExceptionWhenAllRetriesFailDuringScalarExecutionWithRetryPolicy()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            int rowcount = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            try
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ErrorRaisingScalar";
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 7 });
                command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                rowcount = (int)command.ExecuteScalarWithRetry(policy);
            }
            catch (Exception)
            {
                Assert.AreEqual(5, count);
                connection.Close();
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
                throw;
            }

            Assert.Fail("Test should throw");
        }

        [TestMethod]
        public void ExecutesScalarWithRetryPolicyWhenSomeRetriesFailAndThenSucceeds()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            int totalrowcount = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ErrorRaisingScalar";
            command.Connection = connection;
            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 4 });
            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
            totalrowcount = (int)command.ExecuteScalarWithRetry(policy);

            Assert.AreEqual(3, count);
            connection.Close();
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
            Assert.AreEqual(10, totalrowcount);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ThrowsExceptionWhenAllRetriesFailDuringScalarExecutionWithRetryPolicyAndConnectionRetryPolicy()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            int rowcount = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            try
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ErrorRaisingScalar";
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 7 });
                command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                rowcount = (int)command.ExecuteScalarWithRetry(policy, policy);
            }
            catch (Exception)
            {
                Assert.AreEqual(5, count);
                connection.Close();
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
                throw;
            }

            Assert.Fail("Test should throw");
        }

        [TestMethod]
        public void ExecutesScalarWithRetryPolicyAndConnectionRetryPolicyWhenSomeRetriesFailAndThenSucceeds()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            int rowcount = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ErrorRaisingScalar";
            command.Connection = connection;
            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 2 });
            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
            rowcount = (int)command.ExecuteScalarWithRetry(policy, policy);
            Assert.AreEqual(1, count);

            connection.Close();
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
            Assert.AreEqual(10, rowcount);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ThrowsExceptionWhenAllRetriesFailDuringXmlReaderExecutionWithRetryPolicy()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            try
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ErrorRaisingXMLReader";
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 7 });
                command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                using (var reader = command.ExecuteXmlReaderWithRetry(policy))
                {
                    while (reader.Read())
                    {
                    }
                }
            }
            catch (Exception)
            {
                Assert.AreEqual(5, count);

                connection.Close();
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
                throw;
            }

            Assert.Fail("Test should throw");
        }

        [TestMethod]
        public void ExecutesXmlReaderWithRetryPolicyWhenSomeRetriesFailAndThenSucceeds()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            int rowcount = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ErrorRaisingXMLReader";
            command.Connection = connection;
            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 4 });
            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
            using (XmlReader reader = command.ExecuteXmlReaderWithRetry(policy))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            rowcount++;
                            break;
                    }
                }
            }

            Assert.AreEqual(3, count);
            connection.Close();
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
            Assert.AreEqual(10, rowcount);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ThrowsExceptionWhenAllRetriesFailDuringXmlReaderExecutionWithRetryPolicyAndConnectionRetryPolicy()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            try
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ErrorRaisingXMLReader";
                command.Connection = connection;
                command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 7 });
                command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                using (var reader = command.ExecuteXmlReaderWithRetry(policy, policy))
                {
                    while (reader.Read())
                    {
                    }
                }
            }
            catch (Exception)
            {
                Assert.AreEqual(5, count);
                connection.Close();
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
                throw;
            }

            Assert.Fail("Test should throw");
        }

        [TestMethod]
        public void ExecutesXmlReaderWithRetryPolicyAndConnectionRetryPolicyWhenSomeRetriesFailAndThenSucceeds()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            connection.Open();
            SqlCommand command = new SqlCommand();
            int count = 0;
            int rowcount = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ErrorRaisingXMLReader";
            command.Connection = connection;
            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 3 });
            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
            using (var reader = command.ExecuteXmlReaderWithRetry(policy, policy))
            {
                while (reader.Read())
                {
                    rowcount++;
                }
            }

            Assert.AreEqual(2, count);
            connection.Close();
            Assert.AreEqual<ConnectionState>(ConnectionState.Closed, command.Connection.State);
            Assert.AreEqual(10, rowcount);
        }
    }
}
