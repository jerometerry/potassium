namespace Sodium.MemoryTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest4
    {
        [Test]
        public void Test()
        {
            EventSink<int> et = new EventSink<int>();
            EventSink<int> evt = new EventSink<int>();
            Behavior<Event<int>> oout = evt.Map(x => (Event<int>)et).Hold((Event<int>)et);
            Event<int> o = Behavior<int>.SwitchE(oout);
            IListener l = o.Listen(tt =>
            {
                Console.WriteLine("{0}", tt);
            });
            int i = 0;
            while (i < 1000000000)
            {
                evt.Send(i);
                i++;
            }

            l.Stop();
        }
    }
}