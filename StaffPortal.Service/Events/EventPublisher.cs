namespace StaffPortal.Service.Events
{
    public class EventPublisher : IEventPublisher
    {
        private readonly ISubscriptionService _subscriptionService;

        public EventPublisher(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public void Publish<T>(T eventMessage)
        {
            var subscribers = _subscriptionService.GetSubsciptions<T>();
            foreach (var subscriber in subscribers)
            {
                subscriber.HandleEvent(eventMessage);
            }
        }
    }
}
