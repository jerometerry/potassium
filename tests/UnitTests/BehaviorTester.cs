namespace Sodium.Tests
{
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
            var listener = behavior.Subscribe(results.Add);
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
            var behavior = new BehaviorPublisher<int>(0);
            var evt = new EventPublisher<long>();
            var results = new List<string>();
            Func<long, int, string> snapshotFunction = (x, y) => string.Format("{0} {1}", x, y);
            var listener = evt.Snapshot(behavior, snapshotFunction).Subscribe(results.Add);

            evt.Publish(100L);
            behavior.Publish(2);
            evt.Publish(200L);
            behavior.Publish(9);
            behavior.Publish(1);
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
            var results = new List<int>();
            var listener = behavior.SubscribeValues(results.Add);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(12), results);
        }

        [Test]
        public void TestMapB()
        {
            var behavior = new BehaviorPublisher<int>(6);
            var results = new List<string>();
            var listener = behavior.Map(x => x.ToString(CultureInfo.InvariantCulture)).SubscribeValues(results.Add);
            behavior.Publish(8);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("6", "8"), results);
        }

        [Test]
        public void TestMapB2()
        {
            var behavior = new ConstantBehavior<int>(1);
            var behavior1 = behavior.Map(x => x * 3);
            var results = new List<int>();
            var listener = behavior1.SubscribeValues(results.Add);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(3), results);
        }

        [Test]
        public void TestMapB3()
        {
            var behavior = new BehaviorPublisher<int>(1);
            var behavior1 = behavior.Map(x => x * 3);
            var results = new List<int>();
            var listener = behavior1.SubscribeValues(results.Add);
            behavior.Publish(2);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(3, 6), results);
        }

        [Test]
        public void TestMapBLateListen()
        {
            var behavior = new BehaviorPublisher<int>(6);
            var results = new List<string>();
            var map = behavior.Map(x => x.ToString(CultureInfo.InvariantCulture));
            behavior.Publish(2);
            var listener = map.SubscribeValues(results.Add);
            behavior.Publish(8);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("2", "8"), results);
        }

        [Test]
        public void TestApply()
        {
            var bf = new BehaviorPublisher<Func<long, string>>(b => "1 " + b);
            var ba = new BehaviorPublisher<long>(5L);
            var results = new List<string>();
            var listener = ba.Apply(bf).SubscribeValues(results.Add);
            bf.Publish(b => "12 " + b);
            ba.Publish(6L);
            listener.Dispose();
            bf.Dispose();
            ba.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
        }

        [Test]
        public void TestLift()
        {
            var behavior1 = new BehaviorPublisher<int>(1);
            var behavior2 = new BehaviorPublisher<long>(5L);
            var results = new List<string>();
            var combinedBehavior = behavior1.Lift((x, y) => x + " " + y, behavior2);
            var listener = combinedBehavior.SubscribeValues(results.Add);
            behavior1.Publish(12);
            behavior2.Publish(6L);
            listener.Dispose();
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
            var behavior = new BehaviorPublisher<int>(1);
            var mappedBehavior1 = behavior.Map(x => x * 3);
            var mappedBehavior2 = behavior.Map(x => x * 5);
            var results = new List<string>();
            var combinedBehavior = mappedBehavior1.Lift((x, y) => x + " " + y, mappedBehavior2);
            var listener = combinedBehavior.SubscribeValues(results.Add);
            behavior.Publish(2);
            listener.Dispose();
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
            var listener = behavior.SubscribeValues(c =>
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
            var ea = new EventPublisher<int>();
            var sum = new BehaviorFeed<int>(0);
            var sumOut = ea.Snapshot(sum, (x, y) => x + y).Hold(0);
            sum.Feed(sumOut);
            var o = new List<int>();
            var l = sumOut.SubscribeValues(o.Add);
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
            var l = sum.SubscribeValues(o.Add);
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
            var l = sum.SubscribeValues(o.Add);
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

        private class Se
        {
            public readonly char C1;
            public readonly char C2;
            public readonly Sodium.Event<char> Event;

            public Se(char c1, char c2, Sodium.Event<char> evt)
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
            public readonly DiscreteBehavior<char?> Behavior;

            public Sb(char? c1, char? c2, DiscreteBehavior<char?> behavior)
            {
                C1 = c1;
                C2 = c2;
                Behavior = behavior;
            }
        }
    }
}