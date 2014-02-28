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
    public class SqlDatabaseTransientErrorDetectionStrategyFixture
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
        public void RetriesWhenSqlExceptionIsThrownWithTransportLevelError()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    var ex = SqlExceptionCreator.CreateSqlException("A transport-level error has occurred when sending the request to the server", 10053);
                    throw ex;
                });

                Assert.Fail("Should have thrown SqlException");
            }
            catch (SqlException)
            { }
            catch (Exception)
            {
                Assert.Fail("Should have thrown SqlException");
            }

            Assert.AreEqual(6, executeCount);
        }

        [TestMethod]
        public void RetriesWhenSqlExceptionIsThrownWithNetworkLevelError()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    var ex = SqlExceptionCreator.CreateSqlException("A network-related or instance-specific error occurred while establishing a connection to SQL Server.", 10054);
                    throw ex;
                });

                Assert.Fail("Should have thrown SqlException");
            }
            catch (SqlException)
            { }
            catch (Exception)
            {
                Assert.Fail("Should have thrown SqlException");
            }

            Assert.AreEqual(6, executeCount);
        }

        [TestMethod]
        public void DoesNotRetryWhenSqlExceptionIsThrownWithSqlQueryError()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    var ex = SqlExceptionCreator.CreateSqlException("ORDER BY items must appear in the select list if the statement contains a UNION, INTERSECT or EXCEPT operator.", 104);
                    throw ex;
                });

                Assert.Fail("Should have thrown SqlException");
            }
            catch (SqlException)
            { }
            catch (Exception)
            {
                Assert.Fail("Should have thrown SqlException");
            }

            Assert.AreEqual(1, executeCount);
        }

        [TestMethod]
        public void RetriesWhenTimeoutExceptionIsThrown()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    throw new TimeoutException();
                });

                Assert.Fail("Should have thrown TimeoutException");
            }
            catch (TimeoutException)
            { }
            catch (Exception)
            {
                Assert.Fail("Should have thrown TimeoutException");
            }

            Assert.AreEqual(6, executeCount);
        }

        [TestMethod]
        public void RetriesWhenEntityExceptionIsThrownWithTimeoutExceptionAsInnerException()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    throw new EntityException("Sample Error", new TimeoutException("Connection Timed out"));
                });

                Assert.Fail("Should have thrown EntityException");
            }
            catch (EntityException ex)
            {
                Assert.IsInstanceOfType(ex.InnerException, typeof(TimeoutException));
            }
            catch (Exception)
            {
                Assert.Fail("Should have thrown EntityException");
            }

            Assert.AreEqual(6, executeCount);
        }
    }
}
