namespace Sodium.MemoryTests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest1
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var evt = new Event<int?>();
            var behavior = evt.Hold(0);

            // TODO - etens isn't being used. Seems like it should be
            var etens = evt.Map(x => x / 10);

            var changeTens = evt
                .Snapshot(behavior, (neu, old) => neu.Equals(old) ? null : neu)
                .FilterNotNull();
            
            var tensTupleWrappedBehavior = changeTens
                .Map(tens => behavior.Map(tt => new Tuple<int?, int?>(tens, tt)))
                .Hold(behavior.Map(tt => new Tuple<int?, int?>(0, tt)));
            
            var tensTupleBehavior = Behavior<Tuple<int?, int?>>.SwitchB(tensTupleWrappedBehavior);
            
            var listener = tensTupleBehavior.Value().Listen(tu => { });
            
            var i = 0;

            while (i < iterations)
            {
                evt.Fire(i);
                i++;
            }

            listener.Dispose();
        }
    }
}