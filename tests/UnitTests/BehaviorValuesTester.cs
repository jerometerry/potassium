namespace Potassium.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NUnit.Framework;
    using Potassium.Core;
    using Potassium.Extensions;
    using Potassium.Providers;

    [TestFixture]
    public class BehaviorValuesTester : TestBase
    {
        [Test]
        public void TestValues()
        {
            var publisher = new FirableEvent<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var listener = behavior.SubscribeWithInitialFire(results.Add);
            publisher.Fire(2);
            publisher.Fire(7);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(9, 2, 7), results);
        }
    }
}
