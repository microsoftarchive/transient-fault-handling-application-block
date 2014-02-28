using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TransientFaultHandling.Tests.TestObjects;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests
{
    [TestClass]
    public class RetryPolicyProgrammaticallyFixture
    {
        [TestMethod]
        public void CreateFixedIntervalRetryStrategyWithCountAndInterval()
        {
            try
            {
                var retryPolicy = new RetryPolicy<MockErrorDetectionStrategy>(new FixedInterval(3, TimeSpan.FromSeconds(1)));
                retryPolicy.ExecuteAction(() =>
                {
                    // Do Stuff
                    throw new InvalidCastException();
                });
            }
            catch (InvalidCastException)
            { }
            catch (Exception)
            {
                Assert.Fail();
            }
        }
    }
}
