namespace Potassium.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Potassium.Core;
    using Potassium.Providers;
    using Potassium.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class EventTester : TestBase
    {
        [Test]
        public void TestPublishEvent()
        {
            var e = new FirableEvent<int>();
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
            var e = new FirableEvent<int>();
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
            var e1 = new FirableEvent<int>();
            var e2 = new FirableEvent<int>();
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
            var e = new FirableEvent<int>();
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
            var e1 = new FirableEvent<int>();
            var e2 = new FirableEvent<int>();
            var o = new List<int>();
            var evt2 = e1.Map(x => x * 100);
            var evt = evt2 | e2;
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
            var e = new FirableEvent<char>();
            var o = new List<char>();
            var l = (e & (char.IsUpper)).Subscribe(o.Add);
            e.Fire('H');
            e.Fire('o');
            e.Fire('I');
            l.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('H', 'I'), o);
        }

        [Test]
        public void TestFilterNotNull()
        {
            var e = new FirableEvent<string>();
            var o = new List<string>();
            var l = e.Filter(x => x != null).Subscribe(o.Add);
            e.Fire("tomato");
            e.Fire(null);
            e.Fire("peach");
            l.Dispose();
            AssertArraysEqual(Arrays<string>.AsList("tomato", "peach"), o);
        }

        [Test]
        public void TestLoopEvent()
        {
            var ea = new FirableEvent<int>();
            var eb = new EventFeed<int>();
            var evt = ea.Map(x => x % 10);
            var ec = evt.Merge(eb, (x, y) => x + y);
            var ebO = ea.Map(x => x / 10) & (x => x != 0);
            eb.Feed(ebO);
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
            var enabled = new IdentityPredicate(true);
            var ec = new FirableEvent<char>();
            var epred = new QueryPredicate(() => enabled.Value);
            var o = new List<char>();
            var l = ec.Gate(epred).Subscribe(o.Add);
            ec.Fire('H');
            enabled = new IdentityPredicate(false);
            ec.Fire('O');
            enabled = new IdentityPredicate(true);
            ec.Fire('I');
            l.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('H', 'I'), o);
        }

        [Test]
        public void TestCollect()
        {
            var ea = new FirableEvent<int>();
            var o = new List<int>();
            var sum = ea.Collect((a, s) => new Tuple<int, int>(a + s, a + s), 100);
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
            var ea = new FirableEvent<int>();
            var o = new List<int>();
            var sum = ea.Accum((a, s) => a + s, 100);
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
            var e = new FirableEvent<char>();
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
            var e = new FirableEvent<char>();
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