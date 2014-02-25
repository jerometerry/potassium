namespace Sodium.MemoryTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest3
    {
        [Test]
        public void Test()
        {
            Event<int> et = new Event<int>();
            Behavior<int> t = et.Hold(0);
            Event<int> evt = new Event<int>();
            Behavior<Behavior<int>> oout = evt.Map(x => t).Hold(t);
            Behavior<int> o = Behavior<int>.SwitchB(oout);
            IListener l = o.Value().Listen(tt =>
            {
                //System.out.println(tt)
            });
            int i = 0;
            while (i < 1000000000)
            {
                evt.Fire(i);
                i++;
            }

            l.Stop();
        }
    }
}