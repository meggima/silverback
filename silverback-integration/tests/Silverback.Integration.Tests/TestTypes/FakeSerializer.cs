﻿using System;
using System.Collections.Generic;
using System.Text;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Serialization;

namespace Silverback.Tests.TestTypes
{
    public class FakeSerializer : IMessageSerializer
    {
        public byte[] Serialize(IMessage message)
        {
            throw new NotImplementedException();
        }

        public IMessage Deserialize(byte[] message)
        {
            throw new NotImplementedException();
        }
    }
}
