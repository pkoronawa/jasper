﻿using IntegrationTests;
using Jasper.Messaging;
using Jasper.Persistence.Marten;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Persistence.Testing.Marten
{
    public class channel_is_durable : PostgresqlContext
    {
        [Fact]
        public void channels_that_are_or_are_not_durable()
        {


            using (var runtime = JasperHost.For(_ =>
            {
                _.MartenConnectionStringIs(Servers.PostgresConnectionString);
                _.Extensions.Include<MartenBackedPersistence>();
            }))
            {
                var root = runtime.Get<IMessagingRoot>();
                root.Runtime.GetOrBuildSendingAgent("local://one".ToUri()).IsDurable.ShouldBeFalse();
                root.Runtime.GetOrBuildSendingAgent("local://durable/two".ToUri()).IsDurable.ShouldBeTrue();

                root.Runtime.GetOrBuildSendingAgent("tcp://server1:2000".ToUri()).IsDurable.ShouldBeFalse();
                root.Runtime.GetOrBuildSendingAgent("tcp://server2:3000/durable".ToUri()).IsDurable.ShouldBeTrue();
            }
        }
    }
}
