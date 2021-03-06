﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Diagnostics.CodeAnalysis;

namespace Silverback.Messaging
{
    /// <summary>
    ///     Represents a queue to consume from.
    /// </summary>
    public sealed class RabbitQueueConsumerEndpoint : RabbitConsumerEndpoint, IEquatable<RabbitQueueConsumerEndpoint>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RabbitQueueConsumerEndpoint" /> class.
        /// </summary>
        /// <param name="name">
        ///     The name of the queue.
        /// </param>
        public RabbitQueueConsumerEndpoint(string name)
            : base(name)
        {
        }

        /// <inheritdoc cref="ConsumerEndpoint.GetUniqueConsumerGroupName" />
        public override string GetUniqueConsumerGroupName() => Name;

        /// <inheritdoc cref="IEquatable{T}.Equals(T)" />
        public bool Equals(RabbitQueueConsumerEndpoint? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return BaseEquals(other) && Equals(Queue, other.Queue);
        }

        /// <inheritdoc cref="object.Equals(object)" />
        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return Equals((RabbitQueueConsumerEndpoint)obj);
        }

        /// <inheritdoc cref="object.GetHashCode" />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Protected set is not abused")]
        public override int GetHashCode() => Name.GetHashCode(StringComparison.Ordinal);
    }
}
