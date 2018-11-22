﻿using System;
using Silverback.Messaging.Connectors;
using Silverback.Messaging.ErrorHandling;
using Silverback.Messaging.Messages;

namespace Silverback.Messaging.Configuration
{
    /// <summary>
    /// Connects the broker with the inbound and/or outbound endpoints.
    /// </summary>
    public interface IBrokerEndpointsConfigurationBuilder
    {
        IBrokerEndpointsConfigurationBuilder AddOutbound<TMessage>(IEndpoint endpoint) where TMessage : IIntegrationMessage;

        IBrokerEndpointsConfigurationBuilder AddOutbound<TMessage, TConnector>(IEndpoint endpoint) 
            where TMessage : IIntegrationMessage
            where TConnector : IOutboundConnector;

        IBrokerEndpointsConfigurationBuilder AddOutbound<TMessage>(IEndpoint endpoint, Type outboundConnectorType) where TMessage : IIntegrationMessage;

        IBrokerEndpointsConfigurationBuilder AddInbound(IEndpoint endpoint, Func<ErrorPolicyBuilder, IErrorPolicy> errorPolicyFactory = null);

        IBrokerEndpointsConfigurationBuilder AddInbound<TConnector>(IEndpoint endpoint, Func<ErrorPolicyBuilder, IErrorPolicy> errorPolicyFactory = null)
            where TConnector : IInboundConnector;

        IBrokerEndpointsConfigurationBuilder AddInbound(IEndpoint endpoint, Type inboundConnectorType, Func<ErrorPolicyBuilder, IErrorPolicy> errorPolicyFactory = null);

        void Connect();
    }
}