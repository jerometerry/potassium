namespace Potassium.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Potassium.Core;
    using Potassium.Extensions;
    using Potassium.Providers;
    using NUnit.Framework;

    [TestFixture]
    public class BehaviorValuesTester : TestBase
    {
        [Test]
        public void TestValues()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var listener = behavior.Values().Subscribe(results.Add);
            publisher.Publish(2);
            publisher.Publish(7);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(9, 2, 7), results);
        }

        [Test]
        public void TestValuesThenMap()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var l = behavior.Values().Map(x => x + 100).Subscribe(results.Add);
            publisher.Publish(2);
            publisher.Publish(7);
            l.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(109, 102, 107), results);
        }

        [Test]
        public void TestValuesTwiceThenMap()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var listener = DoubleUp(behavior.Values()).Map(x => x + 100).Subscribe(results.Add);
            publisher.Publish(2);
            publisher.Publish(7);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(109, 109, 102, 102, 107, 107), results);
        }

        [Test]
        public void TestValuesThenCoalesce()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var listener = behavior.Values().Coalesce((fst, snd) => snd).Subscribe(results.Add);
            publisher.Publish(2);
            publisher.Publish(7);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(9, 2, 7), results);
        }

        [Test]
        public void TestValuesTwiceThenCoalesce()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var listener = DoubleUp(behavior.Values()).Coalesce((fst, snd) => fst + snd).Subscribe(results.Add);
            publisher.Publish(2);
            publisher.Publish(7);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(18, 4, 14), results);
        }

        [Test]
        public void TestValuesThenSnapshot()
        {
            var publisherInt32 = new EventPublisher<int>();
            var behaviorInt32 = new Behavior<int>(9, publisherInt32);
            var publisherChar = new EventPublisher<char>();
            var behaviorChar = new Behavior<char>('a', publisherChar);
            var results = new List<char>();
            var listener = behaviorInt32.Values().Snapshot(behaviorChar).Subscribe(results.Add);
            publisherChar.Publish('b');
            publisherInt32.Publish(2);
            publisherChar.Publish('c');
            publisherInt32.Publish(7);
            listener.Dispose();
            behaviorInt32.Dispose();
            behaviorChar.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('a', 'b', 'c'), results);
        }

        [Test]
        public void TestValuesTwiceThenSnapshot()
        {
            var publisherInt32 = new EventPublisher<int>();
            var behaviorInt32 = new Behavior<int>(9, publisherInt32);
            var publisherChar = new EventPublisher<char>();
            var behaviorChar = new Behavior<char>('a', publisherChar);
            var results = new List<char>();
            var listener = DoubleUp(behaviorInt32.Values()).Snapshot(behaviorChar).Subscribe(results.Add);
            publisherChar.Publish('b');
            publisherInt32.Publish(2);
            publisherChar.Publish('c');
            publisherInt32.Publish(7);
            listener.Dispose();
            behaviorInt32.Dispose();
            behaviorChar.Dispose();
            AssertArraysEqual(Arrays<char>.AsList('a', 'a', 'b', 'b', 'c', 'c'), results);
        }

        [Test]
        public void TestValuesThenMerge()
        {
            var publisher1 = new EventPublisher<int>();
            var behavior1 = new Behavior<int>(9, publisher1);
            var publisher2 = new EventPublisher<int>();
            var behavior2 = new Behavior<int>(2, publisher2);
            var results = new List<int>();
            var listener = behavior1.Values().Merge(behavior2.Values(), (x, y) => x + y).Subscribe(results.Add);
            publisher1.Publish(1);
            publisher2.Publish(4);
            listener.Dispose();
            behavior1.Dispose();
            behavior2.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(11, 1, 4), results);
        }

        [Test]
        public void TestValuesThenFilter()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var listener = behavior.Values().Filter(a => true).Subscribe(results.Add);
            publisher.Publish(2);
            publisher.Publish(7);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(9, 2, 7), results);
        }

        [Test]
        public void TestValuesTwiceThenFilter()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var listener = DoubleUp(behavior.Values()).Filter(a => true).Subscribe(results.Add);
            publisher.Publish(2);
            publisher.Publish(7);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(9, 9, 2, 2, 7, 7), results);
        }

        [Test]
        public void TestValuesThenOnce()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var listener = behavior.Values().Once().Subscribe(results.Add);
            publisher.Publish(2);
            publisher.Publish(7);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(9), results);
        }

        [Test]
        public void TestValuesTwiceThenOnce()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var listener = DoubleUp(behavior.Values()).Once().Subscribe(results.Add);
            publisher.Publish(2);
            publisher.Publish(7);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(9), results);
        }

        [Test]
        public void TestValuesLateListen()
        {
            var publisher = new EventPublisher<int>();
            var behavior = new Behavior<int>(9, publisher);
            var results = new List<int>();
            var value = behavior.Values();
            publisher.Publish(8);
            var listener = value.Subscribe(results.Add);
            publisher.Publish(2);
            listener.Dispose();
            behavior.Dispose();
            AssertArraysEqual(Arrays<int>.AsList(8, 2), results);
        }

        public Event<T> DoubleUp<T>(Event<T> e)
        {
            return e.Merge(e);
        }

    }
}
