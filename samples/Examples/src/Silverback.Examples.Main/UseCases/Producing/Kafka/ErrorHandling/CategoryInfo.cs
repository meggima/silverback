﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;
using Silverback.Examples.Main.Menu;

namespace Silverback.Examples.Main.UseCases.Producing.Kafka.ErrorHandling
{
    public class CategoryInfo : ICategory
    {
        public string Title => "Error handling";

        public string Description => "The error policies are quite a powerful tool to make your consumer resilient " +
                                     "and fault tolerant. They provide an easy way to instruct Silverback how to " +
                                     "behave in case an exception is thrown by the method processing the incoming " +
                                     "message.";

        public IEnumerable<Type> Children => new List<Type>
        {
            typeof(RetryErrorPolicyUseCase),
            typeof(RetryAndMoveErrorPolicyUseCase),
            typeof(RetryAndSkipErrorPolicyUseCase),
            typeof(RetryAndSkipErrorPolicy2UseCase),
            typeof(UnhandledErrorUseCase)
        };
    }
}
