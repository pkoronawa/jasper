using System;
using System.Linq;
using Jasper.Testing.Messaging.Transports.Stub;
using Jasper.Util;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing
{
    public class configuring_listeners_by_registry : IDisposable
    {
        private IHost _host;
        private JasperOptions theOptions;

        public configuring_listeners_by_registry()
        {
            _host = Host.CreateDefaultBuilder().UseJasper(x =>
            {
                x.Transports.ListenForMessagesFrom("local://one").Sequential();
                x.Transports.ListenForMessagesFrom("local://two").MaximumThreads(11);
                x.Transports.ListenForMessagesFrom("local://three").Durably();
                x.Transports.ListenForMessagesFrom("local://four").Durably().Lightweight();

            }).Build();

            theOptions = _host.Get<JasperOptions>();
        }

        public void Dispose()
        {
            _host.Dispose();
        }

        [Fact]
        public void configure_sequential()
        {
            theOptions.Transports.ListenForMessagesFrom("local://one")
                .As<ListenerSettings>()
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(1);
        }

        [Fact]
        public void configure_max_parallelization()
        {
            theOptions.Transports.ListenForMessagesFrom("local://two")
                .As<ListenerSettings>()
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(11);
        }

        [Fact]
        public void configure_durable()
        {
            theOptions.Transports.ListenForMessagesFrom("local://three")
                .As<ListenerSettings>()
                .IsDurable
                .ShouldBeTrue();
        }

        [Fact]
        public void configure_not_durable()
        {
            theOptions.Transports.ListenForMessagesFrom("local://four")
                .As<ListenerSettings>()
                .IsDurable
                .ShouldBeFalse();
        }
    }

    public class configuring_listeners_by_option
    {

        [Fact]
        public void set_max_parallelization()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            var listener = options.Transports.ListenForMessagesFrom(uri);

            listener.MaximumThreads(11).ShouldBeSameAs(listener);

            listener.As<ListenerSettings>()
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(11);

        }

        [Fact]
        public void set_sequential()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            var listener = options.Transports.ListenForMessagesFrom(uri);

            listener.Sequential().ShouldBeSameAs(listener);

            listener.As<ListenerSettings>()
                .ExecutionOptions
                .MaxDegreeOfParallelism
                .ShouldBe(1);

        }

        [Fact]
        public void is_durable()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            var listener = options.Transports.ListenForMessagesFrom(uri);

            listener.Durably().ShouldBeSameAs(listener);

            listener.As<ListenerSettings>()
                .IsDurable
                .ShouldBeTrue();

        }

        [Fact]
        public void is_not_durable()
        {
            var options = new JasperOptions();

            var uri = "local://one".ToUri();

            var listener = options.Transports.ListenForMessagesFrom(uri);

            listener.Durably().Lightweight().ShouldBeSameAs(listener);

            listener.As<ListenerSettings>()
                .IsDurable
                .ShouldBeFalse();

        }



    }
}
