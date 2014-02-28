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
    public class ReliableSqlConnectionFixture
    {
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
        public void OpensConnectionWithRetryPolicy()
        {
            RetryManager.SetDefault(this.retryPolicySettings.BuildRetryManager(), false);
            var connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString;
            var reliableConnection = new ReliableSqlConnection(connectionString);

            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>("Retry 5 times");

                retryPolicy.ExecuteAction(() =>
                {
                    reliableConnection.Open(retryPolicy);
                });
            }
            catch (SqlException)
            { }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ExecutesNonQuerySqlCommandWithConnectionRetryPolicyAndSqlCommandRetryPolicy()
        {
            RetryManager.SetDefault(this.retryPolicySettings.BuildRetryManager(), false);
            var connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString;
            var reliableConnection = new ReliableSqlConnection(connectionString);

            int count = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>("Retry 5 times");

                retryPolicy.ExecuteAction(() =>
                {
                    SqlCommand command = new SqlCommand("SELECT 1", reliableConnection.Current);
                    count = (int)command.ExecuteNonQueryWithRetry(retryPolicy, retryPolicy);
                });

                Assert.AreEqual(-1, count);
            }
            catch (SqlException)
            { }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ExecutesReaderWithRetryPolicy()
        {
            RetryManager.SetDefault(this.retryPolicySettings.BuildRetryManager(), false);
            var connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString;
            var reliableConnection = new ReliableSqlConnection(connectionString);

            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>("Retry 5 times");

                retryPolicy.ExecuteAction(() =>
                {
                    SqlCommand command = new SqlCommand("SELECT 1", reliableConnection.Current);
                    command.ExecuteReaderWithRetry(retryPolicy);
                });
            }
            catch (SqlException)
            { }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ExecutesXmlReaderWithRetryPolicy()
        {
            RetryManager.SetDefault(this.retryPolicySettings.BuildRetryManager(), false);
            var connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString;
            var reliableConnection = new ReliableSqlConnection(connectionString);

            XmlReader reader;
            int count = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>("Retry 5 times");

                retryPolicy.ExecuteAction(() =>
                {
                    SqlCommand command = new SqlCommand("SELECT 1 FOR XML AUTO", reliableConnection.Current);
                    reader = command.ExecuteXmlReaderWithRetry(retryPolicy);

                    while (reader.Read())
                    {
                        reader.MoveToFirstAttribute();
                        reader.ReadAttributeValue();
                        count = reader.ReadContentAsInt();
                    }
                });

                Assert.AreEqual(1, count);
            }
            catch (SqlException)
            { }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ExecutesSqlCommand()
        {
            RetryManager.SetDefault(this.retryPolicySettings.BuildRetryManager(), false);
            var connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString;
            var reliableConnection = new ReliableSqlConnection(connectionString);

            int count = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>("Retry 5 times");

                retryPolicy.ExecuteAction(() =>
                {
                    SqlCommand command = new SqlCommand("SELECT 1");
                    count = reliableConnection.ExecuteCommand(command);
                });

                Assert.AreEqual(-1, count);
            }
            catch (SqlException)
            { }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void RetriesToExecuteActionWhenSqlExceptionDuringCommandExecution()
        {
            RetryManager.SetDefault(this.retryPolicySettings.BuildRetryManager(), false);
            var connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString;
            var reliableConnection = new ReliableSqlConnection(connectionString);

            int count = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>("Retry 5 times");

                retryPolicy.ExecuteAction(() =>
                {
                    SqlCommand command = new SqlCommand("FAIL");
                    count = reliableConnection.ExecuteCommand(command);
                });

                Assert.AreEqual(-1, count);
            }
            catch (SqlException)
            {
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, reliableConnection.Current.State);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ExecutesCommandWithRetryPolicyWhenSomeRetriesFailAndThenSucceeds()
        {
            RetryManager.SetDefault(this.retryPolicySettings.BuildRetryManager(), false);
            var connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString;
            var reliableConnection = new ReliableSqlConnection(connectionString);

            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            int count = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ErrorRaisingReader";
            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 4 });
            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
            reliableConnection.Open();
            var rowCount = reliableConnection.ExecuteCommand(command, policy);
            reliableConnection.Close();

            Assert.AreEqual<int>(3, count);
            Assert.AreEqual(1, rowCount);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ThrowsExceptionWhenAllRetriesFailAndCommandExecutedWithRetryPolicy()
        {
            RetryManager.SetDefault(this.retryPolicySettings.BuildRetryManager(), false);
            var connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString;
            var reliableConnection = new ReliableSqlConnection(connectionString);

            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            int count = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            int rowCount = 0;
            try
            {
                SqlCommand command = new SqlCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ErrorRaisingReader";
                command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 7 });
                command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                reliableConnection.Open();
                rowCount = reliableConnection.ExecuteCommand(command, policy);
            }
            catch (Exception)
            {
                reliableConnection.Close();
                Assert.AreEqual<int>(5, count);
                Assert.AreEqual(0, rowCount);
                throw;
            }

            Assert.Fail("test should throw");
        }

        [TestMethod]
        public void ExecutesCommandWithoutRetryPolicyWhenSomeRetriesFailAndThenSucceeds()
        {
            RetryManager.SetDefault(this.retryPolicySettings.BuildRetryManager(), false);
            var connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString;
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            var reliableConnection = new ReliableSqlConnection(connectionString, policy, policy);

            int count = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "ErrorRaisingReader";
            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 4 });
            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
            reliableConnection.Open();
            var rowCount = reliableConnection.ExecuteCommand(command);
            reliableConnection.Close();

            Assert.AreEqual<int>(3, count);
            Assert.AreEqual(1, rowCount);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ThrowsExceptionWhenAllRetriesFailAndCommandExecutedWithoutRetryPolicy()
        {
            RetryManager.SetDefault(this.retryPolicySettings.BuildRetryManager(), false);
            var connectionString = ConfigurationManager.ConnectionStrings["TestDatabase"].ConnectionString;
            var policy = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            var reliableConnection = new ReliableSqlConnection(connectionString, policy, policy);

            int count = 0;
            policy.Retrying += (s, args) =>
            {
                count = args.CurrentRetryCount;
            };

            int rowCount = 0;
            try
            {
                SqlCommand command = new SqlCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ErrorRaisingReader";
                command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 7 });
                command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                reliableConnection.Open();
                rowCount = reliableConnection.ExecuteCommand(command);
            }
            catch (Exception)
            {
                reliableConnection.Close();
                Assert.AreEqual<int>(5, count);
                Assert.AreEqual(0, rowCount);
                throw;
            }

            Assert.Fail("test should throw");
        }
    }
}
