// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.ApplicationServer.Caching;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests.Caching
{
    [TestClass]
    public class CacheTransientErrorDetectionStrategyFixture
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
        public void DoesNotRetryWhenDataCacheExceptionIsThrownWithoutAnErrorCode()
        {
            int executeCount = 0;
            try
            {
                var retryPolicy = this.retryManager.GetRetryPolicy<CacheTransientErrorDetectionStrategy>("Retry 5 times");
                retryPolicy.ExecuteAction(() =>
                {
                    executeCount++;

                    throw new DataCacheException("Error without any substatus or error code");
                });

                Assert.Fail("Should have thrown DataCacheException");
            }
            catch (DataCacheException)
            { }
            catch (Exception)
            {
                Assert.Fail("Should have thrown DataCacheException");
            }

            Assert.AreEqual(1, executeCount);
        }
    }
}
