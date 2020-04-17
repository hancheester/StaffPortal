using System;
using System.Collections.Generic;
using System.Text;

namespace StaffPortal.Service.Events
{
    public class EntityDeletedEvent<T>
    {
        public EntityDeletedEvent(T entity)
        {
            Entity = entity;
        }

        public T Entity { get; }
    }
}
