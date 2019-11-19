﻿using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.WorkerQueues
{
    public interface IWorkerQueue : IListeningWorkerQueue
    {
        int QueuedCount { get; }

        Task Enqueue(Envelope envelope);

        Task ScheduleExecution(Envelope envelope);

        void StartListening(IListener listener);
    }


}
