using System.Diagnostics;
using System.Globalization;

namespace Sodium.Tests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class BehaviorTester : SodiumTestCase
    {
        [Test]
        public void TestHold()
        {
            var evt = new EventSink<Int32>();
            var behavior = evt.Hold(0);
            var results = new List<Int32>();
            var listener = behavior.Updates().Listen(results.Add);
            evt.Send(2);
            evt.Send(9);
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(2, 9), results);
        }

        [Test]
        public void TestSnapshot()
        {
            var behavior = new BehaviorSink<Int32>(0);
            var evt = new EventSink<Int64>();
            var results = new List<String>();
            var snapshotFunction = new Lambda2<long, int, string>(
                (x, y) => string.Format("{0} {1}", x, y));
            var listener = evt.Snapshot(behavior, snapshotFunction).Listen(results.Add);

            evt.Send(100L);
            behavior.Send(2);
            evt.Send(200L);
            behavior.Send(9);
            behavior.Send(1);
            evt.Send(300L);
            listener.Unlisten();
            AssertArraysEqual(Arrays<string>.AsList("100 0", "200 2", "300 1"), results);
        }

        [Test]
        public void TestValues()
        {
            var behavior = new BehaviorSink<Int32>(9);
            var results = new List<Int32>();
            var listener = behavior.Value().Listen(results.Add);
            behavior.Send(2);
            behavior.Send(7);
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9, 2, 7), results);
        }

        [Test]
        public void TestConstantBehavior()
        {
            var behavior = new Behavior<Int32>(12);
            var results = new List<Int32>();
            var listener = behavior.Value().Listen(results.Add);
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(12), results);
        }

        [Test]
        public void TestValuesThenMap()
        {
            var behavior = new BehaviorSink<Int32>(9);
            var results = new List<Int32>();
            var map = new Lambda1<Int32, Int32>(x => x + 100);
            var l = behavior.Value().Map(map).Listen(results.Add);
            behavior.Send(2);
            behavior.Send(7);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(109, 102, 107), results);
        }

        [Test]
        public void TestValuesTwiceThenMap()
        {
            var behavior = new BehaviorSink<Int32>(9);
            var results = new List<Int32>();
            var map = new Lambda1<Int32, Int32>(x => x + 100);
            var listener = DoubleUp(behavior.Value()).Map(map).Listen(results.Add);
            behavior.Send(2);
            behavior.Send(7);
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(109, 109, 102, 102, 107, 107), results);
        }


        [Test]
        public void TestValuesThenCoalesce()
        {
            var behavior = new BehaviorSink<Int32>(9);
            var results = new List<Int32>();
            var coaleseFunction = new Lambda2<Int32, Int32, Int32>((fst, snd) => snd);
            var listener = behavior.Value().Coalesce(coaleseFunction).Listen(results.Add);
            behavior.Send(2);
            behavior.Send(7);
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9, 2, 7), results);
        }

        
        [Test]
	    public void TestValuesTwiceThenCoalesce() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
            var coaleseFunction = new Lambda2<Int32, Int32, Int32>((fst, snd) => fst + snd);
            var listener = DoubleUp(behavior.Value()).Coalesce(coaleseFunction).Listen(results.Add);
		    behavior.Send(2);
		    behavior.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(18, 4, 14), results);
	    }
        
        [Test]
	    public void TestValuesThenSnapshot() 
        {
		    var behaviorInt32 = new BehaviorSink<Int32>(9);
		    var behaviorChar = new BehaviorSink<char>('a');
		    var results = new List<char>();
            var listener = behaviorInt32.Value().Snapshot(behaviorChar).Listen(results.Add);
		    behaviorChar.Send('b');
		    behaviorInt32.Send(2);
		    behaviorChar.Send('c');
		    behaviorInt32.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('a', 'b', 'c'), results);
	    }
        
        [Test]
	    public void TestValuesTwiceThenSnapshot() 
        {
		    var behaviorInt32 = new BehaviorSink<Int32>(9);
		    var behaviorChar = new BehaviorSink<char>('a');
		    var results = new List<char>();
            var listener = DoubleUp(behaviorInt32.Value()).Snapshot(behaviorChar).Listen(results.Add);
		    behaviorChar.Send('b');
		    behaviorInt32.Send(2);
		    behaviorChar.Send('c');
		    behaviorInt32.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('a', 'a', 'b', 'b', 'c', 'c'), results);
	    }

        
        [Test]
	    public void TestValuesThenMerge() 
        {
		    var behavior1 = new BehaviorSink<Int32>(9);
		    var behavior2 = new BehaviorSink<Int32>(2);
		    var results = new List<Int32>();
            var combiningFunction = new Lambda2<Int32, Int32, Int32>((x, y) => x+y);
            var listener = Event<Int32>.MergeWith(combiningFunction, behavior1.Value(), behavior2.Value()).Listen(results.Add);
		    behavior1.Send(1);
		    behavior2.Send(4);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(11, 1, 4), results);
	    }
        
        [Test]
	    public void TestValuesThenFilter() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
            var predicate = new Lambda1<int, bool>(a => true);
            var listener = behavior.Value().Filter(predicate).Listen(results.Add);
		    behavior.Send(2);
		    behavior.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9, 2, 7), results);
	    }
        
        [Test]
	    public void TestValuesTwiceThenFilter() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
            var predicate = new Lambda1<Int32, bool>(a => true);
            var listener = DoubleUp(behavior.Value()).Filter(predicate).Listen(results.Add);
		    behavior.Send(2);
		    behavior.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9, 9, 2, 2, 7, 7), results);
	    }
        
        [Test]
	    public void TestValuesThenOnce() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
            var listener = behavior.Value().Once().Listen(results.Add);
		    behavior.Send(2);
		    behavior.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9), results);
	    }
        
        [Test]
	    public void TestValuesTwiceThenOnce() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
            var listener = DoubleUp(behavior.Value()).Once().Listen(results.Add);
		    behavior.Send(2);
		    behavior.Send(7);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(9), results);
	    }

        [Test]
	    public void TestValuesLateListen() 
        {
		    var behavior = new BehaviorSink<Int32>(9);
		    var results = new List<Int32>();
		    var value = behavior.Value();
		    behavior.Send(8);
		    var listener = value.Listen(results.Add);
		    behavior.Send(2);
		    listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(8, 2), results);
	    }
	
        [Test]
	    public void TestMapB() 
        {
		    var behavior = new BehaviorSink<Int32>(6);
		    var results = new List<String>();
		    var listener = behavior.Map(x => x.ToString(CultureInfo.InvariantCulture)).Value().Listen(results.Add);
		    behavior.Send(8);
		    listener.Unlisten();
		    AssertArraysEqual(Arrays<string>.AsList("6", "8"), results);
	    }

        [Test]
        public void TestMapB2()
        {
            var behavior = new BehaviorSink<Int32>(1);
            var behavior1 = behavior.Map(x => x * 3);
            var results = new List<Int32>();
            var listener = behavior1.Value().Listen(results.Add);
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(3), results);
        }

        [Test]
        public void TestMapB3()
        {
            var behavior = new BehaviorSink<Int32>(1);
            var behavior1 = behavior.Map(x => x * 3);
            var results = new List<Int32>();
            var listener = behavior1.Value().Listen(results.Add);
            behavior.Send(2);
            listener.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(3, 6), results);
        }
	
        [Test]
	    public void TestMapBLateListen() 
        {
		    var behavior = new BehaviorSink<Int32>(6);
		    var results = new List<String>();
		    var map = behavior.Map(x => x.ToString(CultureInfo.InvariantCulture));
		    behavior.Send(2);
		    var listener = map.Value().Listen(results.Add);
		    behavior.Send(8);
		    listener.Unlisten();
		    AssertArraysEqual(Arrays<string>.AsList("2", "8"), results);
	    }
	
        [Test]
	    public void TestApply() 
        {
		    var bf = new BehaviorSink<ILambda1<Int64, String>>(new Lambda1<Int64, String>(b => "1 "+b));
		    var ba = new BehaviorSink<Int64>(5L);
		    var results = new List<String>();
		    var listener = Behavior<Int64>.Apply(bf,ba)
                .Value().Listen(results.Add);
		    bf.Send(new Lambda1<Int64, String>(b => "12 "+b));
		    ba.Send(6L);
            listener.Unlisten();
            AssertArraysEqual(Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
	    }

        [Test]
	    public void TestLift() 
        {
		    var behavior1 = new BehaviorSink<Int32>(1);
		    var behavior2 = new BehaviorSink<Int64>(5L);
		    var results = new List<String>();
		    var combinedBehavior = Behavior<Int32>.Lift((x, y) => x + " " + y, behavior1, behavior2);
            var listener = combinedBehavior.Value().Listen(results.Add);
		    behavior1.Send(12);
		    behavior2.Send(6L);
            listener.Unlisten();
            AssertArraysEqual(Arrays<string>.AsList("1 5", "12 5", "12 6"), results);
	    }
	
        [Test]
	    public void TestMapAndLift() 
        {
		    var behavior = new BehaviorSink<Int32>(1);
		    var mappedBehavior1 = behavior.Map(x => x * 3);
		    var mappedBehavior2 = behavior.Map(x => x * 5);
            var results = new List<String>();
            var combinedBehavior = Behavior<Int32>.Lift((x, y) => x + " " + y, mappedBehavior1, mappedBehavior2);
		    var listener = combinedBehavior.Value().Listen(results.Add);
		    behavior.Send(2);
		    listener.Unlisten();
		    AssertArraysEqual(Arrays<string>.AsList("3 5", "6 10"), results);
	    }

        [Test]
	    public void TestHoldIsDelayed() 
        {
	        var e = new EventSink<Int32>();
	        var h = e.Hold(0);
	        var pair = e.Snapshot(h, (a, b) => a + " " + b);
		    var o = new List<String>();
		    var l = pair.Listen(o.Add);
		    e.Send(2);
		    e.Send(3);
		    l.Unlisten();
		    AssertArraysEqual(Arrays<string>.AsList("2 0", "3 2"), o);
	    }

        [Test]
	    public void TestSwitchB()
	    {
	        var sink = new EventSink<Sb>();
	        // Split each field o of SB so we can update multiple behaviours in a
	        // single transaction.
	        var behaviorA = sink.Map(s => s.C1).FilterNotNull().Hold('A');
	        var behaviorB = sink.Map(s => s.C2).FilterNotNull().Hold('a');
	        var bsw = sink.Map(s => s.Behavior).FilterNotNull().Hold(behaviorA);
	        var behavior = Behavior<char?>.SwitchB(bsw);
		    var results = new List<char>();
	        var listener = behavior.Value().Listen(c =>
	        {
	            Assert.IsNotNull(c, "c != null");
	            results.Add(c.Value);
	        });
	        sink.Send(new Sb('B','b',null));
	        sink.Send(new Sb('C','c',behaviorB));
	        sink.Send(new Sb('D','d',null));
	        sink.Send(new Sb('E','e',behaviorA));
	        sink.Send(new Sb('F','f',null));
	        sink.Send(new Sb(null,null,behaviorB));
	        sink.Send(new Sb(null,null,behaviorA));
	        sink.Send(new Sb('G','g',behaviorB));
	        sink.Send(new Sb('H','h',behaviorA));
	        sink.Send(new Sb('I','i',behaviorA));
	        listener.Unlisten();
	        AssertArraysEqual(Arrays<char>.AsList('A','B','c','d','E','F','f','F','g','H','I'), results);
	    }

        [Test]
        public void TestSwitchB1()
        {
            var sink = new EventSink<Sb>();
            // Split each field o of SB so we can update multiple behaviours in a
            // single transaction.
            var behaviorA = sink.Map(s => s.C1).FilterNotNull().Hold('A');
            var behaviorB = sink.Map(s => s.C2).FilterNotNull().Hold('a');
            var bsw = sink.Map(s => s.Behavior).FilterNotNull().Hold(behaviorA);
            var behavior = Behavior<char?>.SwitchB(bsw);
            var results = new List<char>();
            var listener = behavior.Value().Listen(c =>
            {
                Assert.IsNotNull(c, "c != null");
                results.Add(c.Value);
            });
            sink.Send(new Sb('B', 'b', null));
            listener.Unlisten();
            AssertArraysEqual(Arrays<char>.AsList('A', 'B'), results);
        }

        [Test]
        public void TestSwitchE()
        {
            var ese = new EventSink<Se>();
            var ea = ese.Map(s => s.C1).FilterNotNull();
            var eb = ese.Map(s => s.C2).FilterNotNull();
            var bsw = ese.Map(s => s.Event).FilterNotNull().Hold(ea);
            var o = new List<char>();
            var eo = Behavior<char>.SwitchE(bsw);
	        var l = eo.Listen(o.Add);
	        ese.Send(new Se('A','a',null));
	        ese.Send(new Se('B','b',null));
	        ese.Send(new Se('C','c',eb));
	        ese.Send(new Se('D','d',null));
	        ese.Send(new Se('E','e',ea));
	        ese.Send(new Se('F','f',null));
	        ese.Send(new Se('G','g',eb));
	        ese.Send(new Se('H','h',ea));
	        ese.Send(new Se('I','i',ea));
	        l.Unlisten();
	        AssertArraysEqual(Arrays<char>.AsList('A','B','C','d','e','F','G','h','I'), o);
        }

        [Test]
        public void TestLoopBehavior()
        {
            var ea = new EventSink<Int32>();
            var sum = new BehaviorLoop<Int32>();
            var sumOut = ea.Snapshot(sum, (x, y) => x+y).Hold(0);
            sum.Loop(sumOut);
            var o = new List<Int32>();
            var l = sumOut.Value().Listen(o.Add);
            ea.Send(2);
            ea.Send(3);
            ea.Send(1);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(0,2,5,6), o);
            Assert.AreEqual(6, sum.Sample());
        }

        [Test]
        public void TestCollect()
        {
            var ea = new EventSink<Int32>();
            var o = new List<Int32>();
            var sum = ea.Hold(100).Collect(0,
                //(a,s) -> new Tuple2(a+s, a+s)
                new Lambda2<Int32, Int32, Tuple2<Int32, Int32>>((a, s) => new Tuple2<Int32, Int32>(a + s, a + s))
            );
            var l = sum.Value().Listen(o.Add);
            ea.Send(5);
            ea.Send(7);
            ea.Send(1);
            ea.Send(2);
            ea.Send(3);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(100,105,112,113,115,118), o);
        }

        [Test]
        public void TestAccum()
        {
            var ea = new EventSink<Int32>();
            var o = new List<Int32>();
            var sum = ea.Accum(100, (a,s)=>a+s);
            var l = sum.Value().Listen(o.Add);
            ea.Send(5);
            ea.Send(7);
            ea.Send(1);
            ea.Send(2);
            ea.Send(3);
            l.Unlisten();
            AssertArraysEqual(Arrays<Int32>.AsList(100,105,112,113,115,118), o);
        }
        
        /// <summary>
        /// This is used for tests where Value() produces a single initial value on listen,
        /// and then we double that up by causing that single initial event to be repeated.
        /// This needs testing separately, because the code must be done carefully to achieve
        /// this.
        /// </summary>
        private static Event<Int32> DoubleUp(Event<Int32> ev)
        {
            return Event<Int32>.Merge(ev, ev);
        }

        class Se
	    {
	        public Se(char c1, char c2, Event<char> evt)
	        {
	            C1 = c1;
	            C2 = c2;
	            Event = evt;
	        }
	        public readonly char C1;
	        public readonly char C2;
            public readonly Event<char> Event;
	    }

        class Sb
	    {
            public Sb(char? c1, char? c2, Behavior<char?> behavior)
	        {
	            C1 = c1;
	            C2 = c2;
	            Behavior = behavior;
	        }
            public readonly char? C1;
            public readonly char? C2;
            public readonly Behavior<char?> Behavior;
	    }
    }
}