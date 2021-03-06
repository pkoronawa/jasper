using System;
using System.Threading.Tasks;
using Jasper.ErrorHandling;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Testing.Messaging;
using Jasper.Testing.Runtime;
using Jasper.Transports;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.ErrorHandling
{
    public class RequeueContinuationTester
    {
        [Fact]
        public async Task executing_just_puts_it_back_in_line_at_the_back_of_the_queue()
        {
            var callback = Substitute.For<IChannelCallback>();

            var envelope = ObjectMother.Envelope();



            await RequeueContinuation.Instance.Execute(new MockMessagingRoot(), callback, envelope, null, DateTime.Now);

            await callback.Received(1).Defer(envelope);
        }
    }
}
