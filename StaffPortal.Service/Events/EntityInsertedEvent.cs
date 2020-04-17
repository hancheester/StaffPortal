using System;
using System.Collections.Generic;
using System.Text;

namespace StaffPortal.Service.Events
{
    public class EntityInsertedEvent<T>
    {
        public EntityInsertedEvent(T entity)
        {
            Entity = entity;
        }

        public T Entity { get; }
    }
}
