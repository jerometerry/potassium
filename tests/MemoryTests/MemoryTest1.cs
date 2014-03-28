namespace Potassium.MemoryTests
{
    using System;
    using System.Collections.Generic;
    using Potassium.Core;
    using Potassium.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest1
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var finalizers = new List<IDisposable>();

            var behaviorMapFinalizers = new List<IDisposable>();

            var evt = new FirableEvent<int?>();
            finalizers.Add(evt);

            var behavior = evt.Hold(0);
            finalizers.Add(behavior);

            // TODO - etens isn't being used. Seems like it should be
            var etens = evt.Map(x => x / 10);
            finalizers.Add(etens);

            var snapshotEvent = evt.Snapshot((neu, old) => neu.Equals(old) ? null : neu, behavior);
            finalizers.Add(snapshotEvent);

            var changeTens = snapshotEvent.Where(x => x != null);
            finalizers.Add(changeTens);

            var eventOfBehaviors = changeTens.Map(tens =>
            {
                DisposeFinalizers(behaviorMapFinalizers);
                var mapped = behavior.Map(tt => new Tuple<int?, int?>(tens, tt));
                behaviorMapFinalizers.Add(mapped);
                return mapped;
            });
            finalizers.Add(eventOfBehaviors);

            var behaviorMap = behavior.Map(tt => new Tuple<int?, int?>(0, tt));
            finalizers.Add(behaviorMap);

            var tensTupleWrappedBehavior = eventOfBehaviors.Hold(behaviorMap);
            finalizers.Add(tensTupleWrappedBehavior);

            var tensTupleBehavior = tensTupleWrappedBehavior.Switch();
            finalizers.Add(tensTupleBehavior);

            var listener = tensTupleBehavior.SubscribeWithInitialFire(tu => { });
            var i = 0;

            while (i < iterations)
            {
                evt.Fire(i);
                i++;
            }

            listener.Dispose();

            DisposeFinalizers(finalizers);
            DisposeFinalizers(behaviorMapFinalizers);
        }

        private static void DisposeFinalizers(List<IDisposable> items)
        {
            foreach (var item in items)
            {
                item.Dispose();
            }

            items.Clear();
        }
    }
}