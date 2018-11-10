﻿using System.Collections.Generic;

namespace Silverback.Domain
{
    /// <summary>
    /// Exposes the methods to retrieve the <see cref="IDomainEvent{T}"/> collection related to 
    /// an entity.
    /// See <see cref="Entity"/> for a sample implementation of this interface.
    /// </summary>
    public interface IDomainEntity
    {
        IEnumerable<IDomainEvent<IDomainEntity>> GetDomainEvents();

        void ClearEvents();
    }
}