﻿using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Silverback.Messaging.Configuration;
using Silverback.Tests.TestTypes.Domain;
using Silverback.Tests.TestTypes.Subscribers;

namespace Silverback.Tests.Messaging.Subscribers
{
    [TestFixture]
    public class AsyncSubscriberTests
    {
        private TestCommandOneAsyncSubscriber _subscriber;

        [SetUp]
        public void Setup()
        {
            _subscriber = new TestCommandOneAsyncSubscriber();
            _subscriber.Init(new BusBuilder().Build());
        }

        [Test]
        public async Task BasicTest()
        {
            await _subscriber.OnNextAsync(new TestCommandOne());
            _subscriber.OnNext(new TestCommandOne());

            Assert.That(_subscriber.Handled, Is.EqualTo(2));
        }

        [Test]
        public async Task TypeFilteringTest()
        {
            await _subscriber.OnNextAsync(new TestCommandOne());
            await _subscriber.OnNextAsync(new TestCommandTwo());
            await _subscriber.OnNextAsync(new TestCommandOne());

            Assert.That(_subscriber.Handled, Is.EqualTo(2));
        }

        [Test]
        public async Task CustomFilteringTest()
        {
            _subscriber.Filter = m => m.Message == "yes";

            await _subscriber.OnNextAsync(new TestCommandOne { Message = "no" });
            await _subscriber.OnNextAsync(new TestCommandOne { Message = "yes" });
            await _subscriber.OnNextAsync(new TestCommandOne { Message = "yes" });

            Assert.That(_subscriber.Handled, Is.EqualTo(2));
        }
    }
}