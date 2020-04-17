using System;
using System.Collections.Generic;
using System.Text;

namespace StaffPortal.Service.Events
{
    public class EntityUpdatedEvent<T>
    {
        public EntityUpdatedEvent(T entity)
        {
            Entity = entity;
        }

        public T Entity { get; }
    }
}
