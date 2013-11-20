// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Configuration
{
    using System;
    using System.Collections.Generic;

    internal static class WellKnownRetryStrategies
    {
        public const string Incremental = "incremental";
        public const string Backoff = "exponentialBackoff";
        public const string FixedInterval = "fixedInterval";

        public static readonly Dictionary<string, Type> AllKnownRetryStrategies = new Dictionary<string, Type>()
            {
                { "incremental", typeof(IncrementalData) },
                { "exponentialBackoff", typeof(ExponentialBackoffData) },
                { "fixedInterval", typeof(FixedIntervalData) }
            };
    }
}
