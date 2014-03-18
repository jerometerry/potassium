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
            var e = new EventSink<int>();
            var o = new List<int>();
            var l = e.Subscribe(o.Add);
            e.Fire(5);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(5), o);
            e.Fire(6);
            AssertArraysEqual(Arrays<int>.AsList(5), o);
        }

        [Test]
        public void TestMap()
        {
            var e = new EventSink<int>();
            var m = e.Map(x => x.ToString(CultureInfo.InvariantCulture));
            var o = new List<string>();
            var l = m.Subscribe(o.Add);
            e.Fire(5);
            l.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("5"), o);
        }

        [Test]
        public void TestMergeNonSimultaneous()
        {
            var e1 = new EventSink<int>();
            var e2 = new EventSink<int>();
            var o = new List<int>();
            var l = e1.Merge(e2).Subscribe(o.Add);
            e1.Fire(7);
            e2.Fire(9);
            e1.Fire(8);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(7, 9, 8), o);
        }

        [Test]
        public void TestMergeSimultaneous()
        {
            var e = new EventSink<int>();
            var o = new List<int>();
            var l = e.Merge(e).Subscribe(o.Add);
            e.Fire(7);
            e.Fire(9);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(7, 7, 9, 9), o);
        }

        [Test]
        public void TestCoalesce()
        {
            var e1 = new EventSink<int>();
            var e2 = new EventSink<int>();
            var o = new List<int>();
            var evt2 = e1.Map(x => x * 100);
            var evt = evt2.Merge(e2);
            var l = e1.Merge(evt)
                      .Coalesce((a, b) => a + b)
                      .Subscribe(o.Add);
            e1.Fire(2);
            e1.Fire(8);
            e2.Fire(40);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(202, 808, 40), o);
        }

        [Test]
        public void TestFilter()
        {
            var e = new EventSink<char>();
            var o = new List<char>();
            var l = e.Filter(char.IsUpper).Subscribe(o.Add);
            e.Fire('H');
            e.Fire('o');
            e.Fire('I');
            l.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('H', 'I'), o);
        }

        [Test]
        public void TestFilterNotNull()
        {
            var e = new EventSink<string>();
            var o = new List<string>();
            var l = e.FilterNotNull().Subscribe(o.Add);
            e.Fire("tomato");
            e.Fire(null);
            e.Fire("peach");
            l.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("tomato", "peach"), o);
        }

        [Test]
        public void TestLoopEvent()
        {
            var ea = new EventSink<int>();
            var eb = new EventLoop<int>();
            var evt = ea.Map(x => x % 10);
            var ec = evt.Merge(eb, (x, y) => x + y);
            var ebO = ea.Map(x => x / 10).Filter(x => x != 0);
            eb.Loop(ebO);
            var o = new List<int>();
            var l = ec.Subscribe(o.Add);
            ea.Fire(2);
            ea.Fire(52);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(2, 7), o);
        }

        [Test]
        public void TestGate()
        {
            var ec = new EventSink<char>();
            var epred = new BehaviorSink<bool>(true);
            var o = new List<char>();
            var l = ec.Gate(epred).Subscribe(o.Add);
            ec.Fire('H');
            epred.Fire(false);
            ec.Fire('O');
            epred.Fire(true);
            ec.Fire('I');
            l.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('H', 'I'), o);
        }

        [Test]
        public void TestCollect()
        {
            var ea = new EventSink<int>();
            var o = new List<int>();
            var sum = ea.Collect(100, (a, s) => new Tuple<int, int>(a + s, a + s));
            var l = sum.Subscribe(o.Add);
            ea.Fire(5);
            ea.Fire(7);
            ea.Fire(1);
            ea.Fire(2);
            ea.Fire(3);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(105, 112, 113, 115, 118), o);
        }

        [Test]
        public void TestAccum()
        {
            var ea = new EventSink<int>();
            var o = new List<int>();
            var sum = ea.Accum(100, (a, s) => a + s);
            var l = sum.Subscribe(o.Add);
            ea.Fire(5);
            ea.Fire(7);
            ea.Fire(1);
            ea.Fire(2);
            ea.Fire(3);
            l.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(105, 112, 113, 115, 118), o);
        }

        [Test]
        public void TestOnce()
        {
            var e = new EventSink<char>();
            var o = new List<char>();
            var l = e.Once().Subscribe(o.Add);
            e.Fire('A');
            e.Fire('B');
            e.Fire('C');
            l.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('A'), o);
        }

        [Test]
        public void TestDelay()
        {
            var e = new EventSink<char>();
            var b = e.Hold(' ');
            var o = new List<char>();
            var l = e.Delay().Snapshot(b).Subscribe(o.Add);
            e.Fire('C');
            e.Fire('B');
            e.Fire('A');
            l.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('C', 'B', 'A'), o);
        }
    }
}