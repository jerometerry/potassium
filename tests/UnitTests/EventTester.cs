namespace Sodium.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NUnit.Framework;

    [TestFixture]
    public class EventTester : SodiumTestCase
    {
        [Test]
        public void TestPublishEvent()
        {
            var e = new EventPublisher<int>();
            var o = new List<int>();
            var l = e.Subscribe(o.Add);
            e.Publish(5);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(5), o);
            e.Publish(6);
            AssertArraysEqual(Arrays<int>.AsList(5), o);
        }

        [Test]
        public void TestMap()
        {
            var e = new EventPublisher<int>();
            var m = e.Map(x => x.ToString(CultureInfo.InvariantCulture));
            var o = new List<string>();
            var l = m.Subscribe(o.Add);
            e.Publish(5);
            l.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("5"), o);
        }

        [Test]
        public void TestMergeNonSimultaneous()
        {
            var e1 = new EventPublisher<int>();
            var e2 = new EventPublisher<int>();
            var o = new List<int>();
            var l = e1.Merge(e2).Subscribe(o.Add);
            e1.Publish(7);
            e2.Publish(9);
            e1.Publish(8);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(7, 9, 8), o);
        }

        [Test]
        public void TestMergeSimultaneous()
        {
            var e = new EventPublisher<int>();
            var o = new List<int>();
            var l = e.Merge(e).Subscribe(o.Add);
            e.Publish(7);
            e.Publish(9);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(7, 7, 9, 9), o);
        }

        [Test]
        public void TestCoalesce()
        {
            var e1 = new EventPublisher<int>();
            var e2 = new EventPublisher<int>();
            var o = new List<int>();
            var evt2 = e1.Map(x => x * 100);
            var evt = evt2.Merge(e2);
            var l = e1.Merge(evt)
                      .Coalesce((a, b) => a + b)
                      .Subscribe(o.Add);
            e1.Publish(2);
            e1.Publish(8);
            e2.Publish(40);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(202, 808, 40), o);
        }

        [Test]
        public void TestFilter()
        {
            var e = new EventPublisher<char>();
            var o = new List<char>();
            var l = e.Filter(char.IsUpper).Subscribe(o.Add);
            e.Publish('H');
            e.Publish('o');
            e.Publish('I');
            l.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('H', 'I'), o);
        }

        [Test]
        public void TestFilterNotNull()
        {
            var e = new EventPublisher<string>();
            var o = new List<string>();
            var l = e.FilterNotNull().Subscribe(o.Add);
            e.Publish("tomato");
            e.Publish(null);
            e.Publish("peach");
            l.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("tomato", "peach"), o);
        }

        [Test]
        public void TestLoopEvent()
        {
            var ea = new EventPublisher<int>();
            var eb = new EventFeed<int>();
            var evt = ea.Map(x => x % 10);
            var ec = evt.Merge(eb, (x, y) => x + y);
            var ebO = ea.Map(x => x / 10).Filter(x => x != 0);
            eb.Feed(ebO);
            var o = new List<int>();
            var l = ec.Subscribe(o.Add);
            ea.Publish(2);
            ea.Publish(52);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(2, 7), o);
        }

        [Test]
        public void TestGate()
        {
            var ec = new EventPublisher<char>();
            var epred = new BehaviorPublisher<bool>(true);
            var o = new List<char>();
            var l = ec.Gate(epred).Subscribe(o.Add);
            ec.Publish('H');
            epred.Publish(false);
            ec.Publish('O');
            epred.Publish(true);
            ec.Publish('I');
            l.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('H', 'I'), o);
        }

        [Test]
        public void TestCollect()
        {
            var ea = new EventPublisher<int>();
            var o = new List<int>();
            var sum = ea.Collect(100, (a, s) => new Tuple<int, int>(a + s, a + s));
            var l = sum.Subscribe(o.Add);
            ea.Publish(5);
            ea.Publish(7);
            ea.Publish(1);
            ea.Publish(2);
            ea.Publish(3);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(105, 112, 113, 115, 118), o);
        }

        [Test]
        public void TestAccum()
        {
            var ea = new EventPublisher<int>();
            var o = new List<int>();
            var sum = ea.Accum(100, (a, s) => a + s);
            var l = sum.Source.Subscribe(o.Add);
            ea.Publish(5);
            ea.Publish(7);
            ea.Publish(1);
            ea.Publish(2);
            ea.Publish(3);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(105, 112, 113, 115, 118), o);
        }

        [Test]
        public void TestOnce()
        {
            var e = new EventPublisher<char>();
            var o = new List<char>();
            var l = e.Once().Subscribe(o.Add);
            e.Publish('A');
            e.Publish('B');
            e.Publish('C');
            l.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('A'), o);
        }

        [Test]
        public void TestDelay()
        {
            var e = new EventPublisher<char>();
            var b = e.Hold(' ');
            var o = new List<char>();
            var l = e.Delay().Snapshot(b).Subscribe(o.Add);
            e.Publish('C');
            e.Publish('B');
            e.Publish('A');
            l.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('C', 'B', 'A'), o);
        }
    }
}