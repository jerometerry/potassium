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
        public void TestSendEvent()
        {
            var e = new Event<int>();
            var o = new List<int>();
            var l = e.Listen(o.Add);
            e.Fire(5);
            l.Stop();
            AssertArraysEqual(Arrays<int>.AsList(5), o);
            e.Fire(6);
            AssertArraysEqual(Arrays<int>.AsList(5), o);
        }

        [Test]
        public void TestMap()
        {
            var e = new Event<int>();
            var m = e.Map(x => x.ToString(CultureInfo.InvariantCulture));
            var o = new List<string>();
            var l = m.Listen(o.Add);
            e.Fire(5);
            l.Stop();
            AssertArraysEqual(Arrays<string>.AsList("5"), o);
        }

        [Test]
        public void TestMergeNonSimultaneous()
        {
            var e1 = new Event<int>();
            var e2 = new Event<int>();
            var o = new List<int>();
            var l = Event<int>.Merge(e1, e2).Listen(o.Add);
            e1.Fire(7);
            e2.Fire(9);
            e1.Fire(8);
            l.Stop();
            AssertArraysEqual(Arrays<int>.AsList(7, 9, 8), o);
        }

        [Test]
        public void TestMergeSimultaneous()
        {
            var e = new Event<int>();
            var o = new List<int>();
            var l = Event<int>.Merge(e, e).Listen(o.Add);
            e.Fire(7);
            e.Fire(9);
            l.Stop();
            AssertArraysEqual(Arrays<int>.AsList(7, 7, 9, 9), o);
        }

        [Test]
        public void TestCoalesce()
        {
            var e1 = new Event<int>();
            var e2 = new Event<int>();
            var o = new List<int>();
            IListener l =
                 Event<int>.Merge(e1, Event<int>.Merge(e1.Map(x => x * 100), e2))
                .Coalesce((a, b) => a + b)
                .Listen(o.Add);
            e1.Fire(2);
            e1.Fire(8);
            e2.Fire(40);
            l.Stop();
            AssertArraysEqual(Arrays<int>.AsList(202, 808, 40), o);
        }

        [Test]
        public void TestFilter()
        {
            var e = new Event<char>();
            var o = new List<char>();
            var l = e.Filter(char.IsUpper).Listen(o.Add);
            e.Fire('H');
            e.Fire('o');
            e.Fire('I');
            l.Stop();
            AssertArraysEqual(Arrays<char>.AsList('H', 'I'), o);
        }

        [Test]
        public void TestFilterNotNull()
        {
            var e = new Event<string>();
            var o = new List<string>();
            var l = e.FilterNotNull().Listen(o.Add);
            e.Fire("tomato");
            e.Fire(null);
            e.Fire("peach");
            l.Stop();
            AssertArraysEqual(Arrays<string>.AsList("tomato", "peach"), o);
        }

        [Test]
        public void TestLoopEvent()
        {
            var ea = new Event<int>();
            var eb = new Event<int>();
            var ec = Event<int>.MergeWith((x, y) => x + y, ea.Map(x => x % 10), eb);
            var ebO = ea.Map(x => x / 10).Filter(x => x != 0);
            eb.Loop(ebO);
            var o = new List<int>();
            var l = ec.Listen(o.Add);
            ea.Fire(2);
            ea.Fire(52);
            l.Stop();
            AssertArraysEqual(Arrays<int>.AsList(2, 7), o);
        }

        [Test]
        public void TestGate()
        {
            var ec = new Event<char>();
            var epred = Behavior<bool>.Sink(true);
            var o = new List<char>();
            var l = ec.Gate(epred).Listen(o.Add);
            ec.Fire('H');
            epred.Fire(false);
            ec.Fire('O');
            epred.Fire(true);
            ec.Fire('I');
            l.Stop();
            AssertArraysEqual(Arrays<char>.AsList('H', 'I'), o);
        }

        [Test]
        public void TestCollect()
        {
            var ea = new Event<int>();
            var o = new List<int>();
            var sum = ea.Collect(100, (a, s) => new Tuple<int, int>(a + s, a + s));
            var l = sum.Listen(o.Add);
            ea.Fire(5);
            ea.Fire(7);
            ea.Fire(1);
            ea.Fire(2);
            ea.Fire(3);
            l.Stop();
            AssertArraysEqual(Arrays<int>.AsList(105, 112, 113, 115, 118), o);
        }

        [Test]
        public void TestAccum()
        {
            var ea = new Event<int>();
            var o = new List<int>();
            var sum = ea.Accum(100, (a, s) => a + s);
            var l = sum.Updates().Listen(o.Add);
            ea.Fire(5);
            ea.Fire(7);
            ea.Fire(1);
            ea.Fire(2);
            ea.Fire(3);
            l.Stop();
            AssertArraysEqual(Arrays<int>.AsList(105, 112, 113, 115, 118), o);
        }

        [Test]
        public void TestOnce()
        {
            var e = new Event<char>();
            var o = new List<char>();
            var l = e.Once().Listen(o.Add);
            e.Fire('A');
            e.Fire('B');
            e.Fire('C');
            l.Stop();
            AssertArraysEqual(Arrays<char>.AsList('A'), o);
        }

        [Test]
        public void TestDelay()
        {
            var e = new Event<char>();
            var b = e.Hold(' ');
            var o = new List<char>();
            var l = e.Delay().Snapshot(b).Listen(o.Add);
            e.Fire('C');
            e.Fire('B');
            e.Fire('A');
            l.Stop();
            AssertArraysEqual(Arrays<char>.AsList('C', 'B', 'A'), o);
        }
    }
}