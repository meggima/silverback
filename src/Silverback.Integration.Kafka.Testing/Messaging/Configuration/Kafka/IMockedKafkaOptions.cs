﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;

namespace Silverback.Messaging.Configuration.Kafka
{
    /// <summary>
    ///     Stores the mocked Kafka configuration.
    /// </summary>
    public interface IMockedKafkaOptions
    {
        /// <summary>
        ///     Gets or sets the default number of partitions to be created per each topic. The default is 5.
        /// </summary>
        public int DefaultPartitionsCount { get; set; }

        /// <summary>
        ///     Gets the number of partitions created for the given topic. If not specified for a
        ///     topic the <see cref="DefaultPartitionsCount" /> will be used.
        /// </summary>
        public IDictionary<string, int> TopicPartitionsCount { get; }

        /// <summary>
        ///     Gets or sets the value to be used instead of the default 5 seconds or the
        ///     configured"ConfluentConsumerConfigProxy.AutoCommitIntervalMs" /> for the inbound topics. Set it to
        ///     <c>null</c> to disable the feature. The default is 50 milliseconds.
        /// </summary>
        public int? OverriddenAutoCommitIntervalMs { get; set; }

        /// <summary>
        ///     Gets or sets the delay to be applied before and assigning the partitions.
        /// </summary>
        public TimeSpan PartitionsAssignmentDelay { get; set; }
    }
}
