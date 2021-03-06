﻿using System;
using FubuCore;
using FubuCore.Logging;
using FubuTransportation.Diagnostics;
using FubuTransportation.Runtime;

namespace FubuTransportation.Logging
{
    public class ChainExecutionFinished : MessageLogRecord
    {
        // TODO -- add a decent ToString()

        public Guid ChainId { get; set; }
        public EnvelopeToken Envelope { get; set; }
        public long ElapsedMilliseconds { get; set; }

        public override string ToString()
        {
            return "Chain finished for {0} at {1}".ToFormat(Envelope.Message, Envelope.ReceivedAt);
        }

        public override MessageRecord ToRecord()
        {
            return new MessageRecord(Envelope)
            {
                Message = "Finished Chain Execution"
            };
        }
    }
}