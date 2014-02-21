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
            // Split each field out_ of SB so we can update multiple behaviours in a
            // single transaction.
            var ba = esb.Map<char?>(s => s.a).FilterNotNull().Hold('A');
            var bb = esb.Map(s => s.b).FilterNotNull().Hold('a');
            var bsw = esb.Map(s => s.sw).FilterNotNull().Hold(ba);
            var bo = Behavior<char?>.SwitchB(bsw);
            var out_ = new List<char?>();
            var l = bo.Value().Listen(out_.Add);
            esb.send(new SB('B', 'b', null));
            esb.send(new SB('C', 'c', bb));
            esb.send(new SB('D', 'd', null));
            esb.send(new SB('E', 'e', ba));
            esb.send(new SB('F', 'f', null));
            esb.send(new SB(null, null, bb));
            esb.send(new SB(null, null, ba));
            esb.send(new SB('G', 'g', bb));
            esb.send(new SB('H', 'h', ba));
            esb.send(new SB('I', 'i', ba));
            l.unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'c', 'd', 'E', 'F', 'f', 'F', 'g', 'H', 'I'), out_);
        }

        [Test]
        public void TestSwitchB2()
        {
            var esb = new EventSink<SB>();
            // Split each field out_ of SB so we can update multiple behaviours in a
            // single transaction.
            var ba = esb.Map<char?>(s => s.a).FilterNotNull().Hold('A');
            var bb = esb.Map(s => s.b).FilterNotNull().Hold('a');
            var bsw = esb.Map(s => s.sw).FilterNotNull().Hold(ba);
            var bo = Behavior<char?>.SwitchB(bsw);
            var out_ = new List<char?>();
            var l = bo.Value().Listen(out_.Add);
            esb.send(new SB('B', 'b', null));
            esb.send(new SB('C', 'c', bb));
            esb.send(new SB('D', 'd', null));
            esb.send(new SB('E', 'e', ba));
            l.unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'c', 'd', 'E'), out_);
        }

        [Test]
        public void TestSwitchB3()
        {
            var esb = new EventSink<SB>();
            // Split each field out_ of SB so we can update multiple behaviours in a
            // single transaction.
            var ba = esb.Map<char?>(s => s.a).FilterNotNull().Hold('A');
            var bb = esb.Map(s => s.b).FilterNotNull().Hold('a');
            var bsw = esb.Map(s => s.sw).FilterNotNull().Hold(ba);
            var bo = Behavior<char?>.SwitchB(bsw);
            var out_ = new List<char?>();
            var l = bo.Value().Listen(out_.Add);
            esb.send(new SB('B', 'b', null));
            esb.send(new SB('C', 'c', bb));
            esb.send(new SB('D', 'd', null));
            esb.send(new SB('E', 'e', ba));
            esb.send(new SB('F', 'f', null));
            l.unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'c', 'd', 'E', 'F'), out_);
        }

        [Test]
        public void TestSwitchB4()
        {
            var esb = new EventSink<SB>();
            // Split each field out_ of SB so we can update multiple behaviours in a
            // single transaction.
            var ba = esb.Map<char?>(s => s.a).FilterNotNull().Hold('A');
            var bb = esb.Map(s => s.b).FilterNotNull().Hold('a');
            var bsw = esb.Map(s => s.sw).FilterNotNull().Hold(ba);
            var bo = Behavior<char?>.SwitchB(bsw);
            var out_ = new List<char?>();
            var l = bo.Value().Listen(out_.Add);
            esb.send(new SB('B', 'b', null));
            esb.send(new SB('C', 'c', bb));
            esb.send(new SB('D', 'd', null));
            esb.send(new SB('E', 'e', ba));
            esb.send(new SB('F', 'f', ba));
            l.unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'c', 'd', 'E', 'F'), out_);
        }

        [Test]
        public void TestSwitchB5()
        {
            var esb = new EventSink<SB>();
            // Split each field out_ of SB so we can update multiple behaviours in a
            // single transaction.
            var ba = esb.Map<char?>(s => s.a).FilterNotNull().Hold('A');
            var bb = esb.Map(s => s.b).FilterNotNull().Hold('a');
            var bsw = esb.Map(s => s.sw).FilterNotNull().Hold(ba);
            var bo = Behavior<char?>.SwitchB(bsw);
            var out_ = new List<char?>();
            var l = bo.Value().Listen(out_.Add);
            esb.send(new SB('B', 'b', null));
            esb.send(new SB('C', 'c', bb));
            esb.send(new SB('D', 'd', null));
            esb.send(new SB('E', 'e', ba));
            esb.send(new SB('F', 'f', bb));
            l.unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'c', 'd', 'E', 'f'), out_);
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
            var impl = new Lambda1Impl<int, string>(a => a.ToString(CultureInfo.InvariantCulture));
            var results = impl.apply(1);
            Assert.AreEqual("1", results);
        }

        [Test]
        public void TestHandlerImpl()
        {
            List<string> results = new List<string>();
            var impl = new HandlerImpl<string>(results.Add);
            impl.run("hello world!");
            Assert.AreEqual("hello world!", results[0]);
        }

        [Test]
        public void TestLift()
        {
            var a = new BehaviorSink<Int32>(1);
            var b = new BehaviorSink<long>(5L);
            var out_ = new List<String>();
            var l = Behavior<Int32>.Lift(
                (x, y) =>
                {
                    var res = x + " " + y;
                    return res;
                },
                a,
                b
            ).Value().Listen(out_.Add);
            a.Send(12);
            b.Send(6L);
            l.unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<string>.AsList("1 5", "12 5", "12 6"), out_);
        }

        [Test]
        public void TestLift1()
        {
            var a = new BehaviorSink<Int32>(1);
            var b = new BehaviorSink<long>(5L);
            var out_ = new List<String>();
            var l = Behavior<Int32>.Lift(
                (x, y) =>
                {
                    var res = x + " " + y;
                    return res;
                },
                a,
                b
            ).Value().Listen(out_.Add);
            a.Send(12);
            l.unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<string>.AsList("1 5", "12 5"), out_);
        }

        [Test]
        public void TestLiftGlitch()
        {
            var a = new BehaviorSink<Int32>(1);
            var a3 = a.Map((x) => x * 3);
            var a5 = a.Map((x) => x * 5);
            var b = Behavior<Int32>.Lift<Int32, Int32, String>((x, y) => x + " " + y, a3, a5);
            var out_ = new List<String>();
            var l = b.Value().Listen(out_.Add);

            a.Send(2);
            l.unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<string>.AsList("3 5", "6 10"), out_);
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
            var out_ = new List<char?>();
            var eo = Behavior<char?>.SwitchE(bsw);
            var l = eo.Listen(out_.Add);
            ese.send(new SE('A', 'a', null));
            ese.send(new SE('B', 'b', null));
            ese.send(new SE('C', 'c', eb));
            ese.send(new SE('D', 'd', null));
            ese.send(new SE('E', 'e', ea));
            ese.send(new SE('F', 'f', null));
            ese.send(new SE('G', 'g', eb));
            ese.send(new SE('H', 'h', ea));
            ese.send(new SE('I', 'i', ea));
            l.unlisten();
            EventTests.AssertArraysEqual(EventTests.Arrays<char?>.AsList('A', 'B', 'C', 'd', 'e', 'F', 'G', 'h', 'I'), out_);
        }
    }
}
