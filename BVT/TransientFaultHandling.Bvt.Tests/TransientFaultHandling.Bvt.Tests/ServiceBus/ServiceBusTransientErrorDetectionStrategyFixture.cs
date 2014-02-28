// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Net;

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
    }
}
