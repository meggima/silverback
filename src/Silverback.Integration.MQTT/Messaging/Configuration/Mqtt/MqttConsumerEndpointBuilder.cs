﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using MQTTnet.Protocol;
using Silverback.Util;

namespace Silverback.Messaging.Configuration.Mqtt
{
    /// <inheritdoc cref="IMqttConsumerEndpointBuilder" />
    public class MqttConsumerEndpointBuilder
        : ConsumerEndpointBuilder<MqttConsumerEndpoint, IMqttConsumerEndpointBuilder>, IMqttConsumerEndpointBuilder
    {
        private MqttClientConfig _clientConfig;

        private MqttEventsHandlers _mqttEventsHandlers;

        private string[]? _topicNames;

        private MqttQualityOfServiceLevel? _qualityOfServiceLevel;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MqttConsumerEndpointBuilder" /> class.
        /// </summary>
        /// <param name="clientConfig">
        ///     The <see cref="MqttClientConfig" />.
        /// </param>
        /// <param name="mqttEventsHandlers">
        ///     The <see cref="MqttEventsHandlers" />.
        /// </param>
        /// <param name="endpointsConfigurationBuilder">
        ///     The optional reference to the <see cref="IEndpointsConfigurationBuilder" /> that instantiated the
        ///     builder.
        /// </param>
        public MqttConsumerEndpointBuilder(
            MqttClientConfig clientConfig,
            MqttEventsHandlers mqttEventsHandlers,
            IEndpointsConfigurationBuilder? endpointsConfigurationBuilder = null)
            : base(endpointsConfigurationBuilder)
        {
            _mqttEventsHandlers = mqttEventsHandlers;
            _clientConfig = clientConfig;
        }

        /// <inheritdoc cref="EndpointBuilder{TEndpoint,TBuilder}.This" />
        protected override IMqttConsumerEndpointBuilder This => this;

        /// <inheritdoc cref="IMqttConsumerEndpointBuilder.ConsumeFrom" />
        public IMqttConsumerEndpointBuilder ConsumeFrom(params string[] topics)
        {
            Check.HasNoEmpties(topics, nameof(topics));

            _topicNames = topics;

            return this;
        }

        /// <inheritdoc cref="IMqttConsumerEndpointBuilder.Configure(Action{MqttClientConfig})" />
        public IMqttConsumerEndpointBuilder Configure(Action<MqttClientConfig> configAction)
        {
            Check.NotNull(configAction, nameof(configAction));

            configAction.Invoke(_clientConfig);

            return this;
        }

        /// <inheritdoc cref="IMqttConsumerEndpointBuilder.Configure(Action{IMqttClientConfigBuilder})" />
        public IMqttConsumerEndpointBuilder Configure(Action<IMqttClientConfigBuilder> configBuilderAction)
        {
            Check.NotNull(configBuilderAction, nameof(configBuilderAction));

            var configBuilder = new MqttClientConfigBuilder(
                _clientConfig,
                EndpointsConfigurationBuilder?.ServiceProvider);
            configBuilderAction.Invoke(configBuilder);

            _clientConfig = configBuilder.Build();

            return this;
        }

        /// <inheritdoc cref="IMqttConsumerEndpointBuilder.BindEvents(Action{IMqttEventsHandlersBuilder})"/>
        public IMqttConsumerEndpointBuilder BindEvents(Action<IMqttEventsHandlersBuilder> eventsHandlersBuilderAction)
        {
            Check.NotNull(eventsHandlersBuilderAction, nameof(eventsHandlersBuilderAction));

            var eventsBuilder = new MqttEventsHandlersBuilder(_mqttEventsHandlers);
            eventsHandlersBuilderAction.Invoke(eventsBuilder);

            _mqttEventsHandlers = eventsBuilder.Build();

            return this;
        }

        /// <inheritdoc cref="IMqttConsumerEndpointBuilder.BindEvents(Action{MqttEventsHandlers})"/>
        public IMqttConsumerEndpointBuilder BindEvents(Action<MqttEventsHandlers> eventsHandlersAction)
        {
            Check.NotNull(eventsHandlersAction, nameof(eventsHandlersAction));

            eventsHandlersAction.Invoke(_mqttEventsHandlers);

            return this;
        }

        /// <inheritdoc cref="IMqttConsumerEndpointBuilder.WithQualityOfServiceLevel" />
        public IMqttConsumerEndpointBuilder WithQualityOfServiceLevel(MqttQualityOfServiceLevel qosLevel)
        {
            _qualityOfServiceLevel = qosLevel;
            return this;
        }

        /// <inheritdoc cref="IMqttConsumerEndpointBuilder.WithAtMostOnceQoS" />
        public IMqttConsumerEndpointBuilder WithAtMostOnceQoS()
        {
            _qualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce;
            return this;
        }

        /// <inheritdoc cref="IMqttConsumerEndpointBuilder.WithAtLeastOnceQoS" />
        public IMqttConsumerEndpointBuilder WithAtLeastOnceQoS()
        {
            _qualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce;
            return this;
        }

        /// <inheritdoc cref="IMqttConsumerEndpointBuilder.WithExactlyOnceQoS" />
        public IMqttConsumerEndpointBuilder WithExactlyOnceQoS()
        {
            _qualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce;
            return this;
        }

        /// <inheritdoc cref="EndpointBuilder{TEndpoint,TBuilder}.CreateEndpoint" />
        protected override MqttConsumerEndpoint CreateEndpoint()
        {
            var endpoint = new MqttConsumerEndpoint(_topicNames ?? Array.Empty<string>())
            {
                Configuration = _clientConfig,
                EventsHandlers = _mqttEventsHandlers
            };

            if (_qualityOfServiceLevel != null)
                endpoint.QualityOfServiceLevel = _qualityOfServiceLevel.Value;

            return endpoint;
        }
    }
}
