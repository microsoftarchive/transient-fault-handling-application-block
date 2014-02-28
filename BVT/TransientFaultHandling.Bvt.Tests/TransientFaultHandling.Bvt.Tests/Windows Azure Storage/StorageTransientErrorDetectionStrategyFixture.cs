// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Services.Client;
using System.Net;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests.Windows_Azure_Storage
{
    [TestClass]
    public class StorageTransientErrorDetectionStrategyFixture
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
        public void RetriesWhenDataServiceRequestExceptionIsThrownForServerBusy()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<StorageTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    var innerException = new DataServiceClientException(Microsoft.WindowsAzure.StorageClient.StorageErrorCodeStrings.ServerBusy);
                    var ex = new DataServiceRequestException(Microsoft.WindowsAzure.StorageClient.StorageErrorCodeStrings.ServerBusy, innerException);
                    throw ex;
                });

                Assert.Fail("Should have thrown DataServiceRequestException");
            }
            catch (DataServiceRequestException)
            { }
            catch (Exception)
            {
                Assert.Fail("Should have thrown DataServiceRequestException");
            }

            Assert.AreEqual(6, executeCount);
        }

        [TestMethod]
        public void RetriesWhenTimeoutExceptionIsThrown()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<StorageTransientErrorDetectionStrategy>("Retry 5 times");
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
        public void RetriesWhenWebExceptionIsThrownForConnectionClosed()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<StorageTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    throw new WebException("Connection Closed", WebExceptionStatus.ConnectionClosed);
                });

                Assert.Fail("Should have thrown WebException");
            }
            catch (WebException)
            { }
            catch (Exception)
            {
                Assert.Fail("Should have thrown WebException");
            }

            Assert.AreEqual(6, executeCount);
        }

        [TestMethod]
        public void RetriesWhenWebExceptionIsThrownForConnectFailed()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<StorageTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    throw new WebException("Connection Failure", WebExceptionStatus.ConnectFailure);
                });

                Assert.Fail("Should have thrown WebException");
            }
            catch (WebException)
            { }
            catch (Exception)
            {
                Assert.Fail("Should have thrown WebException");
            }

            Assert.AreEqual(6, executeCount);
        }

        [TestMethod]
        public void DoesNotRetryWhenUnauthorizedAccessExceptionIsThrown()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<StorageTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    throw new UnauthorizedAccessException("Access Violation");
                });

                Assert.Fail("Should have thrown UnauthorizedAccessException");
            }
            catch (UnauthorizedAccessException)
            { }
            catch (Exception)
            {
                Assert.Fail("Should have thrown UnauthorizedAccessException");
            }

            Assert.AreEqual(1, executeCount);
        }
    }
}
