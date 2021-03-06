﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Silverback.Messaging.Messages;

namespace Silverback.Examples.Consumer.Subscribers
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Subscriber")]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Subscriber")]
    public class BinaryFilesSubscriber
    {
        private readonly ILogger<SampleEventsSubscriber> _logger;

        public BinaryFilesSubscriber(ILogger<SampleEventsSubscriber> logger)
        {
            _logger = logger;
        }

        public void OnBinaryReceived(IBinaryFileMessage message)
        {
            _logger.LogInformation("Received Binary File {@message}", message);
        }
    }
}
