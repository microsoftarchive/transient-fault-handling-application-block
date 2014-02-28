// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests
{
    [TestClass]
    public class RetryPolicyConfigurationSettingsFixture
    {
        private SystemConfigurationSource configurationSource;

        [TestInitialize]
        public void Initialize()
        {
            this.configurationSource = new SystemConfigurationSource();
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
        public void ReadsFixedIntervalRetryStrategyValuesFromConfiguration()
        {
            var settings = RetryPolicyConfigurationSettings.GetRetryPolicySettings(this.configurationSource);
            FixedIntervalData data = (FixedIntervalData)settings.RetryStrategies.Get("Fixed Interval Non Default");

            Assert.AreEqual("Fixed Interval Non Default", data.Name);
            Assert.AreEqual(new TimeSpan(0, 0, 2), data.RetryInterval);
            Assert.AreEqual(2, data.MaxRetryCount);
            Assert.AreEqual(false, data.FirstFastRetry);
        }

        [TestMethod]
        public void ReadsIncrementalRetryStrategyValuesFromConfiguration()
        {
            var settings = RetryPolicyConfigurationSettings.GetRetryPolicySettings(this.configurationSource);
            IncrementalData data = (IncrementalData)settings.RetryStrategies.Get("Incremental Non Default");

            Assert.AreEqual("Incremental Non Default", data.Name);
            Assert.AreEqual(new TimeSpan(0, 0, 1), data.InitialInterval);
            Assert.AreEqual(new TimeSpan(0, 0, 2), data.RetryIncrement);
            Assert.AreEqual(3, data.MaxRetryCount);
            Assert.AreEqual(false, data.FirstFastRetry);
        }

        [TestMethod]
        public void ReadsExponentialBackoffRetryStrategyValuesFromConfiguration()
        {
            var settings = RetryPolicyConfigurationSettings.GetRetryPolicySettings(this.configurationSource);
            ExponentialBackoffData data = (ExponentialBackoffData)settings.RetryStrategies.Get("Exponential Backoff Non Default");

            Assert.AreEqual("Exponential Backoff Non Default", data.Name);
            Assert.AreEqual(new TimeSpan(0, 0, 1), data.MinBackoff);
            Assert.AreEqual(new TimeSpan(0, 0, 2), data.MaxBackoff);
            Assert.AreEqual(TimeSpan.FromMilliseconds(300), data.DeltaBackoff);
            Assert.AreEqual(4, data.MaxRetryCount);
            Assert.AreEqual(false, data.FirstFastRetry);
        }
    }
}
