using System.Globalization;

namespace Sodium.Tests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class EventTester : SodiumTestCase
    {
        [Test]
        public void TestSendEvent()
        {
            var e = new EventSink<Int32>();
            var o = new List<Int32>();
            var l = e.Listen(o.Add);
            e.Send(5);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(5), o);
            e.Send(6);
            AssertArraysEqual(Arrays<Int32>.AsList(5), o);
        }

        [Test]
        public void TestMap()
        {
            var e = new EventSink<Int32>();
            var m = e.Map(x => x.ToString(CultureInfo.InvariantCulture));
            var o = new List<String>();
            var l = m.Listen(o.Add);
            e.Send(5);
            l.Unlisten();
            AssertArraysEqual(Arrays<string>.AsList("5"), o);
        }

        [Test]
        public void TestMergeNonSimultaneous()
        {
            var e1 = new EventSink<Int32>();
            var e2 = new EventSink<Int32>();
            var o = new List<Int32>();
            var l = Event<Int32>.Merge(e1, e2).Listen(o.Add);
            e1.Send(7);
            e2.Send(9);
            e1.Send(8);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(7, 9, 8), o);
        }

        [Test]
        public void TestMergeSimultaneous()
        {
            var e = new EventSink<Int32>();
            var o = new List<Int32>();
            var l = Event<Int32>.Merge(e, e).Listen(o.Add);
            e.Send(7);
            e.Send(9);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(7, 7, 9, 9), o);
        }

        [Test]
        public void TestCoalesce()
        {
            var e1 = new EventSink<Int32>();
            var e2 = new EventSink<Int32>();
            var o = new List<Int32>();
            IListener l =
                 Event<Int32>.Merge(e1, Event<Int32>.Merge(e1.Map(x => x * 100), e2))
                .Coalesce((a, b) => a + b)
                .Listen(o.Add);
            e1.Send(2);
            e1.Send(8);
            e2.Send(40);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(202, 808, 40), o);
        }

        [Test]
        public void TestFilter()
        {
            var e = new EventSink<char>();
            var o = new List<char>();
            var l = e.Filter(char.IsUpper).Listen(o.Add);
            e.Send('H');
            e.Send('o');
            e.Send('I');
            l.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('H', 'I'), o);
        }

        [Test]
        public void TestFilterNotNull()
        {
            var e = new EventSink<String>();
            var o = new List<String>();
            var l = e.FilterNotNull().Listen(o.Add);
            e.Send("tomato");
            e.Send(null);
            e.Send("peach");
            l.Unlisten();
            AssertArraysEqual(Arrays<String>.AsList("tomato", "peach"), o);
        }

        [Test]
        public void TestLoopEvent()
        {
            var ea = new EventSink<Int32>();
            var eb = new EventLoop<Int32>();
            var ec = Event<Int32>.MergeWith((x, y) => x + y, ea.Map(x => x % 10), eb);
            var ebO = ea.Map(x => x / 10).Filter(x => x != 0);
            eb.Loop(ebO);
            var o = new List<Int32>();
            var l = ec.Listen(o.Add);
            ea.Send(2);
            ea.Send(52);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(2, 7), o);
        }

        [Test]
        public void TestGate()
        {
            var ec = new EventSink<char>();
            var epred = new BehaviorSink<Boolean>(true);
            var o = new List<char>();
            var l = ec.Gate(epred).Listen(o.Add);
            ec.Send('H');
            epred.Send(false);
            ec.Send('O');
            epred.Send(true);
            ec.Send('I');
            l.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('H', 'I'), o);
        }

        [Test]
        public void TestCollect()
        {
            var ea = new EventSink<Int32>();
            var o = new List<Int32>();
            var sum = ea.Collect(100,
                //(a,s) => new Tuple2(a+s, a+s)
                new Lambda2<Int32, Int32, Tuple2<Int32, Int32>>((a, s) => new Tuple2<Int32, Int32>(a + s, a + s))
            );
            var l = sum.Listen(o.Add);
            ea.Send(5);
            ea.Send(7);
            ea.Send(1);
            ea.Send(2);
            ea.Send(3);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(105, 112, 113, 115, 118), o);
        }

        [Test]
        public void TestAccum()
        {
            var ea = new EventSink<Int32>();
            var o = new List<Int32>();
            var sum = ea.Accum(100, (a, s) => a + s);
            var l = sum.Updates().Listen(o.Add);
            ea.Send(5);
            ea.Send(7);
            ea.Send(1);
            ea.Send(2);
            ea.Send(3);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(105, 112, 113, 115, 118), o);
        }

        [Test]
        public void TestOnce()
        {
            var e = new EventSink<char>();
            var o = new List<char>();
            var l = e.Once().Listen(o.Add);
            e.Send('A');
            e.Send('B');
            e.Send('C');
            l.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('A'), o);
        }

        [Test]
        public void TestDelay()
        {
            var e = new EventSink<char>();
            var b = e.Hold(' ');
            var o = new List<char>();
            var l = e.Delay().Snapshot(b).Listen(o.Add);
            e.Send('C');
            e.Send('B');
            e.Send('A');
            l.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('C', 'B', 'A'), o);
        }
    }

}