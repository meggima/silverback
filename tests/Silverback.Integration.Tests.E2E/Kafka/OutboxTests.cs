﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Publishing;
using Silverback.Tests.Integration.E2E.TestHost;
using Silverback.Tests.Integration.E2E.TestTypes.Database;
using Silverback.Tests.Integration.E2E.TestTypes.Messages;
using Silverback.Util;
using Xunit;
using Xunit.Abstractions;

namespace Silverback.Tests.Integration.E2E.Kafka
{
    public class OutboxTests : KafkaTestFixture
    {
        public OutboxTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task OutboxProduceStrategy_DefaultSettings_ProducedAndConsumed()
        {
            Host
                .WithTestDbContext()
                .ConfigureServices(
                    services => services
                        .AddLogging()
                        .AddSilverback()
                        .UseModel()
                        .UseDbContext<TestDbContext>()
                        .WithConnectionToMessageBroker(
                            options => options
                                .AddMockedKafka()
                                .AddOutboxDatabaseTable()
                                .AddOutboxWorker(TimeSpan.FromMilliseconds(100)))
                        .AddKafkaEndpoints(
                            endpoints => endpoints
                                .Configure(config => { config.BootstrapServers = "PLAINTEXT://e2e"; })
                                .AddOutbound<IIntegrationEvent>(
                                    endpoint => endpoint
                                        .ProduceTo(DefaultTopicName)
                                        .ProduceToOutbox())
                                .AddInbound(
                                    endpoint => endpoint
                                        .ConsumeFrom(DefaultTopicName)
                                        .Configure(
                                            config =>
                                            {
                                                config.GroupId = "consumer1";
                                            })))
                        .AddIntegrationSpyAndSubscriber())
                .Run();

            var publisher = Host.ScopedServiceProvider.GetRequiredService<IEventPublisher>();
            var dbContext = Host.ScopedServiceProvider.GetRequiredService<TestDbContext>();

            for (int i = 1; i <= 15; i++)
            {
                await publisher.PublishAsync(new TestEventOne { Content = $"{i}" });
            }

            await dbContext.SaveChangesAsync();

            await Helper.WaitUntilAllMessagesAreConsumedAsync();

            Helper.Spy.OutboundEnvelopes.Should().HaveCount(15);
            Helper.Spy.InboundEnvelopes.Should().HaveCount(15);
            Helper.Spy.InboundEnvelopes
                .Select(envelope => ((TestEventOne)envelope.Message!).Content)
                .Should().BeEquivalentTo(Enumerable.Range(1, 15).Select(i => $"{i}"));
        }

        [Fact]
        public async Task OutboxProduceStrategy_TransactionAborted_MessageNotProduced()
        {
            Host
                .WithTestDbContext()
                .ConfigureServices(
                    services => services
                        .AddLogging()
                        .AddSilverback()
                        .UseModel()
                        .UseDbContext<TestDbContext>()
                        .WithConnectionToMessageBroker(
                            options => options
                                .AddMockedKafka()
                                .AddOutboxDatabaseTable()
                                .AddOutboxWorker(TimeSpan.FromMilliseconds(100)))
                        .AddKafkaEndpoints(
                            endpoints => endpoints
                                .Configure(config => { config.BootstrapServers = "PLAINTEXT://e2e"; })
                                .AddOutbound<IIntegrationEvent>(
                                    endpoint => endpoint
                                        .ProduceTo(DefaultTopicName)
                                        .ProduceToOutbox())
                                .AddInbound(
                                    endpoint => endpoint
                                        .ConsumeFrom(DefaultTopicName)
                                        .Configure(
                                            config =>
                                            {
                                                config.GroupId = "consumer1";
                                            })))
                        .AddIntegrationSpy())
                .Run();

            using (var scope = Host.ServiceProvider.CreateScope())
            {
                var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
                await publisher.PublishAsync(new TestEventOne());

                /* Just avoid call to SaveChanges */
            }

            var dbContext = Host.ScopedServiceProvider.GetRequiredService<TestDbContext>();
            await Helper.WaitUntilAllMessagesAreConsumedAsync();

            Helper.Spy.OutboundEnvelopes.Should().HaveCount(1);
            Helper.Spy.InboundEnvelopes.Should().BeEmpty();

            dbContext.Outbox.Should().BeEmpty();
            DefaultTopic.TotalMessagesCount.Should().Be(0);
        }

        [Fact]
        public async Task OutboxProduceStrategy_WithEndpointNameFunction_ProducedToProperEndpoint()
        {
            Host
                .WithTestDbContext()
                .ConfigureServices(
                    services => services
                        .AddLogging()
                        .AddSilverback()
                        .UseModel()
                        .UseDbContext<TestDbContext>()
                        .WithConnectionToMessageBroker(
                            options => options
                                .AddMockedKafka()
                                .AddOutboxDatabaseTable()
                                .AddOutboxWorker(TimeSpan.FromMilliseconds(100)))
                        .AddKafkaEndpoints(
                            endpoints => endpoints
                                .Configure(config => { config.BootstrapServers = "PLAINTEXT://e2e"; })
                                .AddOutbound<IIntegrationEvent>(
                                    endpoint => endpoint
                                        .ProduceTo(
                                            envelope =>
                                            {
                                                var testEventOne = (TestEventOne)envelope.Message!;
                                                switch (testEventOne.Content)
                                                {
                                                    case "1":
                                                        return "topic1";
                                                    case "2":
                                                        return "topic2";
                                                    case "3":
                                                        return "topic3";
                                                    default:
                                                        throw new InvalidOperationException();
                                                }
                                            })
                                        .ProduceToOutbox()))
                        .AddIntegrationSpyAndSubscriber())
                .Run();

            var dbContext = Host.ScopedServiceProvider.GetRequiredService<TestDbContext>();
            var topic1 = Helper.GetTopic("topic1");
            var topic2 = Helper.GetTopic("topic2");
            var topic3 = Helper.GetTopic("topic3");
            var publisher = Host.ScopedServiceProvider.GetRequiredService<IEventPublisher>();

            await publisher.PublishAsync(new TestEventOne { Content = "1" });
            await publisher.PublishAsync(new TestEventOne { Content = "2" });
            await publisher.PublishAsync(new TestEventOne { Content = "3" });

            topic1.TotalMessagesCount.Should().Be(0);
            topic2.TotalMessagesCount.Should().Be(0);
            topic3.TotalMessagesCount.Should().Be(0);

            await dbContext.SaveChangesAsync();

            await AsyncTestingUtil.WaitAsync(
                () =>
                    topic1.TotalMessagesCount >= 1 &&
                    topic2.TotalMessagesCount >= 1 &&
                    topic3.TotalMessagesCount >= 1);

            dbContext.Outbox.ForEach(
                outboxMessage => outboxMessage.ActualEndpointName.Should().NotBeNullOrEmpty());

            topic1.TotalMessagesCount.Should().Be(1);
            topic2.TotalMessagesCount.Should().Be(1);
            topic3.TotalMessagesCount.Should().Be(1);
        }
    }
}
