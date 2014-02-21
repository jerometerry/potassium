using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace sodium.tests
{
    [TestFixture]
    public class EventTests
    {
        [Test]
        public void TestListen()
        {
            var esb = new EventSink<Int32>();
            var results = new List<Int32>();
            var listener = esb.Listen(results.Add);
            Assert.IsNotNull(listener);
            esb.Send(123);
            Assert.AreEqual(123, results[0]);
        }

        [Test]
        public void TestFilter()
        {
            var esb = new EventSink<Int32>();
            var even = esb.Filter(a => a%2 == 0);
            var results = new List<Int32>();
            var listener = even.Listen(results.Add);
            Assert.IsNotNull(listener);
            esb.Send(1);
            esb.Send(2);
            esb.Send(3);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(2, results[0]);
        }

        [Test]
        public void TestFilter2()
        {
            var e = new EventSink<char>();
            var results = new List<char>();
            var l = e.Filter(char.IsUpper).Listen(results.Add);
            e.Send('H');
            e.Send('o');
            e.Send('I');
            l.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('H','I'), results);
        }

        [Test]
        public void TestFilterNotNull()
        {
            var esb = new EventSink<Int32?>();
            var nonNull = esb.FilterNotNull();
            var results = new List<Int32>();
            var listener = nonNull.Listen(a => results.Add(a.Value));
            Assert.IsNotNull(listener);
            esb.Send(1);
            esb.Send(null);
            esb.Send(3);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(3, results[1]);
        }

        [Test]
        public void TestMap()
        {
            var esb = new EventSink<Int32>();
            var map = esb.Map<string>(a => a.ToString());
            Assert.IsNotNull(map);
            var results = new List<string>();
            var listener = map.Listen(results.Add);
            Assert.IsNotNull(listener);
            Assert.IsNotNull(map);
            esb.Send(123);
            Assert.AreEqual("123", results[0]);
        }

        [Test]
        public void TestCoalesce()
        {
            var e1 = new EventSink<Int32>();
            var e2 = new EventSink<Int32>();
            var results = new List<Int32>();
            var l =
                 Event<Int32>.Merge(e1,Event<Int32>.Merge(e1.Map(x => x * 100), e2))
                .Coalesce((a,b) => a+b)
                .Listen(results.Add);
            e1.Send(2);
            e1.Send(8);
            e2.Send(40);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(202, 808, 40), results);
        }

        [Test]
        public void TestDelay()
        {
            var e = new EventSink<char>();
            var b = e.Hold(' ');
            var results = new List<char>();
            var l = e.Delay().Snapshot(b).Listen(results.Add);
            e.Send('C');
            e.Send('B');
            e.Send('A');
            l.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('C','B','A'), results);
        }

        [Test]
        public void TestCollect()
        {
            var ea = new EventSink<Int32>();
            var results = new List<Int32>();
            var sum = ea.Collect(100,
                //(a,s) -> new Tuple2(a+s, a+s)
                new Lambda2<Int32, Int32, Tuple2<Int32,Int32>>((a,s) => new Tuple2<int, int>(a+s,a+s)));
            var l = sum.Listen(results.Add);
            ea.Send(5);
            ea.Send(7);
            ea.Send(1);
            ea.Send(2);
            ea.Send(3);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(105,112,113,115,118), results);
        }

        [Test]
        public void TestAccum()
        {
            var ea = new EventSink<Int32>();
            var results = new List<Int32>();
            var sum = ea.Accum(100, (a,s)=>a+s);
            var l = sum.Updates().Listen(results.Add);
            ea.Send(5);
            ea.Send(7);
            ea.Send(1);
            ea.Send(2);
            ea.Send(3);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(105,112,113,115,118), results);
        }

        [Test]
        public void TestGate()
        {
            var ec = new EventSink<char>();
            var epred = new BehaviorSink<Boolean>(true);
            var results = new List<Char>();
            var l = ec.Gate(epred).Listen(results.Add);
            ec.Send('H');
            epred.Send(false);
            ec.Send('O');
            epred.Send(true);
            ec.Send('I');
            l.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('H','I'), results);
        }

        [Test]
        public void TestOnce()
        {
            var e = new EventSink<char>();
            var results = new List<char>();
            var l = e.Once().Listen(results.Add);
            e.Send('A');
            e.Send('B');
            e.Send('C');
            l.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('A'), results);
        }

        public static void AssertArraysEqual<TA>(List<TA> l1, List<TA> l2)
        {
            Assert.True(Arrays<TA>.AreArraysEqual(l1, l2));
        }

        internal static class Arrays<TA>
        {

            public static List<TA> AsList(params TA[] items)
            {
                return new List<TA>(items);
            }

            public static bool AreArraysEqual(List<TA> l1, List<TA> l2)
            {
                if (l1.Count != l2.Count)
                    return false;

                l1.Sort();
                l2.Sort();

                for (int i = 0; i < l1.Count; i++)
                {
                    TA item1 = l1[i];
                    TA item2 = l2[i];
                    if (!item1.Equals(item2))
                        return false;
                }

                return true;
            }

            public static void AssertArraysEqual(List<TA> l1, List<TA> l2)
            {
                Assert.True(AreArraysEqual(l1, l2));
            }
        }
    }
}
