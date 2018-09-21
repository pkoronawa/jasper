﻿using System;
using System.Linq;
using Baseline;
using Jasper.Conneg;
using Jasper.Util;

namespace Jasper.Messaging.Runtime.Routing
{
    public class MessageRoute
    {
        public MessageRoute(Type messageType, Uri destination, string contentType)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            MessageType = messageType.ToMessageAlias();
            DotNetType = messageType;
            Destination = destination;
            ContentType = contentType;
        }

        public MessageRoute(Type messageType, ModelWriter writer, IChannel channel, string contentType)
            : this(messageType, channel.Uri, contentType)
        {
            Writer = writer[contentType];
            Channel = channel;
        }

        public IMessageSerializer Writer { get; internal set; }

        public string MessageType { get; }

        public Type DotNetType { get; }
        public Uri Destination { get; }
        public string ContentType { get; }

        public IChannel Channel { get; set; }


        public Envelope CloneForSending(Envelope envelope)
        {
            if (envelope.Message == null)
            {
                throw new ArgumentNullException(nameof(envelope.Message), "Envelope.Message cannot be null");
            }

            var sending = envelope.Clone();
            sending.Id = CombGuidIdGeneration.NewGuid();
            sending.OriginalId = envelope.Id;

            sending.ReplyUri = envelope.ReplyUri ?? Channel.LocalReplyUri;

            Channel.ApplyModifications(sending);

            sending.ContentType = envelope.ContentType ?? ContentType;

            sending.Writer = Writer;
            sending.Destination = Destination;
            sending.Route = this;

            return sending;
        }

        public bool MatchesEnvelope(Envelope envelope)
        {
            if (Destination != envelope.Destination) return false;

            if (envelope.ContentType != null) return ContentType == envelope.ContentType;

            return !envelope.AcceptedContentTypes.Any() || envelope.AcceptedContentTypes.Contains(ContentType);
        }

        public override string ToString()
        {
            return $"{nameof(MessageType)}: {MessageType}, {nameof(DotNetType)}: {DotNetType}, {nameof(Destination)}: {Destination}, {nameof(ContentType)}: {ContentType}";
        }
    }
}
