namespace Potassium.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NUnit.Framework;
    using Potassium.Core;
    using Potassium.Providers;

    [TestFixture]
    public class BehaviorTester : TestBase
    {
        [Test]
        public void TestCombine()
        {
            var e1 = new FirableEvent<int>();
            var b1 = e1.Hold(100);

            var e2 = new FirableEvent<int>();
            var b2 = e2.Hold(200);

            var b = b1.Combine(b2, (x, y) => x + y);

            var results = new List<int>();
            b.SubscribeWithInitialFire(results.Add);
            e1.Fire(101);
            e2.Fire(250);

            AssertArraysEqual(Arrays<int>.AsList(300, 301, 351), results);
        }

        [Test]
        public void TestHold()
        {
            var evt = new FirableEvent<int>();
            var behavior = evt.Hold(0);
            var results = new List<int>();
            var listener = behavior.Subscribe(results.Add);
            evt.Fire(2);
            evt.Fire(9);
            listener.Dispose();
            evt.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(2, 9), results);
        }

        [Test]
        public void TestSnapshot()
        {
            var publisher = new FirableEvent<int>();
            var behavior = new Behavior<int>(0, publisher);
            var evt = new FirableEvent<long>();
            var results = new List<string>();
            Func<long, int, string> snapshotFunction = (x, y) => string.Format("{0} {1}", x, y);
            var listener = evt.Snapshot(snapshotFunction, behavior).Subscribe(results.Add);

            evt.Fire(100L);
            publisher.Fire(2);
            evt.Fire(200L);
            publisher.Fire(9);
            publisher.Fire(1);
            evt.Fire(300L);
            listener.Dispose();
            behavior.Dispose();
            evt.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("100 0", "200 2", "300 1"), results);
        }

        [Test]
        public void TestConstantBehavior()
        {
            var behavior = new Identity<int>(12);
            Assert.AreEqual(12, behavior.Value);
            behavior.Dispose();
        }

        [Test]
        public void TestMapB()
        {
            var publisher = new FirableEvent<int>();
            var behavior = new Behavior<int>(6, publisher);
            var results = new List<string>();
            var map = behavior.Map(x => x.ToString(CultureInfo.InvariantCulture));
            var listener = map.SubscribeWithInitialFire(results.Add);
            publisher.Fire(8);
            listener.Dispose();
            map.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("6", "8"), results);
        }

        [Test]
        public void TestMapB3()
        {
            var publisher = new FirableEvent<int>();
            var behavior = new Behavior<int>(1, publisher);
            var behavior1 = behavior.Map(x => x * 3);
            var results = new List<int>();
            var listener = behavior1.SubscribeWithInitialFire(results.Add);
            publisher.Fire(2);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(3, 6), results);
        }

        [Test]
        public void TestMapBLateListen()
        {
            var publisher = new FirableEvent<int>();
            var behavior = new Behavior<int>(6, publisher);
            var results = new List<string>();
            var map = behavior.Map(x => x.ToString(CultureInfo.InvariantCulture));
            publisher.Fire(2);
            var listener = map.SubscribeWithInitialFire(results.Add);
            publisher.Fire(8);
            listener.Dispose();
            map.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("2", "8"), results);
        }

        [Test]
        public void TestApply()
        {
            var pbf = new FirableEvent<Func<long, string>>();
            var bf = new Behavior<Func<long, string>>(b => "1 " + b, pbf);
            var pba = new FirableEvent<long>();
            var ba = new Behavior<long>(5L, pba);
            var results = new List<string>();
            var apply = Functor.Apply(bf, ba);
            var listener = apply.SubscribeWithInitialFire(results.Add);
            pbf.Fire(b => "12 " + b);
            pba.Fire(6L);
            listener.Dispose();
            apply.Dispose();
            bf.Dispose();
            ba.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
        }

        [Test]
        public void TestLift()
        {
            var pub1 = new FirableEvent<int>();
            var behavior1 = new Behavior<int>(1, pub1);
            var pub2 = new FirableEvent<long>();
            var behavior2 = new Behavior<long>(5L, pub2);
            var results = new List<string>();
            var combinedBehavior = Functor.Lift((x, y) => x + " " + y, behavior1, behavior2);
            var listener = combinedBehavior.SubscribeWithInitialFire(results.Add);
            pub1.Fire(12);
            pub2.Fire(6L);
            listener.Dispose();
            behavior1.Dispose();
            behavior2.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
        }

        /// <summary>
        /// A Lift Glitch is when two behaviors that are combined into a single behavior fired at the same time,
        /// and the resulting behavior fires twice. What should happen is that the resulting behavior
        /// fires once when the source behaviors fire simultaneously.
        /// </summary>
        [Test]
        public void TestLiftGlitch()
        {
            var publisher = new FirableEvent<int>();
            var behavior = new Behavior<int>(1, publisher);
            var mappedBehavior1 = behavior.Map(x => x * 3);
            var mappedBehavior2 = behavior.Map(x => x * 5);
            var results = new List<string>();
            var combinedBehavior = Functor.Lift((x, y) => x + " " + y, mappedBehavior1, mappedBehavior2);
            var listener = combinedBehavior.SubscribeWithInitialFire(results.Add);
            publisher.Fire(2);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("3 5", "6 10"), results);
        }

        [Test]
        public void TestHoldIsDelayed()
        {
            var evt = new FirableEvent<int>();
            var behavior = evt.Hold(0);
            var pair = evt.Snapshot((a, b) => a + " " + b, behavior);
            var results = new List<string>();
            var listener = pair.Subscribe(results.Add);
            evt.Fire(2);
            evt.Fire(3);
            listener.Dispose();
            evt.Dispose();
            behavior.Dispose();
            pair.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("2 0", "3 2"), results);
        }

        [Test]
        public void TestSwitchB()
        {
            var sink = new FirableEvent<Sb>();

            // Split each field o of SB so we can update multiple behaviors in a
            // single transaction.
            var behaviorA = sink.Map(s => s.C1).Filter(x => x != null).Hold('A');
            var behaviorB = sink.Map(s => s.C2).Filter(x => x != null).Hold('a');
            var bsw = sink.Map(s => s.Behavior).Filter(x => x != null).Hold(behaviorA);
            var behavior = bsw.Switch();
            var results = new List<char>();
            var listener = behavior.SubscribeWithInitialFire(c =>
            {
                Assert.IsNotNull(c, "c != null");
                results.Add(c.Value);
            });
            sink.Fire(new Sb('B', 'b', null));
            sink.Fire(new Sb('C', 'c', behaviorB));
            sink.Fire(new Sb('D', 'd', null));
            sink.Fire(new Sb('E', 'e', behaviorA));
            sink.Fire(new Sb('F', 'f', null));
            sink.Fire(new Sb(null, null, behaviorB));
            sink.Fire(new Sb(null, null, behaviorA));
            sink.Fire(new Sb('G', 'g', behaviorB));
            sink.Fire(new Sb('H', 'h', behaviorA));
            sink.Fire(new Sb('I', 'i', behaviorA));
            listener.Dispose();
            behaviorA.Dispose();
            behaviorB.Dispose();
            bsw.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('A', 'B', 'c', 'd', 'E', 'F', 'f', 'F', 'g', 'H', 'I'), results);
        }

        [Test]
        public void TestSwitchE()
        {
            var ese = new FirableEvent<Se>();
            var ea = ese.Map(s => s.C1);
            var eb = ese.Map(s => s.C2);
            var tmp1 = ese.Map(s => s.Event);
            var tmp2 = tmp1.Filter(x => x != null);
            var bsw = tmp2.Hold(ea);
            var o = new List<char>();
            var eo = bsw.Switch();
            var l = eo.Subscribe(o.Add);
            ese.Fire(new Se('A', 'a', null));
            ese.Fire(new Se('B', 'b', null));
            ese.Fire(new Se('C', 'c', eb));
            ese.Fire(new Se('D', 'd', null));
            ese.Fire(new Se('E', 'e', ea));
            ese.Fire(new Se('F', 'f', null));
            ese.Fire(new Se('G', 'g', eb));
            ese.Fire(new Se('H', 'h', ea));
            ese.Fire(new Se('I', 'i', ea));
            l.Dispose();
            ese.Dispose();
            ea.Dispose();
            eb.Dispose();
            bsw.Dispose();
            eo.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('A', 'B', 'C', 'd', 'e', 'F', 'G', 'h', 'I'), o);
        }

        [Test]
        public void TestCollect()
        {
            var ea = new FirableEvent<int>();
            var o = new List<int>();
            var sum = ea.Hold(100).Collect((a, s) => new Tuple<int, int>(a + s, a + s), 0);
            var l = sum.SubscribeWithInitialFire(o.Add);
            ea.Fire(5);
            ea.Fire(7);
            ea.Fire(1);
            ea.Fire(2);
            ea.Fire(3);
            l.Dispose();
            ea.Dispose();
            sum.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(100, 105, 112, 113, 115, 118), o);
        }

        [Test]
        public void TestAccum()
        {
            var ea = new FirableEvent<int>();
            var o = new List<int>();
            var sum = ea.Accum((a, s) => a + s, 100);
            var l = sum.SubscribeWithInitialFire(o.Add);
            ea.Fire(5);
            ea.Fire(7);
            ea.Fire(1);
            ea.Fire(2);
            ea.Fire(3);
            l.Dispose();
            ea.Dispose();
            sum.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(100, 105, 112, 113, 115, 118), o);
        }

        private class Se
        {
            public readonly char C1;
            public readonly char C2;
            public readonly Event<char> Event;

            public Se(char c1, char c2, Event<char> evt)
            {
                C1 = c1;
                C2 = c2;
                Event = evt;
            }
        }

        private class Sb
        {
            public readonly char? C1;
            public readonly char? C2;
            public readonly Behavior<char?> Behavior;

            public Sb(char? c1, char? c2, Behavior<char?> behavior)
            {
                C1 = c1;
                C2 = c2;
                Behavior = behavior;
            }
        }
    }
}