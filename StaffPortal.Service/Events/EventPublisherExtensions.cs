namespace StaffPortal.Service.Events
{
    public static class EventPublisherExtensions
    {
        public static void EntityInserted<T>(this IEventPublisher eventPublisher, T entity)
        {
            eventPublisher.Publish(new EntityInsertedEvent<T>(entity));
        }

        public static void EntityUpdated<T>(this IEventPublisher eventPublisher, T entity)
        {
            eventPublisher.Publish(new EntityUpdatedEvent<T>(entity));
        }

        public static void EntityDeleted<T>(this IEventPublisher eventPublisher, T entity)
        {
            eventPublisher.Publish(new EntityDeletedEvent<T>(entity));
        }
    }
}
