﻿using System;
using NSubstitute;
using NUnit.Framework;
using Silverback.Messaging;
using Silverback.Messaging.Configuration;
using Silverback.Tests.TestTypes.Configuration;
using Silverback.Tests.TestTypes.Domain;
using Silverback.Tests.TestTypes.Handlers;
using Silverback.Tests.TestTypes.Subscribers;

namespace Silverback.Tests.Messaging
{
    [TestFixture]
    public class BusConfigTests
    {
        [SetUp]
        public void Setup()
        {
            TestCommandOneHandler.Counter = 0;
            TestCommandTwoHandler.Counter = 0;
            FakeConfigurator.Executed = false;
        }

        [Test]
        public void SubscribeHandlerMethodTest()
        {
            using (var bus = new Bus())
            {
                int counterOne = 0;
                int counterTwo = 0;

                bus.Config()
                    .Subscribe<TestCommandOne>(m => counterOne++)
                    .Subscribe<TestCommandTwo>(m => counterTwo++);

                bus.Publish(new TestCommandOne());
                bus.Publish(new TestCommandTwo());
                bus.Publish(new TestCommandOne());
                bus.Publish(new TestCommandTwo());
                bus.Publish(new TestCommandTwo());

                Assert.That(counterOne, Is.EqualTo(2));
                Assert.That(counterTwo, Is.EqualTo(3));
            }
        }


        [Test]
        public void SubscribeUntypedHandlerMethodTest()
        {
            using (var bus = new Bus())
            {
                int counter = 0;

                bus.Config().Subscribe(m => counter++);

                bus.Publish(new TestCommandOne());
                bus.Publish(new TestCommandTwo());
                bus.Publish(new TestCommandOne());
                bus.Publish(new TestCommandTwo());
                bus.Publish(new TestCommandTwo());

                Assert.That(counter, Is.EqualTo(5));
            }
        }

        [Test]
        public void SubscribeHandlerTest()
        {
            using (var bus = new Bus())
            {
                bus.Config()
                    .WithFactory(t => (IMessageHandler)Activator.CreateInstance(t))
                    .Subscribe<TestCommandOneHandler>()
                    .Subscribe<TestCommandTwoHandler>();

                bus.Publish(new TestCommandOne());
                bus.Publish(new TestCommandTwo());
                bus.Publish(new TestCommandOne());
                bus.Publish(new TestCommandTwo());
                bus.Publish(new TestCommandTwo());

                Assert.That(TestCommandOneHandler.Counter, Is.EqualTo(2));
                Assert.That(TestCommandTwoHandler.Counter, Is.EqualTo(3));
            }
        }

        [Test]
        public void SubscribeCustomSubscriberTest()
        {
            using (var bus = new Bus())
            {
                bus.Config()
                    .Subscribe(o => new TestCustomSubscriber(o));

                bus.Publish(new TestCommandOne());
                bus.Publish(new TestCommandTwo());
                bus.Publish(new TestCommandOne());
                bus.Publish(new TestCommandTwo());
                bus.Publish(new TestCommandTwo());

                Assert.That(TestCustomSubscriber.Counter, Is.EqualTo(5));
            }
        }

        [Test]
        public void ConfigureUsingTest()
        {
            using (var bus = new Bus())
            {
                bus.Config()
                    .ConfigureUsing<FakeConfigurator>();

                Assert.That(FakeConfigurator.Executed, Is.True);
            }
        }
    }
}
