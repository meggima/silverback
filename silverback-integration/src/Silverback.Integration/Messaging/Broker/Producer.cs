﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Serialization;

namespace Silverback.Messaging.Broker
{
    /// <summary>
    /// The default <see cref="IProducer" /> implementation.
    /// </summary>
    /// <seealso cref="Silverback.Messaging.Broker.EndpointConnectedObject" />
    /// <seealso cref="Silverback.Messaging.Broker.IProducer" />
    public abstract class Producer : EndpointConnectedObject, IProducer
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Consumer" /> class.
        /// </summary>
        /// <param name="broker">The broker.</param>
        /// <param name="endpoint">The endpoint.</param>
        protected Producer(IBroker broker, IEndpoint endpoint)
            : base(broker, endpoint)
        {
            _logger = broker.LoggerFactory.CreateLogger<Producer>();
        }

        /// <summary>
        /// Sends the specified message through the message broker.
        /// </summary>
        /// <param name="envelope">The envelope containing the message to be sent.</param>
        public void Produce(IEnvelope envelope)
        {
            Produce(envelope.Message, Serializer.Serialize(envelope));
            _logger.LogTrace($"Produced message '{envelope.Message.Id}' on endpoint '{Endpoint.Name}'.");
        }

        /// <summary>
        /// Sends the specified message through the message broker.
        /// </summary>
        /// <param name="envelope">The envelope containing the message to be sent.</param>
        /// <returns></returns>
        public async Task ProduceAsync(IEnvelope envelope)
        {
            await ProduceAsync(envelope.Message, Serializer.Serialize(envelope));
            _logger.LogTrace($"Produced message '{envelope.Message.Id}' on endpoint '{Endpoint.Name}'.");
        }

        /// <summary>
        /// Sends the specified message through the message broker.
        /// </summary>
        /// <param name="message">The original message.</param>
        /// <param name="serializedMessage">The serialized <see cref="IEnvelope"/> including the <see cref="IIntegrationMessage"/>.
        /// This is what is supposed to be sent through the broker.</param>
        protected abstract void Produce(IIntegrationMessage message, byte[] serializedMessage);

        /// <summary>
        /// Sends the specified message through the message broker.
        /// </summary>
        /// <param name="message">The original message.</param>
        /// <param name="serializedMessage">The serialized <see cref="IEnvelope"/> including the <see cref="IIntegrationMessage"/>.
        /// This is what is supposed to be sent through the broker.</param>
        protected abstract Task ProduceAsync(IIntegrationMessage message, byte[] serializedMessage);
    }
}