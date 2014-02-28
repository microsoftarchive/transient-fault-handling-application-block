// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;

namespace TransientFaultHandling.Tests.TestObjects
{
    public class MockErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        public MockErrorDetectionStrategy()
        {
            CallCount = 0;
            ThreadIdList = new List<int>();
        }

        public bool IsTransient(Exception ex)
        {
            ThreadIdList.Add(Thread.CurrentThread.ManagedThreadId);
            ++CallCount;

            if (ex is AggregateException)
            {
                return this.IsTransientNonAggregate(ex.InnerException);
            }

            return this.IsTransientNonAggregate(ex);
        }

        private bool IsTransientNonAggregate(Exception ex)
        {
            if (ex is InvalidCastException)
            {
                return true;
            }

            if (ex is InvalidOperationException)
            {
                return true;
            }

            if (ex is SecurityException)
            {
                return true;
            }

            return false;
        }

        public int CallCount { get; set; }

        public List<int> ThreadIdList { get; set; }
    }
}
