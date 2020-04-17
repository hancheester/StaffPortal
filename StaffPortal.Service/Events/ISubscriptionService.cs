using System.Collections.Generic;

namespace StaffPortal.Service.Events
{
    public interface ISubscriptionService
    {
        IList<IConsumer<T>> GetSubsciptions<T>();
    }
}
