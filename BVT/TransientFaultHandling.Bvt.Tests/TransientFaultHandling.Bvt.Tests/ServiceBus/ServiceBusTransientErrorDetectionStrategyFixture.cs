// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Net;
using System.ServiceModel;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests.ServiceBus
{
    [TestClass]
    public class ServiceBusTransientErrorDetectionStrategyFixture
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
        public void RetriesWhenExceptionIsThrownWithWebExceptionWithTimeoutAsInnerException()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<ServiceBusTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    throw new EntityException("Connection Error", new WebException("error", WebExceptionStatus.Timeout));
                });

                Assert.Fail("Should have thrown EntityException");
            }
            catch (EntityException ex)
            {
                Assert.IsInstanceOfType(ex.InnerException, typeof(WebException));
            }
            catch (Exception)
            {
                Assert.Fail("Should have thrown EntityException");
            }

            Assert.AreEqual(6, executeCount);
        }

        [TestMethod]
        public void RetriesWhenTimeoutExceptionIsThrown()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<ServiceBusTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    throw new TimeoutException();
                });

                Assert.Fail("Should have thrown TimeoutException");
            }
            catch (TimeoutException)
            {
            }
            catch (Exception)
            {
                Assert.Fail("Should have thrown TimeoutException");
            }

            Assert.AreEqual(6, executeCount);
        }

        [TestMethod]
        public void RetriesWhenServerTooBusyExceptionIsThrown()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<ServiceBusTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    throw new ServerTooBusyException();
                });

                Assert.Fail("Should have thrown ServerTooBusyException");
            }
            catch (ServerTooBusyException)
            {
            }
            catch (Exception)
            {
                Assert.Fail("Should have thrown ServerTooBusyException");
            }

            Assert.AreEqual(6, executeCount);
        }

        [TestMethod]
        public void RetriesWhenCommunicationExceptionIsThrown()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<ServiceBusTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    throw new CommunicationException();
                });

                Assert.Fail("Should have thrown CommunicationException");
            }
            catch (CommunicationException)
            {
            }
            catch (Exception)
            {
                Assert.Fail("Should have thrown CommunicationException");
            }

            Assert.AreEqual(6, executeCount);
        }

        [TestMethod]
        public void DoesNotRetryWhenExceptionIsThrown()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<ServiceBusTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    throw new Exception();
                });

                Assert.Fail("Should have thrown Exception");
            }
            catch (Exception)
            { }

            Assert.AreEqual(1, executeCount);
        }
    }
}
