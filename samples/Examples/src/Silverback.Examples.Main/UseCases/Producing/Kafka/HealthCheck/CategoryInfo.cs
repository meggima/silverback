﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;
using Silverback.Examples.Main.Menu;

namespace Silverback.Examples.Main.UseCases.Producing.Kafka.HealthCheck
{
    public class CategoryInfo : ICategory
    {
        public string Title => "Health checks";

        public string Description => "Silverback provides some health check capabilities to monitor the connection " +
                                     "with the message broker and more.";

        public IEnumerable<Type> Children => new List<Type>
        {
            typeof(OutboundEndpointsHealthUseCase),
            typeof(OutboundQueueHealthUseCase)
        };
    }
}
