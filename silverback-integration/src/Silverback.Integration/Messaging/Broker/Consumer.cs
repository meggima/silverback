﻿using System;
using Microsoft.Extensions.Logging;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Serialization;

namespace Silverback.Messaging.Broker
{
    public abstract class Consumer : EndpointConnectedObject, IConsumer
    {
        private readonly ILogger<Consumer> _logger;

        public event EventHandler<IEnvelope> Received;

        protected Consumer(IBroker broker, IEndpoint endpoint, ILogger<Consumer> logger)
           : base(broker, endpoint)
        {
            _logger = logger;
        }

        protected void HandleMessage(byte[] buffer)
        {
            if (Received == null)
                throw new InvalidOperationException("A message was received but no handler is attached to the Received event.");

            var envelope = DeserializeMessage(buffer);

            _logger.LogTrace($"Received message '{envelope.Message.Id}' from endpoint '{Endpoint.Name}' (source: '{envelope.Source}').");

            try
            {
                Received(this, envelope);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    $"Error occurred processing the message '{envelope.Message.Id}' from endpoint '{Endpoint.Name}'.");

                throw;
            }
        }

        private IEnvelope DeserializeMessage(byte[] buffer)
        {
            try
            {
                return Endpoint.Serializer.Deserialize(buffer);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred deserializing a message.");
                throw;
            }
        }
    }

    public abstract class Consumer<TBroker, TEndpoint> : Consumer
        where TBroker : class, IBroker
        where TEndpoint : class, IEndpoint
    {
        protected Consumer(IBroker broker, IEndpoint endpoint, ILogger<Consumer> logger) 
            : base(broker, endpoint, logger)
        {
        }

        protected new TBroker Broker => (TBroker)base.Broker;

        protected new TEndpoint Endpoint => (TEndpoint)base.Endpoint;
    }
}