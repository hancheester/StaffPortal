using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StaffPortal.Service.Events
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IServiceProvider _serviceProvider;

        public SubscriptionService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IList<IConsumer<T>> GetSubsciptions<T>()
        {
            var consumers = _serviceProvider.GetServices<IConsumer<T>>().ToList();
            return consumers;
        }
    }
}
