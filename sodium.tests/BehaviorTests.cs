using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace sodium.tests
{
    [TestFixture]
    public class BehaviorTests
    {
        [Test]
        public void TestSwitchB()
        {
            var esb = new EventSink<SB>();
            // Split each field out of SB so we can update multiple behaviours in a
            // single transaction.
            var ba = esb.Map<char?>(s => s.a).FilterNotNull().Hold('A');
            var bb = esb.Map(s => s.b).FilterNotNull().Hold('a');
            var bsw = esb.Map(s => s.sw).FilterNotNull().Hold(ba);
            var bo = Behavior<char?>.SwitchB(bsw);
            var results = new List<char?>();
            var l = bo.Value().Listen(results.Add);
            esb.Send(new SB('B', 'b', null));
            esb.Send(new SB('C', 'c', bb));
            esb.Send(new SB('D', 'd', null));
            esb.Send(new SB('E', 'e', ba));
            esb.Send(new SB('F', 'f', null));
            esb.Send(new SB(null, null, bb));
            esb.Send(new SB(null, null, ba));
            esb.Send(new SB('G', 'g', bb));
            esb.Send(new SB('H', 'h', ba));
            esb.Send(new SB('I', 'i', ba));
            l.Unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'c', 'd', 'E', 'F', 'f', 'F', 'g', 'H', 'I'), results);
        }

        [Test]
        public void TestSwitchB2()
        {
            var esb = new EventSink<SB>();
            // Split each field out of SB so we can update multiple behaviours in a
            // single transaction.
            var ba = esb.Map(s => s.a).FilterNotNull().Hold('A');
            var bb = esb.Map(s => s.b).FilterNotNull().Hold('a');
            var bsw = esb.Map(s => s.sw).FilterNotNull().Hold(ba);
            var bo = Behavior<char?>.SwitchB(bsw);
            var results = new List<char?>();
            var l = bo.Value().Listen(results.Add);
            esb.Send(new SB('B', 'b', null));
            esb.Send(new SB('C', 'c', bb));
            esb.Send(new SB('D', 'd', null));
            esb.Send(new SB('E', 'e', ba));
            l.Unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'c', 'd', 'E'), results);
        }

        [Test]
        public void TestSwitchB3()
        {
            var esb = new EventSink<SB>();
            // Split each field out of SB so we can update multiple behaviours in a
            // single transaction.
            var ba = esb.Map<char?>(s => s.a).FilterNotNull().Hold('A');
            var bb = esb.Map(s => s.b).FilterNotNull().Hold('a');
            var bsw = esb.Map(s => s.sw).FilterNotNull().Hold(ba);
            var bo = Behavior<char?>.SwitchB(bsw);
            var results = new List<char?>();
            var l = bo.Value().Listen(results.Add);
            esb.Send(new SB('B', 'b', null));
            esb.Send(new SB('C', 'c', bb));
            esb.Send(new SB('D', 'd', null));
            esb.Send(new SB('E', 'e', ba));
            esb.Send(new SB('F', 'f', null));
            l.Unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'c', 'd', 'E', 'F'), results);
        }

        [Test]
        public void TestSwitchB4()
        {
            var esb = new EventSink<SB>();
            // Split each field out of SB so we can update multiple behaviours in a
            // single transaction.
            var ba = esb.Map<char?>(s => s.a).FilterNotNull().Hold('A');
            var bb = esb.Map(s => s.b).FilterNotNull().Hold('a');
            var bsw = esb.Map(s => s.sw).FilterNotNull().Hold(ba);
            var bo = Behavior<char?>.SwitchB(bsw);
            var results = new List<char?>();
            var l = bo.Value().Listen(results.Add);
            esb.Send(new SB('B', 'b', null));
            esb.Send(new SB('C', 'c', bb));
            esb.Send(new SB('D', 'd', null));
            esb.Send(new SB('E', 'e', ba));
            esb.Send(new SB('F', 'f', ba));
            l.Unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'c', 'd', 'E', 'F'), results);
        }

        [Test]
        public void TestSwitchB5()
        {
            var esb = new EventSink<SB>();
            // Split each field out of SB so we can update multiple behaviours in a
            // single transaction.
            var ba = esb.Map<char?>(s => s.a).FilterNotNull().Hold('A');
            var bb = esb.Map(s => s.b).FilterNotNull().Hold('a');
            var bsw = esb.Map(s => s.sw).FilterNotNull().Hold(ba);
            var bo = Behavior<char?>.SwitchB(bsw);
            var results = new List<char?>();
            var l = bo.Value().Listen(results.Add);
            esb.Send(new SB('B', 'b', null));
            esb.Send(new SB('C', 'c', bb));
            esb.Send(new SB('D', 'd', null));
            esb.Send(new SB('E', 'e', ba));
            esb.Send(new SB('F', 'f', bb));
            l.Unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'c', 'd', 'E', 'f'), results);
        }

        class SB
        {
            public SB(char? a, char? b, Behavior<char?> sw)
            {
                this.a = a;
                this.b = b;
                this.sw = sw;
            }
            public char? a;
            public char? b;
            public Behavior<char?> sw;
        }

        [Test]
        public void TestTransactionHandlerImpl()
        {
            var results = new List<string>();
            var impl = new TransactionHandler<string>((t, a) => results.Add(a));
            impl.Run(null, "this is a test");
            Assert.AreEqual("this is a test", results[0]);
        }

        [Test]
        public void TestLambda1Impl()
        {
            var impl = new Lambda1<int, string>(a => a.ToString(CultureInfo.InvariantCulture));
            var results = impl.Apply(1);
            Assert.AreEqual("1", results);
        }

        [Test]
        public void TestHandlerImpl()
        {
            var results = new List<string>();
            var impl = new Handler<string>(results.Add);
            impl.Run("hello world!");
            Assert.AreEqual("hello world!", results[0]);
        }

        [Test]
        public void TestLift()
        {
            var a = new BehaviorSink<Int32>(1);
            var b = new BehaviorSink<long>(5L);
            var results = new List<String>();
            var l = Behavior<Int32>.Lift(
                (x, y) =>
                {
                    var res = x + " " + y;
                    return res;
                },
                a,
                b
            ).Value().Listen(results.Add);
            a.Send(12);
            b.Send(6L);
            l.Unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
        }

        [Test]
        public void TestLift1()
        {
            var a = new BehaviorSink<Int32>(1);
            var b = new BehaviorSink<long>(5L);
            var results = new List<String>();
            var l = Behavior<Int32>.Lift(
                (x, y) =>
                {
                    var res = x + " " + y;
                    return res;
                },
                a,
                b
            ).Value().Listen(results.Add);
            a.Send(12);
            l.Unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<string>.AsList("1 5", "12 5"), results);
        }

        [Test]
        public void TestLiftGlitch()
        {
            var a = new BehaviorSink<Int32>(1);
            var a3 = a.Map((x) => x * 3);
            var a5 = a.Map((x) => x * 5);
            var b = Behavior<Int32>.Lift<Int32, String>((x, y) => x + " " + y, a3, a5);
            var results = new List<String>();
            var l = b.Value().Listen(results.Add);

            a.Send(2);
            l.Unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<string>.AsList("3 5", "6 10"), results);
        }

        class SE
        {
            public SE(char? a, char? b, Event<char?> sw)
            {
                this.a = a;
                this.b = b;
                this.sw = sw;
            }
            public char? a;
            public char? b;
            public Event<char?> sw;
        }

        [Test]
        public void TestSwitchE()
        {
            var ese = new EventSink<SE>();
            var ea = ese.Map(s => s.a).FilterNotNull();
            var eb = ese.Map(s => s.b).FilterNotNull();
            var bsw = ese.Map(s => s.sw).FilterNotNull().Hold(ea);
            var results = new List<char?>();
            var eo = Behavior<char?>.SwitchE(bsw);
            var l = eo.Listen(results.Add);
            ese.Send(new SE('A', 'a', null));
            ese.Send(new SE('B', 'b', null));
            ese.Send(new SE('C', 'c', eb));
            ese.Send(new SE('D', 'd', null));
            ese.Send(new SE('E', 'e', ea));
            ese.Send(new SE('F', 'f', null));
            ese.Send(new SE('G', 'g', eb));
            ese.Send(new SE('H', 'h', ea));
            ese.Send(new SE('I', 'i', ea));
            l.Unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'C', 'd', 'e', 'F', 'G', 'h', 'I'), results);
        }
    }
}
