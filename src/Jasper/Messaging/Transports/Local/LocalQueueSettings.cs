using System.Collections.Generic;
using Jasper.Configuration;

namespace Jasper.Messaging.Transports.Local
{
    public class LocalQueueSettings : ListenerSettings
    {
        public LocalQueueSettings(string name)
        {
            Name = name.ToLowerInvariant();
        }


        public IList<Subscription> Subscriptions { get; } = new List<Subscription>();
    }
}
