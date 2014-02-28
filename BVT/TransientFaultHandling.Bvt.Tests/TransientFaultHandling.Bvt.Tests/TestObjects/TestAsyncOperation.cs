using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Bvt.Tests.TestObjects
{
    public class TestAsyncOperation
    {
        public bool ThrowFatalExceptionAtBegin = false;
        public int BeginMethodCount { get; private set; }
        public int EndMethodCount { get; private set; }
        public Exception ExceptionToThrowAtEnd { get; set; }
        public Exception ExceptionToThrowAtBegin { get; set; }
        public Exception FatalException { get; set; }
        public int CountToThrowAtBegin { get; set; }
        public int CountToThrowAtEnd { get; set; }

        public bool ThrowException { get; set; }

        public TestAsyncOperation(bool throwException = false)
        {
            ThrowException = throwException;
        }

        public IAsyncResult BeginMethod(System.AsyncCallback callback, object state)
        {
            this.BeginMethodCount++;
            if (BeginMethodCount < CountToThrowAtBegin)
            {
                if (ExceptionToThrowAtBegin != null)
                {
                    throw ExceptionToThrowAtBegin;
                }
            }

            if (ThrowFatalExceptionAtBegin == true)
            {
                if (FatalException != null)
                {
                    Console.WriteLine("Throwing exception of type {0}", FatalException.GetType().ToString());
                    throw FatalException;
                }
            }

            var noOperationAction = new Action(() => { });
            var asyncResult = noOperationAction.BeginInvoke(callback, state);
            return asyncResult;
        }

        public void EndMethod(IAsyncResult asyncResult)
        {
            this.EndMethodCount++;
            if (this.EndMethodCount < this.CountToThrowAtEnd)
            {
                if (ExceptionToThrowAtEnd != null)
                {
                    throw ExceptionToThrowAtEnd;
                }
            }

            if (FatalException != null)
            {
                throw FatalException;
            }
        }   
    }
}
