namespace Sodium.MemoryTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest1
    {
        [Test]
        public void Test()
        {
            var evt = new Event<int?>();
            
            // A new event that simply divides by ten
            var etens = evt.Map(x => x / 10);

            var behavior = evt.Hold(0);

            // Event that is fired whenever the tens value changes
            var changeTens = evt
                .Snapshot(behavior, (neu, old) => neu.Equals(old) ? null : neu)
                .FilterNotNull();

            var tensTupleWrappedBehavior = changeTens
                .Map(tens => behavior.Map(tt => new Tuple<int?, int?>(tens, tt)))
                .Hold(behavior.Map(tt => new Tuple<int?, int?>(0, tt)));

            // A behavior that updates whenver evt is fired that generates a new tens value.
            // Each firing is a Tuple<int?,int?> containing value/10 along with the value.
            var tensTupleBehavior = Behavior<Tuple<int?, int?>>.SwitchB(tensTupleWrappedBehavior);

            var listener = tensTupleBehavior.Value().Listen(tu => { });
            
            int i = 0;
            
            while (i < 1000000000)
            {
                evt.Fire(i);
                i++;
            }

            listener.Stop();
        }
    }
}