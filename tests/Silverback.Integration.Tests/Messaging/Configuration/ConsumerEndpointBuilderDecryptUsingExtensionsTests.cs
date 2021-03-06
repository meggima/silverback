﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using FluentAssertions;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Encryption;
using Silverback.Tests.Types;
using Xunit;

namespace Silverback.Tests.Integration.Messaging.Configuration
{
    public class ConsumerEndpointBuilderDecryptUsingExtensionsTests
    {
        [Fact]
        public void DecryptUsingAes_WithKeyAndIV_EncryptionSettingsSet()
        {
            var builder = new TestConsumerEndpointBuilder();

            var key = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            var iv = new byte[] { 0x11, 0x12, 0x13, 0x14, 0x15 };

            var endpoint = builder.DecryptUsingAes(key, iv).Build();

            endpoint.Encryption.Should().BeOfType<SymmetricEncryptionSettings>();
            endpoint.Encryption.As<SymmetricEncryptionSettings>().Key.Should().BeSameAs(key);
            endpoint.Encryption.As<SymmetricEncryptionSettings>().InitializationVector.Should().BeSameAs(iv);
        }
    }
}
