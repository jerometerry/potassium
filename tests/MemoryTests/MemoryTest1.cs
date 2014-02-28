namespace Sodium.MemoryTests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest1
    {
        [TestCase(50)]
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var evt = new Event<int?>();
            evt.Description = "Root event that will be fired on";

            var behavior = evt.Hold(0);
            behavior.Description = "Behavior that gets updates from the root event";

            // TODO - etens isn't being used. Seems like it should be
            var etens = evt.Map(x => x / 10);
            etens.Description = "Root event mapped to values divided by ten";

            var snapshotEvent = evt.Snapshot(behavior, (neu, old) => neu.Equals(old) ? null : neu);
            snapshotEvent.Description = "Event<int?> that fires the snapshot function when the root event is fired. " + 
                "Neu is the value fired on the root event, old is the value of the behavior at the time of the firing";

            var changeTens = snapshotEvent.FilterNotNull();
            changeTens.Description = "Event<int?> that filters out null values from the snapshot event, giving only the events when the tens changes";

            var eventOfBehaviors = changeTens.Map(tens => behavior.Map(tt => new Tuple<int?, int?>(tens, tt)));
            eventOfBehaviors.Description = "Event<Behaviors<Tuple<int?,int?>>, fired when the tens changes";

            var tensTupleWrappedBehavior = eventOfBehaviors.Hold(behavior.Map(tt => new Tuple<int?, int?>(0, tt)));
            tensTupleWrappedBehavior.Description = "Behavior<Behaviors<Tuple<int?,int?>>";

            var tensTupleBehavior = Behavior<Tuple<int?, int?>>.SwitchB(tensTupleWrappedBehavior);
            tensTupleBehavior.Description = "Behavior<Tuple<int?, int?>>";

            var tensTupleEvent = tensTupleBehavior.Value();
            tensTupleEvent.Description = "Event<Tuple<int?,int?>>";

            var listener = tensTupleEvent.Listen(tu => { });
            listener.Description = "Listener for tens changes";

            var i = 0;

            while (i < iterations)
            {
                evt.Fire(i);
                i++;
            }

            Metrics.AutoDispose();
            Assert.AreEqual(0, Metrics.LiveItemCount);
        }
    }
}