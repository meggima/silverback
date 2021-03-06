// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using Silverback.Messaging.Messages;

namespace Silverback.Tests.Core.TestTypes.Messages
{
    public class TestEnvelope : ITestRawEnvelope, IEnvelope
    {
        public TestEnvelope(object message, bool autoUnwrap = true)
        {
            Message = message;
            AutoUnwrap = autoUnwrap;
        }

        public bool AutoUnwrap { get; }

        public object? Message { get; set; }
    }
}
