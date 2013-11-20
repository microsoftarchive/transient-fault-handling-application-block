// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.TestSupport
{
    using System;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

    public class AlwaysTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception ex)
        {
            return true;
        }
    }
}
