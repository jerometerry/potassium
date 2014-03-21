namespace JT.Rx.Net.Tests
{
    using JT.Rx.Net.Core;
    using JT.Rx.Net.Continuous;
    using JT.Rx.Net.Discrete;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NUnit.Framework;

    [TestFixture]
    public class BehaviorTester : SodiumTestCase
    {
        [Test]
        public void TestHold()
        {
            var evt = new EventPublisher<int>();
            var behavior = evt.Hold(0);
            var results = new List<int>();
            var listener = behavior.Source.Subscribe(results.Add);
            evt.Publish(2);
            evt.Publish(9);
            listener.Dispose();
            evt.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(2, 9), results);
        }

        [Test]
        public void TestSnapshot()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new EventDrivenBehavior<int>(0, publisher);
            var evt = new EventPublisher<long>();
            var results = new List<string>();
            Func<long, int, string> snapshotFunction = (x, y) => string.Format("{0} {1}", x, y);
            var listener = evt.Snapshot(behavior, snapshotFunction).Subscribe(results.Add);

            evt.Publish(100L);
            publisher.Publish(2);
            evt.Publish(200L);
            publisher.Publish(9);
            publisher.Publish(1);
            evt.Publish(300L);
            listener.Dispose();
            behavior.Dispose();
            evt.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("100 0", "200 2", "300 1"), results);
        }

        [Test]
        public void TestConstantBehavior()
        {
            var behavior = new ConstantBehavior<int>(12);
            Assert.AreEqual(12, behavior.Value);
            behavior.Dispose();
        }

        [Test]
        public void TestMapB()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new EventDrivenBehavior<int>(6, publisher);
            var results = new List<string>();
            var map = behavior.Map(x => x.ToString(CultureInfo.InvariantCulture));
            var values = map.Values();
            var listener = values.Subscribe(results.Add);
            publisher.Publish(8);
            listener.Dispose();
            values.Dispose();
            map.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("6", "8"), results);
        }

        [Test]
        public void TestMapB2()
        {
            var behavior = new ConstantBehavior<int>(1);
            var behavior1 = new MonadBinder<int, int>(behavior, x => x * 3);
            Assert.AreEqual(3, behavior1.Value);
            behavior1.Dispose();
            behavior.Dispose();
        }

        [Test]
        public void TestMapB3()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new EventDrivenBehavior<int>(1, publisher);
            var behavior1 = behavior.Map(x => x * 3);
            var results = new List<int>();
            var values = behavior1.Values();
            var listener = values.Subscribe(results.Add);
            publisher.Publish(2);
            listener.Dispose();
            values.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(3, 6), results);
        }

        [Test]
        public void TestMapBLateListen()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new EventDrivenBehavior<int>(6, publisher);
            var results = new List<string>();
            var map = behavior.Map(x => x.ToString(CultureInfo.InvariantCulture));
            publisher.Publish(2);
            var values = map.Values();
            var listener = values.Subscribe(results.Add);
            publisher.Publish(8);
            listener.Dispose();
            map.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("2", "8"), results);
        }

        [Test]
        public void TestApply()
        {
            var pbf = new EventPublisher<Func<long, string>>();
            var bf = new EventDrivenBehavior<Func<long, string>>(b => "1 " + b, pbf);
            var pba = new EventPublisher<long>();
            var ba = new EventDrivenBehavior<long>(5L, pba);
            var results = new List<string>();
            var apply = ba.Apply(bf);
            var values = apply.Values();
            var listener = values.Subscribe(results.Add);
            pbf.Publish(b => "12 " + b);
            pba.Publish(6L);
            listener.Dispose();
            values.Dispose();
            apply.Dispose();
            bf.Dispose();
            ba.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
        }

        [Test]
        public void TestLift()
        {
            var pub1 = new EventPublisher<int>();
            var behavior1 = new EventDrivenBehavior<int>(1, pub1);
            var pub2 = new EventPublisher<long>();
            var behavior2 = new EventDrivenBehavior<long>(5L, pub2);
            var results = new List<string>();
            var combinedBehavior = behavior1.Lift((x, y) => x + " " + y, behavior2);
            var values = combinedBehavior.Values();
            var listener = values.Subscribe(results.Add);
            pub1.Publish(12);
            pub2.Publish(6L);
            listener.Dispose();
            values.Dispose();
            behavior1.Dispose();
            behavior2.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
        }

        /// <summary>
        /// A Lift Glitch is when two behaviors that are combined into a single behavior publish at the same time,
        /// and the resulting behavior publishes twice. What should happen is that the resulting behavior
        /// publishes once when the source behaviors publish simultaneously.
        /// </summary>
        [Test]
        public void TestLiftGlitch()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new EventDrivenBehavior<int>(1, publisher);
            var mappedBehavior1 = behavior.Map(x => x * 3);
            var mappedBehavior2 = behavior.Map(x => x * 5);
            var results = new List<string>();
            var combinedBehavior = mappedBehavior1.Lift((x, y) => x + " " + y, mappedBehavior2);
            var values = combinedBehavior.Values();
            var listener = values.Subscribe(results.Add);
            publisher.Publish(2);
            listener.Dispose();
            values.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("3 5", "6 10"), results);
        }

        [Test]
        public void TestHoldIsDelayed()
        {
            var evt = new EventPublisher<int>();
            var behavior = evt.Hold(0);
            var pair = evt.Snapshot(behavior, (a, b) => a + " " + b);
            var results = new List<string>();
            var listener = pair.Subscribe(results.Add);
            evt.Publish(2);
            evt.Publish(3);
            listener.Dispose();
            evt.Dispose();
            behavior.Dispose();
            pair.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("2 0", "3 2"), results);
        }

        [Test]
        public void TestSwitchB()
        {
            var sink = new EventPublisher<Sb>();

            // Split each field o of SB so we can update multiple behaviors in a
            // single transaction.
            var behaviorA = sink.Map(s => s.C1).FilterNotNull().Hold('A');
            var behaviorB = sink.Map(s => s.C2).FilterNotNull().Hold('a');
            var bsw = sink.Map(s => s.Behavior).FilterNotNull().Hold(behaviorA);
            var behavior = bsw.Switch();
            var results = new List<char>();
            var values = behavior.Values();
            var listener = values.Subscribe(c =>
            {
                Assert.IsNotNull(c, "c != null");
                results.Add(c.Value);
            });
            sink.Publish(new Sb('B', 'b', null));
            sink.Publish(new Sb('C', 'c', behaviorB));
            sink.Publish(new Sb('D', 'd', null));
            sink.Publish(new Sb('E', 'e', behaviorA));
            sink.Publish(new Sb('F', 'f', null));
            sink.Publish(new Sb(null, null, behaviorB));
            sink.Publish(new Sb(null, null, behaviorA));
            sink.Publish(new Sb('G', 'g', behaviorB));
            sink.Publish(new Sb('H', 'h', behaviorA));
            sink.Publish(new Sb('I', 'i', behaviorA));
            listener.Dispose();
            values.Dispose();
            behaviorA.Dispose();
            behaviorB.Dispose();
            bsw.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('A', 'B', 'c', 'd', 'E', 'F', 'f', 'F', 'g', 'H', 'I'), results);
        }

        [Test]
        public void TestSwitchE()
        {
            var ese = new EventPublisher<Se>();
            var ea = ese.Map(s => s.C1).FilterNotNull();
            var eb = ese.Map(s => s.C2).FilterNotNull();
            var tmp1 = ese.Map(s => s.Event);
            var tmp2 = tmp1.FilterNotNull();
            var bsw = tmp2.Hold(ea);
            var o = new List<char>();
            var eo = bsw.Switch();
            var l = eo.Subscribe(o.Add);
            ese.Publish(new Se('A', 'a', null));
            ese.Publish(new Se('B', 'b', null));
            ese.Publish(new Se('C', 'c', eb));
            ese.Publish(new Se('D', 'd', null));
            ese.Publish(new Se('E', 'e', ea));
            ese.Publish(new Se('F', 'f', null));
            ese.Publish(new Se('G', 'g', eb));
            ese.Publish(new Se('H', 'h', ea));
            ese.Publish(new Se('I', 'i', ea));
            l.Dispose();
            ese.Dispose();
            ea.Dispose();
            eb.Dispose();
            bsw.Dispose();
            eo.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('A', 'B', 'C', 'd', 'e', 'F', 'G', 'h', 'I'), o);
        }

        [Test]
        public void TestLoopBehavior()
        {
            var feed = new EventFeed<int>();
            var ea = new EventPublisher<int>();
            var sum = new EventDrivenBehavior<int>(0, feed);
            var sumOut = ea.Snapshot(sum, (x, y) => x + y).Hold(0);
            feed.Feed(sumOut.Source);
            var o = new List<int>();
            var values = sumOut.Values();
            var l = values.Subscribe(o.Add);
            ea.Publish(2);
            ea.Publish(3);
            ea.Publish(1);
            var sample = sum.Value;
            l.Dispose();
            ea.Dispose();
            sum.Dispose();
            sumOut.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(0, 2, 5, 6), o);
            Assert.AreEqual(6, sample);
        }

        [Test]
        public void TestCollect()
        {
            var ea = new EventPublisher<int>();
            var o = new List<int>();
            var sum = ea.Hold(100).Collect(0, (a, s) => new Tuple<int, int>(a + s, a + s));
            var values = sum.Values();
            var l = values.Subscribe(o.Add);
            ea.Publish(5);
            ea.Publish(7);
            ea.Publish(1);
            ea.Publish(2);
            ea.Publish(3);
            l.Dispose();
            ea.Dispose();
            sum.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(100, 105, 112, 113, 115, 118), o);
        }

        [Test]
        public void TestAccum()
        {
            var ea = new EventPublisher<int>();
            var o = new List<int>();
            var sum = ea.Accum(100, (a, s) => a + s);
            var values = sum.Values();
            var l = values.Subscribe(o.Add);
            ea.Publish(5);
            ea.Publish(7);
            ea.Publish(1);
            ea.Publish(2);
            ea.Publish(3);
            l.Dispose();
            values.Dispose();
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
            public readonly EventDrivenBehavior<char?> Behavior;

            public Sb(char? c1, char? c2, EventDrivenBehavior<char?> behavior)
            {
                C1 = c1;
                C2 = c2;
                Behavior = behavior;
            }
        }
    }
}