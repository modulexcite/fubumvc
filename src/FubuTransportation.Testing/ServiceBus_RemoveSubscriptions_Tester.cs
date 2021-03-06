﻿using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using FubuTransportation.Subscriptions;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class ServiceBus_RemoveSubscriptions_Tester : InteractionContext<ServiceBus>
    {
        private IList<Envelope> TheEnvelopesSent
        {
            get
            {
                return MockFor<IEnvelopeSender>().GetArgumentsForCallsMadeOn(x => x.Send(null))
                    .Select(x => x[0] as Envelope)
                    .ToList();
            }
        }

        [Test]
        public void sends_subscriptions_removed()
        {
            var local = new Uri("memory://node1");
            var subscriptions = new[]
            {
                new Subscription
                {
                    Receiver = local,
                    Source = new Uri("memory://node2"),
                },
                new Subscription
                {
                    Receiver = local,
                    Source = new Uri("memory://node3"),
                }
            };
            MockFor<ISubscriptionRepository>().Expect(x => x.LoadSubscriptions(SubscriptionRole.Subscribes))
                .Return(subscriptions);
            MockFor<ISubscriptionRepository>().Expect(x => x.RemoveLocalSubscriptions())
                .Return(subscriptions);

            ClassUnderTest.RemoveSubscriptionsForThisNodeAsync();

            var envelopes = TheEnvelopesSent;
            envelopes.ShouldHaveCount(2);
            envelopes.Any(x => x.Destination == new Uri("memory://node2")).ShouldBeTrue();
            envelopes.Any(x => x.Destination == new Uri("memory://node3")).ShouldBeTrue();
            envelopes.Each(x => x.Message.As<SubscriptionsRemoved>().Receiver.ShouldEqual(local));
        }
    }
}