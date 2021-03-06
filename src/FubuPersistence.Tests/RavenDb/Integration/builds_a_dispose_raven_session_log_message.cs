﻿using System.Security.Principal;
using System.Threading;
using FubuMVC.Core.Http.Owin;
using FubuPersistence.RavenDb;
using FubuTestingSupport;
using NUnit.Framework;
using Raven.Client;
using Rhino.Mocks;

namespace FubuPersistence.Tests.RavenDb.Integration
{
    public class builds_a_dispose_raven_session_log_message
    {
        private DisposeRavenSessionMessage theMessage;

        [SetUp]
        public void SetUp()
        {
            var advanced = MockRepository.GenerateMock<ISyncAdvancedSessionOperation>();
            advanced.Stub(x => x.NumberOfRequests).Return(10);
            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Stub(x => x.Advanced).Return(advanced);

            session.Advanced.NumberOfRequests.ShouldEqual(10);

            var request = new OwinHttpRequest();
            request.FullUrl("http://something/somethingelse");
            request.HttpMethod("GET");

            var currentPrincipal = new GenericPrincipal(new GenericIdentity("bob"), new string[0]);
            Thread.CurrentPrincipal = currentPrincipal;

            theMessage = DisposeRavenSessionMessage.For(session);
        }

        [Test]
        public void the_username_is_correct()
        {
            theMessage.UserName.ShouldEqual("bob");
        }

        [Test]
        public void the_request_number_is_correct()
        {
            theMessage.Requests.ShouldEqual(10);
        }
    }
}