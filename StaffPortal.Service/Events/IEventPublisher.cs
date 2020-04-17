﻿namespace StaffPortal.Service.Events
{
    public interface IEventPublisher
    {
        void Publish<T>(T eventMessage);
    }
}
