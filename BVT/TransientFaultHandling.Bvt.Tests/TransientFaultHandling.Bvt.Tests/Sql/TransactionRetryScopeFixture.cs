// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests.TestObjects;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests.Sql
{
    [TestClass]
    public class TransactionRetryScopeFixture
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
        public void TransactionIsCommittedWhenSomeRetriesFailAndThenSucceeds()
        {
            this.DeleteAllOnTranscationScopeTestTable();

            int retryTransactionCount = 0;
            var policyForTransaction = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            policyForTransaction.Retrying += (s, args) =>
            {
                retryTransactionCount++;
            };

            int retrySqlCommandCount = 0;
            var policyForSqlCommand = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 2 times, first retry is fast");
            policyForSqlCommand.Retrying += (s, args) =>
            {
                retrySqlCommandCount++;
            };

            int transactionActionExecutedCount = 0;
            Action action = new Action(() =>
            {
                transactionActionExecutedCount++;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = "Insert Into TranscationScopeTestTable (rowId) Values (@rowId);";
                        command.Connection = connection;
                        command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                        command.ExecuteNonQueryWithRetry(policyForSqlCommand);
                    }

                    if (retryTransactionCount < 4)
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = "ErrorRaisingForExecuteNonQuery";
                            command.Connection = connection;
                            command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                            command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 10 });
                            command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                            command.ExecuteNonQueryWithRetry(policyForSqlCommand);
                        }
                    }
                }
            });

            using (TransactionRetryScope scope = new TransactionRetryScope(policyForTransaction, action))
            {
                try
                {
                    scope.InvokeUnitOfWork();
                    scope.Complete();
                }
                catch (Exception)
                {
                    Assert.Fail("Should not throw");
                }
            }

            Assert.AreEqual(1, this.GetCountOnTranscationScopeTestTable());
            Assert.AreEqual(5, transactionActionExecutedCount);
            Assert.AreEqual(4, retryTransactionCount);
            Assert.AreEqual(8, retrySqlCommandCount);
        }

        [TestMethod]
        public void TransactionIsRolledBackAndExceptionIsThrownWhenAllRetriesFail()
        {
            this.DeleteAllOnTranscationScopeTestTable();

            int retryTransactionCount = 0;
            var policyForTransaction = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 5 times");
            policyForTransaction.Retrying += (s, args) =>
            {
                retryTransactionCount++;
            };

            int retrySqlCommandCount = 0;
            var policyForSqlCommand = this.retryManager.GetRetryPolicy<FakeSqlAzureTransientErrorDetectionStrategy>("Retry 2 times, first retry is fast");
            policyForSqlCommand.Retrying += (s, args) =>
            {
                retrySqlCommandCount++;
            };

            int transactionActionExecutedCount = 0;
            Action action = new Action(() =>
            {
                transactionActionExecutedCount++;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = "Insert Into TranscationScopeTestTable (rowId) Values (@rowId);";
                        command.Connection = connection;
                        command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                        command.ExecuteNonQueryWithRetry(policyForSqlCommand);
                    }

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "ErrorRaisingForExecuteNonQuery";
                        command.Connection = connection;
                        command.Parameters.Add(new SqlParameter("rowId", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() });
                        command.Parameters.Add(new SqlParameter("maxCountToRaiseErrors", SqlDbType.Int) { Value = 10 });
                        command.Parameters.Add(new SqlParameter("error", SqlDbType.Int) { Value = 60000 });
                        command.ExecuteNonQueryWithRetry(policyForSqlCommand);
                    }
                }
            });

            using (TransactionRetryScope scope = new TransactionRetryScope(policyForTransaction, action))
            {
                try
                {
                    scope.InvokeUnitOfWork();
                    scope.Complete();

                    Assert.Fail("Should have thrown SqlException");
                }
                catch (SqlException)
                { }
                catch (Exception)
                {
                    Assert.Fail("Should have thrown SqlException");
                }
            }

            Assert.AreEqual(0, this.GetCountOnTranscationScopeTestTable());
            Assert.AreEqual(6, transactionActionExecutedCount);
            Assert.AreEqual(5, retryTransactionCount);
            Assert.AreEqual(12, retrySqlCommandCount);
        }

        private int GetCountOnTranscationScopeTestTable()
        {
            int count = 0;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "Select Count(*) From TranscationScopeTestTable";
                    count = (int)command.ExecuteScalar();
                }

                connection.Close();
            }

            return count;
        }

        private void DeleteAllOnTranscationScopeTestTable()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "Delete From TranscationScopeTestTable";
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
    }
}
