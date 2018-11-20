﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Silverback.Examples.Common;
using Silverback.Examples.Common.Messages;
using Silverback.Messaging;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Publishing;
using Silverback.Messaging.Subscribers;

namespace Silverback.Examples.Main.UseCases.Basic
{
    public class TranslateUseCase : UseCase, ISubscriber
    {
        public TranslateUseCase() : base("Translate outbound message", 20)
        {
        }

        protected override void ConfigureServices(IServiceCollection services) => services
            .AddBus()
            .AddBroker<FileSystemBroker>()
            .AddScoped<ISubscriber, TranslateUseCase>();

        protected override void Configure(IBrokerEndpointsConfigurationBuilder endpoints) => endpoints
            .AddOutbound<IIntegrationEvent>(FileSystemEndpoint.Create("simple-events", Configuration.FileSystemBrokerBasePath));

        protected override async Task Execute(IServiceProvider serviceProvider)
        {
            var publisher = serviceProvider.GetService<IEventPublisher<SimpleEvent>>();

            await publisher.PublishAsync(new SimpleEvent { Content = DateTime.Now.ToString("HH:mm:ss.fff") });
        }

        [Subscribe]
        IMessage OnSimpleEvent(SimpleEvent message) => new SimpleIntegrationEvent {Content = message.Content};
    }
}