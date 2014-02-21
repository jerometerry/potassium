using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace sodium
{
    [TestFixture]
    public class EventTests
    {
        [Test]
        public void TestListen()
        {
            var esb = new EventSink<Int32>();
            var results = new List<Int32>();
            var listener = esb.listen(results.Add);
            Assert.IsNotNull(listener);
            esb.send(123);
            Assert.AreEqual(123, results[0]);
        }

        [Test]
        public void TestFilter()
        {
            var esb = new EventSink<Int32>();
            var even = esb.filter(a => a%2 == 0);
            var results = new List<Int32>();
            var listener = even.listen(results.Add);
            Assert.IsNotNull(listener);
            esb.send(1);
            esb.send(2);
            esb.send(3);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(2, results[0]);
        }

        [Test]
        public void TestFilter2()
        {
            var e = new EventSink<char>();
            var out_ = new List<char>();
            var l = e.filter(char.IsUpper).listen(out_.Add);
            e.send('H');
            e.send('o');
            e.send('I');
            l.unlisten();
            AssertArraysEqual(Arrays<char>.AsList('H','I'), out_);
        }

        [Test]
        public void TestFilterNotNull()
        {
            var esb = new EventSink<Int32?>();
            var nonNull = esb.filterNotNull();
            var results = new List<Int32>();
            var listener = nonNull.listen(a => results.Add(a.Value));
            Assert.IsNotNull(listener);
            esb.send(1);
            esb.send(null);
            esb.send(3);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(3, results[1]);
        }

        [Test]
        public void TestMap()
        {
            var esb = new EventSink<Int32>();
            var map = esb.map<string>(a => a.ToString());
            Assert.IsNotNull(map);
            var results = new List<string>();
            var listener = map.listen(results.Add);
            Assert.IsNotNull(listener);
            Assert.IsNotNull(map);
            esb.send(123);
            Assert.AreEqual("123", results[0]);
        }

        [Test]
        public void TestCoalesce()
        {
            var e1 = new EventSink<Int32>();
            var e2 = new EventSink<Int32>();
            var out_ = new List<Int32>();
            var l =
                 Event<Int32>.merge(e1,Event<Int32>.merge(e1.map(x => x * 100), e2))
                .coalesce((a,b) => a+b)
                .listen((x) => { out_.Add(x); });
            e1.send(2);
            e1.send(8);
            e2.send(40);
            l.unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(202, 808, 40), out_);
        }

        [Test]
        public void TestDelay()
        {
            var e = new EventSink<char>();
            var b = e.hold(' ');
            var out_ = new List<char>();
            var l = e.delay().snapshot(b).listen((x) => { out_.Add(x); });
            e.send('C');
            e.send('B');
            e.send('A');
            l.unlisten();
            AssertArraysEqual(Arrays<char>.AsList('C','B','A'), out_);
        }

        [Test]
        public void TestCollect()
        {
            var ea = new EventSink<Int32>();
            var out_ = new List<Int32>();
            var sum = ea.collect(100,
                //(a,s) -> new Tuple2(a+s, a+s)
                new Lambda2Impl<Int32, Int32, Tuple2<Int32,Int32>>((a,s) => new Tuple2<int, int>(a+s,a+s)));
            var l = sum.listen(out_.Add);
            ea.send(5);
            ea.send(7);
            ea.send(1);
            ea.send(2);
            ea.send(3);
            l.unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(105,112,113,115,118), out_);
        }

        [Test]
        public void TestAccum()
        {
            var ea = new EventSink<Int32>();
            var out_ = new List<Int32>();
            var sum = ea.accum(100, (a,s)=>a+s);
            var l = sum.updates().listen(out_.Add);
            ea.send(5);
            ea.send(7);
            ea.send(1);
            ea.send(2);
            ea.send(3);
            l.unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(105,112,113,115,118), out_);
        }

        [Test]
        public void TestGate()
        {
            var ec = new EventSink<char>();
            var epred = new BehaviorSink<Boolean>(true);
            var out_ = new List<Char>();
            var l = ec.gate(epred).listen(out_.Add);
            ec.send('H');
            epred.send(false);
            ec.send('O');
            epred.send(true);
            ec.send('I');
            l.unlisten();
            AssertArraysEqual(Arrays<char>.AsList('H','I'), out_);
        }

        [Test]
        public void TestOnce()
        {
            var e = new EventSink<char>();
            var out_ = new List<char>();
            var l = e.once().listen(out_.Add);
            e.send('A');
            e.send('B');
            e.send('C');
            l.unlisten();
            AssertArraysEqual(Arrays<char>.AsList('A'), out_);
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
