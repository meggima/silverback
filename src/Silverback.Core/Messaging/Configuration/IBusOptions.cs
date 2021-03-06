﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;
using Silverback.Messaging.Subscribers.Subscriptions;

namespace Silverback.Messaging.Configuration
{
    /// <summary>
    ///     Stores the internal bus configuration (subscribers, etc.).
    /// </summary>
    public interface IBusOptions
    {
        /// <summary>
        ///     Gets the collection of <see cref="ISubscription" />. A single subscription can resolve to multiple
        ///     subscribed methods.
        /// </summary>
        IList<ISubscription> Subscriptions { get; }

        /// <summary>
        ///     Gets the collection of handled message types. These types will be recognized as messages and thus
        ///     automatically republished when returned by a subscribed method.
        /// </summary>
        IList<Type> MessageTypes { get; }
    }
}
