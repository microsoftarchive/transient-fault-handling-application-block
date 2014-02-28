using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TransientFaultHandling.Tests.TestObjects;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Tests
{
    [TestClass]
    public class ArgumentValidationFixture
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenExecutingANullAction()
        {
            RetryPolicy policy = new RetryPolicy(new MockErrorDetectionStrategy(), 10);
            policy.ExecuteAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExceptionIsThrownWhenExecutingANullTask()
        {
            RetryPolicy policy = new RetryPolicy(new MockErrorDetectionStrategy(), 10);
            policy.ExecuteAsync<int>(null);
        }
    }
}
