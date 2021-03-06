﻿using FubuMVC.Core.Http;
using FubuMVC.Core.Http.Cookies;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuMVC.Tests.Http.Cookies
{
    [TestFixture]
    public class CookieValueSourceTester : InteractionContext<CookieValueSource>
    {
        private ICookies inner;

        protected override void beforeEach()
        {
            inner = MockFor<ICookies>();
        }

        [Test]
        public void provenance_has_to_be_cookies()
        {
            ClassUnderTest.Provenance.ShouldEqual(RequestDataSource.Cookie.ToString());
        }

        [Test]
        public void has_delegates()
        {
            ClassUnderTest.Has("a").ShouldBeFalse();


            inner.Stub(x => x.Has("b")).Return(true);

            ClassUnderTest.Has("b").ShouldBeFalse();
        }

        [Test]
        public void get_with_single_value()
        {
            var cookie = new Cookie("a", "1");
            cookie.Value.ShouldEqual("1");

            inner.Stub(x => x.Get("a")).Return(cookie);

            ClassUnderTest.Get("a").ShouldEqual("1");
        }

        [Test]
        public void value_with_miss()
        {
            ClassUnderTest.Value("a", v => {
                Assert.Fail("should not be called");
            }).ShouldBeFalse();
        }

        [Test]
        public void value_with_single_value_hit()
        {
            var cookie = new Cookie("a", "1");
            inner.Stub(x => x.Get("a")).Return(cookie);

            bool wasCalled = false;
            ClassUnderTest.Value("a", v => {
                wasCalled = true;

                v.RawKey.ShouldEqual("a");
                v.RawValue.ShouldEqual("1");
                v.Source.ShouldEqual(ClassUnderTest.Provenance);
            }).ShouldBeTrue();

            wasCalled.ShouldBeTrue();
        }

        [Test]
        public void value_that_hits_but_not_deterministic()
        {
            var cookie = new Cookie("a");
            cookie.Value.ShouldBeNull();
            inner.Stub(x => x.Get("a")).Return(cookie);

            ClassUnderTest.Value("a", v =>
            {
                Assert.Fail("should not be called");
            }).ShouldBeFalse();
        }


    }
}