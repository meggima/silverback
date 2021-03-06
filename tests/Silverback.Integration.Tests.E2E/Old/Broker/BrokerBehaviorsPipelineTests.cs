// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Silverback.Messaging;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Encryption;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Publishing;
using Silverback.Messaging.Sequences.Batch;
using Silverback.Messaging.Sequences.Chunking;
using Silverback.Messaging.Serialization;
using Silverback.Tests.Integration.E2E.TestHost;
using Silverback.Tests.Integration.E2E.TestTypes.Messages;
using Silverback.Util;
using Xunit;
using Xunit.Abstractions;

namespace Silverback.Tests.Integration.E2E.Old.Broker
{
    public class BrokerBehaviorsPipelineTests : KafkaTestFixture
    {
        private static readonly byte[] AesEncryptionKey =
        {
            0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e,
            0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c
        };

        public BrokerBehaviorsPipelineTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact(Skip = "Deprecated")]
        public async Task BatchConsuming_CorrectlyConsumedInBatch()
        {
            var message1 = new TestEventOne
            {
                Content = "Hello E2E!"
            };
            var message2 = new TestEventOne
            {
                Content = "Hello E2E!"
            };

            Host.ConfigureServices(
                    services => services
                        .AddLogging()
                        .AddSilverback()
                        .UseModel()
                        .WithConnectionToMessageBroker(options => options.AddMockedKafka())
                        .AddEndpoints(
                            endpoints => endpoints
                                .AddOutbound<IIntegrationEvent>(new KafkaProducerEndpoint(DefaultTopicName))
                                .AddInbound(
                                    new KafkaConsumerEndpoint(DefaultTopicName)
                                    {
                                        Batch = new BatchSettings
                                        {
                                            Size = 2
                                        }
                                    }))
                        .AddIntegrationSpy())
                .Run();

            var publisher = Host.ScopedServiceProvider.GetRequiredService<IEventPublisher>();
            await publisher.PublishAsync(message1);

            await Helper.WaitUntilAllMessagesAreConsumedAsync();

            Helper.Spy.OutboundEnvelopes.Should().HaveCount(1);
            Helper.Spy.InboundEnvelopes.Should().BeEmpty();

            await publisher.PublishAsync(message2);

            await Helper.WaitUntilAllMessagesAreConsumedAsync();

            Helper.Spy.OutboundEnvelopes.Should().HaveCount(2);
            Helper.Spy.InboundEnvelopes.Should().HaveCount(2);
        }

        [Fact(Skip = "Deprecated")]
        public async Task ChunkingWithBatchConsuming_CorrectlyConsumedInBatchAndAggregated()
        {
            var message1 = new TestEventOne
            {
                Content = "Hello E2E!"
            };
            var message2 = new TestEventOne
            {
                Content = "Hello E2E!"
            };

            Host.ConfigureServices(
                    services => services
                        .AddLogging()
                        .AddSilverback()
                        .UseModel()
                        .WithConnectionToMessageBroker(options => options.AddMockedKafka())
                        .AddEndpoints(
                            endpoints => endpoints
                                .AddOutbound<IIntegrationEvent>(
                                    new KafkaProducerEndpoint(DefaultTopicName)
                                    {
                                        Chunk = new ChunkSettings
                                        {
                                            Size = 10
                                        }
                                    })
                                .AddInbound(
                                    new KafkaConsumerEndpoint(DefaultTopicName)
                                    {
                                        Batch = new BatchSettings
                                        {
                                            Size = 2
                                        }
                                    }))
                        .AddIntegrationSpy())
                .Run();

            var publisher = Host.ScopedServiceProvider.GetRequiredService<IEventPublisher>();
            await publisher.PublishAsync(message1);

            await Helper.WaitUntilAllMessagesAreConsumedAsync();

            Helper.Spy.OutboundEnvelopes.Should().HaveCount(3);
            Helper.Spy.InboundEnvelopes.Should().BeEmpty();

            await publisher.PublishAsync(message2);

            await Helper.WaitUntilAllMessagesAreConsumedAsync();

            Helper.Spy.OutboundEnvelopes.Should().HaveCount(6);
            Helper.Spy.OutboundEnvelopes.ForEach(
                envelope =>
                {
                    envelope.RawMessage.Should().NotBeNull();
                    envelope.RawMessage!.Length.Should().BeLessOrEqualTo(10);
                });
            Helper.Spy.InboundEnvelopes.Should().HaveCount(2);
        }

        [Fact(Skip = "Deprecated")]
        public async Task ChunkingWithCustomHeaders_HeadersTransferred()
        {
            var message = new TestEventWithHeaders
            {
                Content = "Hello E2E!",
                CustomHeader = "Hello header!",
                CustomHeader2 = false
            };

            Host.ConfigureServices(
                    services => services
                        .AddLogging()
                        .AddSilverback()
                        .UseModel()
                        .WithConnectionToMessageBroker(options => options.AddMockedKafka())
                        .AddEndpoints(
                            endpoints => endpoints
                                .AddOutbound<IIntegrationEvent>(
                                    new KafkaProducerEndpoint(DefaultTopicName)
                                    {
                                        Chunk = new ChunkSettings
                                        {
                                            Size = 10
                                        }
                                    })
                                .AddInbound(new KafkaConsumerEndpoint(DefaultTopicName)))
                        .AddIntegrationSpy())
                .Run();

            var publisher = Host.ScopedServiceProvider.GetRequiredService<IEventPublisher>();
            await publisher.PublishAsync(message);

            await Helper.WaitUntilAllMessagesAreConsumedAsync();

            Helper.Spy.InboundEnvelopes.Should().HaveCount(1);
            Helper.Spy.InboundEnvelopes[0].Message.Should().BeEquivalentTo(message);
            Helper.Spy.InboundEnvelopes[0].Headers.Should().ContainEquivalentOf(
                new MessageHeader("x-custom-header", "Hello header!"));
            Helper.Spy.InboundEnvelopes[0].Headers.Should().ContainEquivalentOf(
                new MessageHeader("x-custom-header2", "False"));
        }

        [Fact(Skip = "Deprecated")]
        public async Task ChunkingWithRemappedHeaderNames_HeadersRemappedAndMessageReceived()
        {
            var message = new TestEventOne
            {
                Content = "Hello E2E!"
            };

            Host.ConfigureServices(
                    services => services
                        .AddLogging()
                        .AddSilverback()
                        .UseModel()
                        .WithConnectionToMessageBroker(options => options.AddMockedKafka())
                        .AddEndpoints(
                            endpoints => endpoints
                                .AddOutbound<IIntegrationEvent>(
                                    new KafkaProducerEndpoint(DefaultTopicName)
                                    {
                                        Chunk = new ChunkSettings
                                        {
                                            Size = 10
                                        }
                                    })
                                .AddInbound(new KafkaConsumerEndpoint(DefaultTopicName)))
                        .WithCustomHeaderName(DefaultMessageHeaders.ChunkIndex, "x-ch-id")
                        .WithCustomHeaderName(DefaultMessageHeaders.ChunksCount, "x-ch-cnt")
                        .AddIntegrationSpy())
                .Run();

            var publisher = Host.ScopedServiceProvider.GetRequiredService<IEventPublisher>();
            await publisher.PublishAsync(message);

            await Helper.WaitUntilAllMessagesAreConsumedAsync();

            Helper.Spy.OutboundEnvelopes.ForEach(
                envelope =>
                {
                    envelope.Headers.GetValue("x-ch-id").Should().NotBeNullOrEmpty();
                    envelope.Headers.GetValue("x-ch-cnt").Should().NotBeNullOrEmpty();
                    envelope.Headers.GetValue(DefaultMessageHeaders.ChunkIndex).Should().BeNull();
                    envelope.Headers.GetValue(DefaultMessageHeaders.ChunksCount).Should().BeNull();
                });

            Helper.Spy.InboundEnvelopes.Should().HaveCount(1);
            Helper.Spy.InboundEnvelopes[0].Message.Should().BeEquivalentTo(message);
        }

        [Fact(Skip = "Deprecated")]
        public async Task EncryptionAndChunking_EncryptedAndChunkedThenAggregatedAndDecrypted()
        {
            var message = new TestEventOne
            {
                Content = "Hello E2E!"
            };
            var rawMessage = await Endpoint.DefaultSerializer.SerializeAsync(
                                 message,
                                 new MessageHeaderCollection(),
                                 MessageSerializationContext.Empty) ??
                             throw new InvalidOperationException("Serializer returned null");

            Host.ConfigureServices(
                    services => services
                        .AddLogging()
                        .AddSilverback()
                        .UseModel()
                        .WithConnectionToMessageBroker(options => options.AddMockedKafka())
                        .AddEndpoints(
                            endpoints => endpoints
                                .AddOutbound<IIntegrationEvent>(
                                    new KafkaProducerEndpoint(DefaultTopicName)
                                    {
                                        Chunk = new ChunkSettings
                                        {
                                            Size = 10
                                        },
                                        Encryption = new SymmetricEncryptionSettings
                                        {
                                            Key = AesEncryptionKey
                                        }
                                    })
                                .AddInbound(
                                    new KafkaConsumerEndpoint(DefaultTopicName)
                                    {
                                        Encryption = new SymmetricEncryptionSettings
                                        {
                                            Key = AesEncryptionKey
                                        }
                                    }))
                        .AddIntegrationSpy())
                .Run();

            var publisher = Host.ScopedServiceProvider.GetRequiredService<IEventPublisher>();
            await publisher.PublishAsync(message);

            await Helper.WaitUntilAllMessagesAreConsumedAsync();

            Helper.Spy.OutboundEnvelopes.Should().HaveCount(5);
            Helper.Spy.OutboundEnvelopes[0].RawMessage.Should().NotBeEquivalentTo(rawMessage.Read(10));
            Helper.Spy.OutboundEnvelopes.ForEach(
                envelope =>
                {
                    envelope.RawMessage.Should().NotBeNull();
                    envelope.RawMessage!.Length.Should().BeLessOrEqualTo(10);
                });
            Helper.Spy.InboundEnvelopes.Should().HaveCount(1);
            Helper.Spy.InboundEnvelopes[0].Message.Should().BeEquivalentTo(message);
        }
    }
}
