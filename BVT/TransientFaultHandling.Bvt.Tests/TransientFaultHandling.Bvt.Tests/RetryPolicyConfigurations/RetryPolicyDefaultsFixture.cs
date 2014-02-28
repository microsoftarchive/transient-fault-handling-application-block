using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests
{
    [TestClass]
    public class RetryPolicyDefaultsFixture
    {
        [TestMethod]
        public void DefaultRetryStrategyValues()
        {
            Assert.AreEqual(10, RetryStrategy.DefaultClientRetryCount);
            Assert.AreEqual(10, RetryStrategy.DefaultClientBackoff.Seconds);
            Assert.AreEqual(30, RetryStrategy.DefaultMaxBackoff.Seconds);
            Assert.AreEqual(1, RetryStrategy.DefaultMinBackoff.Seconds);
            Assert.AreEqual(1, RetryStrategy.DefaultRetryInterval.Seconds);
            Assert.AreEqual(1, RetryStrategy.DefaultRetryIncrement.Seconds);
            Assert.AreEqual(true, RetryStrategy.DefaultFirstFastRetry);
        }

        [TestMethod]
        public void ExecutesActionWithDefaultFixedRetryStrategy()
        {
            int count = 0;
            var retryPolicy = RetryPolicy.DefaultFixed;
            try
            {
                retryPolicy.ExecuteAction(() =>
                {
                    // Do Stuff
                    count++;
                    throw new ApplicationException();
                });
            }
            catch (ApplicationException)
            {
                Assert.AreEqual(11, count);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ExecutesActionWithDefaultIncrementRetryStrategy()
        {
            int count = 0;
            var retryPolicy = RetryPolicy.DefaultProgressive;
            try
            {
                retryPolicy.ExecuteAction(() =>
                {
                    // Do Stuff
                    count++;
                    throw new ApplicationException();
                });
            }
            catch (ApplicationException)
            {
                Assert.AreEqual(11, count);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ExecutesActionWithDefaultExponentialRetryStrategy()
        {
            int count = 0;
            var retryPolicy = RetryPolicy.DefaultExponential;
            try
            {
                retryPolicy.ExecuteAction(() =>
                {
                    // Do Stuff
                    count++;
                    throw new ApplicationException();
                });
            }
            catch (ApplicationException)
            {
                Assert.AreEqual(11, count);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ExecutesActionWithNoRetryStrategy()
        {
            int count = 0;
            var retryPolicy = RetryPolicy.NoRetry;
            try
            {
                retryPolicy.ExecuteAction(() =>
                {
                    // Do Stuff
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
    }
}
