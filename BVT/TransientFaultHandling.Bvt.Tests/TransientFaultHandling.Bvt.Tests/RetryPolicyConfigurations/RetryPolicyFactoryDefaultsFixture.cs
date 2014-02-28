using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests.TestObjects;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TransientFaultHandling.Tests.TestObjects;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests
{
    [TestClass]
    public class RetryPolicyFactoryDefaultsFixture
    {
        [TestMethod]
        public void CreatesDefaultRetryPolicyFromConfiguration()
        {
            var retryPolicy = RetryPolicyFactory.GetRetryPolicy<MockErrorDetectionStrategy>();
            var retryStrategy = retryPolicy.RetryStrategy as Incremental;

            Assert.AreEqual("Default Retry Strategy", retryStrategy.Name);
        }

        [TestMethod]
        public void CreatesDefaultSqlConnectionPolicyFromConfiguration()
        {
            var retryPolicy = RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicy();
            var retryStrategy = retryPolicy.RetryStrategy as Incremental;

            Assert.AreEqual("Default SqlConnection Retry Strategy", retryStrategy.Name);
        }

        [TestMethod]
        public void CreatesDefaultSqlCommandPolicyFromConfiguration()
        {
            var retryPolicy = RetryPolicyFactory.GetDefaultSqlCommandRetryPolicy();
            var retryStrategy = retryPolicy.RetryStrategy as Incremental;

            Assert.AreEqual("Default SqlCommand Retry Strategy", retryStrategy.Name);
            Assert.IsInstanceOfType(retryPolicy.ErrorDetectionStrategy, typeof(SqlDatabaseTransientErrorDetectionStrategy));
        }

        [TestMethod]
        public void CreatesDefaultServiceBusPolicyFromConfiguration()
        {
            var retryPolicy = RetryPolicyFactory.GetDefaultAzureServiceBusRetryPolicy();
            var retryStrategy = retryPolicy.RetryStrategy as Incremental;

            Assert.AreEqual("Default Azure ServiceBus Retry Strategy", retryStrategy.Name);
            var busPolicy1 = RetryPolicyFactory.GetRetryPolicy<ServiceBusTransientErrorDetectionStrategy>();
            Assert.IsInstanceOfType(busPolicy1.RetryStrategy, typeof(Incremental));
            Assert.IsInstanceOfType(retryPolicy.ErrorDetectionStrategy, typeof(ServiceBusTransientErrorDetectionStrategy));
            Assert.IsInstanceOfType(busPolicy1.ErrorDetectionStrategy, typeof(ServiceBusTransientErrorDetectionStrategy));
        }

        [TestMethod]
        public void CreatesDefaultAzureCachingPolicyFromConfiguration()
        {
            var retryPolicy = RetryPolicyFactory.GetDefaultAzureCachingRetryPolicy();
            var retryStrategy = retryPolicy.RetryStrategy as Incremental;

            Assert.AreEqual("Default Azure Caching Retry Strategy", retryStrategy.Name);
            Assert.IsInstanceOfType(retryPolicy.ErrorDetectionStrategy, typeof(CacheTransientErrorDetectionStrategy));
            var cachePolicy1 = RetryPolicyFactory.GetRetryPolicy<CacheTransientErrorDetectionStrategy>();
            Assert.IsInstanceOfType(cachePolicy1.ErrorDetectionStrategy, typeof(CacheTransientErrorDetectionStrategy));
            Assert.IsInstanceOfType(cachePolicy1.RetryStrategy, typeof(Incremental));
        }

        [TestMethod]
        public void CreatesDefaultAzureStoragePolicyFromConfiguration()
        {
            var retryPolicy = RetryPolicyFactory.GetDefaultAzureStorageRetryPolicy();
            var retryStrategy = retryPolicy.RetryStrategy as Incremental;

            Assert.AreEqual("Default Azure Storage Retry Strategy", retryStrategy.Name);
            Assert.IsInstanceOfType(retryPolicy.ErrorDetectionStrategy, typeof(StorageTransientErrorDetectionStrategy));
            var storagePolicy1 = RetryPolicyFactory.GetRetryPolicy<StorageTransientErrorDetectionStrategy>();
            Assert.IsInstanceOfType(storagePolicy1.RetryStrategy, typeof(Incremental));
            Assert.IsInstanceOfType(storagePolicy1.ErrorDetectionStrategy, typeof(StorageTransientErrorDetectionStrategy));
        }

        [TestMethod]
        public void PolicyInstancesAreNotSingletons()
        {
            var connPolicy = RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicy();
            var nonDefaultIncRetry = connPolicy.RetryStrategy as Incremental;

            var connPolicy1 = RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicy();
            var nonDefaultIncRetry1 = connPolicy1.RetryStrategy as Incremental;

            Assert.AreNotSame(connPolicy, connPolicy1);
        }

        [TestMethod]
        public void DefaultRetryPolicyIsNoRetry()
        {
            int count = 0;
            var connPolicy = RetryPolicyFactory.GetRetryPolicy<MockErrorDetectionStrategy>();
            try
            {
                connPolicy.ExecuteAction(() =>
                {
                    count++;
                    throw new ApplicationException();
                });
            }
            catch (ApplicationException)
            {
                Assert.AreEqual(1, count);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ExceptionIsThrownWhenRetryStrategyIsNotDefinedInConfiguration()
        {
            RetryPolicyFactory.GetRetryPolicy<MockErrorDetectionStrategy>("someinstancewhichdoesnotexist");
        }
    }
}
