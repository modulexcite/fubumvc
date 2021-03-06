﻿using System;
using FubuCore;
using FubuCore.Logging;
using FubuMVC.Core.Runtime.Logging;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using Rhino.Mocks;
using System.Linq;
using System.Collections.Generic;
using ExceptionReport = FubuCore.Logging.ExceptionReport;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class ChainSuccessContinuationTester
    {
        private Envelope theEnvelope;
        private FubuTransportation.Runtime.Invocation.InvocationContext theContext;
        private RecordingEnvelopeSender theSender;
        private ChainSuccessContinuation theContinuation;
        private RecordingLogger theLogger;
        private TestContinuationContext theContinuationContext;

        [SetUp]
        public void SetUp()
        {
            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new object();

            theContinuationContext = new TestContinuationContext();

            theContext = new FubuTransportation.Runtime.Invocation.InvocationContext(theEnvelope, new HandlerChain());

            theContext.EnqueueCascading(new object());
            theContext.EnqueueCascading(new object());
            theContext.EnqueueCascading(new object());

            theSender = new RecordingEnvelopeSender();

            theContinuation = new ChainSuccessContinuation(theContext);

            theLogger = new RecordingLogger();

            theContinuation.Execute(theEnvelope, theContinuationContext);
        }

        [Test]
        public void should_mark_the_message_as_successful()
        {
            theEnvelope.Callback.AssertWasCalled(x => x.MarkSuccessful());
        }

        [Test]
        public void should_log_the_chain_success()
        {
            theContinuationContext.RecordedLogs.InfoMessages.Single()
                .ShouldEqual(new MessageSuccessful { Envelope = theEnvelope.ToToken() });
        }
    }

    [TestFixture]
    public class ChainSuccessContinuation_failure_Tester
    {
        private Envelope theEnvelope;
        private FubuTransportation.Runtime.Invocation.InvocationContext theContext;
        private RecordingEnvelopeSender theSender;
        private ChainSuccessContinuation theContinuation;
        private RecordingLogger theLogger;
        private TestContinuationContext theContinuationContext;
        private Exception theException;

        [SetUp]
        public void SetUp()
        {
            theEnvelope = ObjectMother.Envelope();
            theEnvelope.Message = new object();
            theException = new Exception("Failure");
            theEnvelope.Callback.Stub(x => x.MarkSuccessful()).Throw(theException);

            theContinuationContext = new TestContinuationContext();

            theContext = new FubuTransportation.Runtime.Invocation.InvocationContext(theEnvelope, new HandlerChain());
            theContext.EnqueueCascading(new object());

            theContinuation = new ChainSuccessContinuation(theContext);
            theContinuation.Execute(theEnvelope, theContinuationContext);
        }

        [Test]
        public void should_not_log_success()
        {
            theContinuationContext.RecordedLogs.InfoMessages.ShouldNotContain(
                new MessageSuccessful { Envelope = theEnvelope.ToToken() });
        }

        [Test]
        public void should_move_the_envelope_to_the_error_queue()
        {
            theEnvelope.Callback.AssertWasCalled(x => x.MoveToErrors(new ErrorReport(theEnvelope, theException)));
        }

        [Test]
        public void should_log_the_exception()
        {
            var report = theContinuationContext.RecordedLogs.ErrorMessages.Single()
                .As<ExceptionReport>();

            report.ExceptionText.ShouldEqual(theException.ToString());
        }

        [Test]
        public void should_send_a_failure_ack()
        {
            theContinuationContext.RecordedOutgoing.FailureAcknowledgementMessage
                .ShouldEqual("Sending cascading message failed: Failure");
        }
    }
}