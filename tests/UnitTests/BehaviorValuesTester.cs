namespace Potassium.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NUnit.Framework;
    using Potassium.Core;
    using Potassium.Providers;

    [TestFixture]
    public class BehaviorValuesTester : TestBase
    {
        [Test]
        public void TestValues()
        {
            var publisher = new FirableEvent<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var listener = behavior.SubscribeWithInitialFire(results.Add);
            publisher.Fire(2);
            publisher.Fire(7);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(9, 2, 7), results);
        }

         [Test]
         public void TestValuesThenMap()
         {
             var publisher = new FirableEvent<int>();
             var behavior = new Behavior<int>(9, publisher);
             var results = new List<int>();
             var l = behavior.ToEventWithInitialFire().Map(x => x + 100).Subscribe(results.Add);
             publisher.Fire(2);
             publisher.Fire(7);
             l.Dispose();
             behavior.Dispose();
             AssertArraysEqual(Arrays<int>.AsList(109, 102, 107), results);
         }
 
         [Test]
         public void TestValuesTwiceThenMap()
         {
             var publisher = new FirableEvent<int>();
             var behavior = new Behavior<int>(9, publisher);
             var results = new List<int>();
             var listener = DoubleUp(behavior.ToEventWithInitialFire()).Map(x => x + 100).Subscribe(results.Add);
             publisher.Fire(2);
             publisher.Fire(7);
             listener.Dispose();
             behavior.Dispose();
             AssertArraysEqual(Arrays<int>.AsList(109, 109, 102, 102, 107, 107), results);
         }

         [Test]
         public void TestValuesThenCoalesce()
         {
             var publisher = new FirableEvent<int>();
             var behavior = new Behavior<int>(9, publisher);
             var results = new List<int>();
             var listener = behavior.ToEventWithInitialFire().Coalesce((fst, snd) => snd).Subscribe(results.Add);
             publisher.Fire(2);
             publisher.Fire(7);
             listener.Dispose();
             behavior.Dispose();
             AssertArraysEqual(Arrays<int>.AsList(9, 2, 7), results);
         }
 
         [Test]
         public void TestValuesTwiceThenCoalesce()
         {
             var publisher = new FirableEvent<int>();
             var behavior = new Behavior<int>(9, publisher);
             var results = new List<int>();
             var listener = DoubleUp(behavior.ToEventWithInitialFire()).Coalesce((fst, snd) => fst + snd).Subscribe(results.Add);
             publisher.Fire(2);
             publisher.Fire(7);
             listener.Dispose();
             behavior.Dispose();
             AssertArraysEqual(Arrays<int>.AsList(18, 4, 14), results);
         }
 
         [Test]
         public void TestValuesThenSnapshot()
         {
             var publisherInt32 = new FirableEvent<int>();
             var behaviorInt32 = new Behavior<int>(9, publisherInt32);
             var publisherChar = new FirableEvent<char>();
             var behaviorChar = new Behavior<char>('a', publisherChar);
             var results = new List<char>();
             var listener = behaviorInt32.ToEventWithInitialFire().Snapshot(behaviorChar).Subscribe(results.Add);
             publisherChar.Fire('b');
             publisherInt32.Fire(2);
             publisherChar.Fire('c');
             publisherInt32.Fire(7);
             listener.Dispose();
             behaviorInt32.Dispose();
             behaviorChar.Dispose();
             AssertArraysEqual(Arrays<char>.AsList('a', 'b', 'c'), results);
         }
 
         [Test]
         public void TestValuesTwiceThenSnapshot()
         {
             var publisherInt32 = new FirableEvent<int>();
             var behaviorInt32 = new Behavior<int>(9, publisherInt32);
             var publisherChar = new FirableEvent<char>();
             var behaviorChar = new Behavior<char>('a', publisherChar);
             var results = new List<char>();
             var listener = DoubleUp(behaviorInt32.ToEventWithInitialFire()).Snapshot(behaviorChar).Subscribe(results.Add);
             publisherChar.Fire('b');
             publisherInt32.Fire(2);
             publisherChar.Fire('c');
             publisherInt32.Fire(7);
             listener.Dispose();
             behaviorInt32.Dispose();
             behaviorChar.Dispose();
             AssertArraysEqual(Arrays<char>.AsList('a', 'a', 'b', 'b', 'c', 'c'), results);
         }
 
         [Test]
         public void TestValuesThenMerge()
         {
             var publisher1 = new FirableEvent<int>();
             var behavior1 = new Behavior<int>(9, publisher1);
             var publisher2 = new FirableEvent<int>();
             var behavior2 = new Behavior<int>(2, publisher2);
             var results = new List<int>();
             var listener = behavior1.ToEventWithInitialFire().Merge(behavior2.ToEventWithInitialFire()).Coalesce((x, y) => x + y).Subscribe(results.Add);
             publisher1.Fire(1);
             publisher2.Fire(4);
             listener.Dispose();
             behavior1.Dispose();
             behavior2.Dispose();
             AssertArraysEqual(Arrays<int>.AsList(11, 1, 4), results);
         }
 
         [Test]
         public void TestValuesThenFilter()
         {
             var publisher = new FirableEvent<int>();
             var behavior = new Behavior<int>(9, publisher);
             var results = new List<int>();
             var listener = behavior.ToEventWithInitialFire().Filter(a => true).Subscribe(results.Add);
             publisher.Fire(2);
             publisher.Fire(7);
             listener.Dispose();
             behavior.Dispose();
             AssertArraysEqual(Arrays<int>.AsList(9, 2, 7), results);
         }
 
         [Test]
         public void TestValuesTwiceThenFilter()
         {
             var publisher = new FirableEvent<int>();
             var behavior = new Behavior<int>(9, publisher);
             var results = new List<int>();
             var listener = DoubleUp(behavior.ToEventWithInitialFire()).Filter(a => true).Subscribe(results.Add);
             publisher.Fire(2);
             publisher.Fire(7);
             listener.Dispose();
             behavior.Dispose();
             AssertArraysEqual(Arrays<int>.AsList(9, 9, 2, 2, 7, 7), results);
         }
 
         [Test]
         public void TestValuesThenOnce()
         {
             var publisher = new FirableEvent<int>();
             var behavior = new Behavior<int>(9, publisher);
             var results = new List<int>();
             var listener = behavior.ToEventWithInitialFire().Once().Subscribe(results.Add);
             publisher.Fire(2);
             publisher.Fire(7);
             listener.Dispose();
             behavior.Dispose();
             AssertArraysEqual(Arrays<int>.AsList(9), results);
         }
 
         [Test]
         public void TestValuesTwiceThenOnce()
         {
             var publisher = new FirableEvent<int>();
             var behavior = new Behavior<int>(9, publisher);
             var results = new List<int>();
             var listener = DoubleUp(behavior.ToEventWithInitialFire()).Once().Subscribe(results.Add);
             publisher.Fire(2);
             publisher.Fire(7);
             listener.Dispose();
             behavior.Dispose();
             AssertArraysEqual(Arrays<int>.AsList(9), results);
         }
 
         [Test]
         public void TestValuesLateListen()
         {
             var publisher = new FirableEvent<int>();
             var behavior = new Behavior<int>(9, publisher);
             var results = new List<int>();
             var value = behavior.ToEventWithInitialFire();
             publisher.Fire(8);
             var listener = value.Subscribe(results.Add);
             publisher.Fire(2);
             listener.Dispose();
             behavior.Dispose();
             AssertArraysEqual(Arrays<int>.AsList(8, 2), results);
         }
 
         public Event<T> DoubleUp<T>(Event<T> e)
         {
             return e | e;
         }
    }
}
