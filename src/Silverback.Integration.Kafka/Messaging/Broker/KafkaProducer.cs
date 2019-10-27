﻿// Copyright (c) 2019 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Silverback.Messaging.Messages;
using Silverback.Util;

namespace Silverback.Messaging.Broker
{
    public class KafkaProducer : Producer<KafkaBroker, KafkaProducerEndpoint>, IDisposable
    {
        private readonly ILogger _logger;
        private Confluent.Kafka.IProducer<byte[], byte[]> _innerProducer;

        private static readonly ConcurrentDictionary<Confluent.Kafka.ProducerConfig, Confluent.Kafka.IProducer<byte[], byte[]>> ProducersCache =
            new ConcurrentDictionary<Confluent.Kafka.ProducerConfig, Confluent.Kafka.IProducer<byte[], byte[]>>(new KafkaClientConfigComparer());

        public KafkaProducer(KafkaBroker broker, KafkaProducerEndpoint endpoint, MessageKeyProvider messageKeyProvider,
            ILogger<KafkaProducer> logger, MessageLogger messageLogger)
            : base(broker, endpoint, messageKeyProvider, logger, messageLogger)
        {
            _logger = logger;

            Endpoint.Validate();
        }

        protected override IOffset Produce(object message, byte[] serializedMessage, IEnumerable<MessageHeader> headers) =>
            AsyncHelper.RunSynchronously(() => ProduceAsync(message, serializedMessage, headers));

        protected override async Task<IOffset> ProduceAsync(object message, byte[] serializedMessage, IEnumerable<MessageHeader> headers)
        {
            try
            {
                var kafkaMessage = new Confluent.Kafka.Message<byte[], byte[]>
                {
                    Key = KeyHelper.GetMessageKey(message),
                    Value = serializedMessage
                };

                if (headers != null && headers.Any())
                {
                    kafkaMessage.Headers = new Confluent.Kafka.Headers();
                    headers.ForEach(h => kafkaMessage.Headers.Add(h.ToConfluentHeader()));
                }

                var deliveryReport = await GetInnerProducer().ProduceAsync(Endpoint.Name, kafkaMessage);

                if (Endpoint.Configuration.ArePersistenceStatusReportsEnabled)
                {
                    CheckPersistenceStatus(deliveryReport);
                }

                return new KafkaOffset(deliveryReport.TopicPartitionOffset);
            }
            catch (Confluent.Kafka.KafkaException ex)
            {
                // Disposing and re-creating the producer will maybe fix the issue
                DisposeInnerProducer();

                throw new ProduceException("Error occurred producing the message. See inner exception for details.", ex);
            }
        }

        private Confluent.Kafka.IProducer<byte[], byte[]> GetInnerProducer() =>
            _innerProducer ??= ProducersCache.GetOrAdd(Endpoint.Configuration.ConfluentConfig, _ => CreateInnerProducer());

        private Confluent.Kafka.IProducer<byte[], byte[]> CreateInnerProducer()
        {
            _logger.LogTrace("Creating Confluent.Kafka.Producer...");

            return new Confluent.Kafka.ProducerBuilder<byte[], byte[]>(Endpoint.Configuration.ConfluentConfig).Build();
        }

        private void CheckPersistenceStatus(Confluent.Kafka.DeliveryResult<byte[], byte[]> deliveryReport)
        {
            switch (deliveryReport.Status)
            {
                case Confluent.Kafka.PersistenceStatus.PossiblyPersisted
                    when Endpoint.Configuration.ThrowIfNotAcknowledged:
                {
                    throw new ProduceException(
                        "The message was transmitted to broker, but no acknowledgement was received.");
                }
                case Confluent.Kafka.PersistenceStatus.PossiblyPersisted:
                {
                    _logger.LogWarning(
                        "The message was transmitted to broker, but no acknowledgement was received.");
                    break;
                }
                case Confluent.Kafka.PersistenceStatus.NotPersisted:
                {
                    throw new ProduceException(
                        "The message was never transmitted to the broker, " +
                        "or failed with an error indicating it was not written " +
                        "to the log.'");
                }
            }
        }

        private void DisposeInnerProducer()
        {
            // Dispose only if still in cache to avoid ObjectDisposedException
            if (!ProducersCache.TryRemove(Endpoint.Configuration.ConfluentConfig, out _))
                return;

            _innerProducer?.Flush(TimeSpan.FromSeconds(10));
            _innerProducer?.Dispose();
            _innerProducer = null;
        }

        public void Dispose()
        {
            DisposeInnerProducer();
        }
    }
}
