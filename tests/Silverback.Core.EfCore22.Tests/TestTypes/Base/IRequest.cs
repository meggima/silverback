// Copyright (c) 2019 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using Silverback.Messaging.Messages;

namespace Silverback.Tests.Core.EFCore22.TestTypes.Base
{
    public interface IRequest<out TResponse> : IMessage
    {
    }
}