// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

namespace Silverback.Tests.Integration.TestTypes.Domain
{
    public class TestEventTwo : IIntegrationEvent
    {
        public string? Content { get; set; }
    }
}
