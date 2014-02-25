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
            Event<int?> et = new Event<int?>();
            Behavior<int?> t = et.Hold(0);
            Event<int?> etens = et.Map(x => x / 10);
            Event<int?> changeTens = et.Snapshot(t, (neu, old) => neu.Equals(old) ? null : neu).FilterNotNull();
            Behavior<Behavior<Tuple<int?, int?>>> oout =
                changeTens.Map(tens => t.Map(tt => new Tuple<int?, int?>(tens, tt)))
                .Hold(t.Map(tt => new Tuple<int?, int?>(0, tt)));
            Behavior<Tuple<int?, int?>> o = Behavior<Tuple<int?, int?>>.SwitchB(oout);
            IListener l = o.Value().Listen(tu =>
            {
                //System.out.println(tu.a+","+tu.b);
            });
            int i = 0;
            while (i < 1000000000)
            {
                et.Fire(i);
                i++;
            }

            l.Stop();
        }
    }
}