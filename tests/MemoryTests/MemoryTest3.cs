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
            //new Thread() {
            //    public void run()
            //    {
            //        try {
            //            while (true) {
            //                System.out.println("memory "+Runtime.getRuntime().totalMemory());
            //                Thread.sleep(5000);
            //            }
            //        }
            //        catch (InterruptedException e) {
            //            System.out.println(e.toString());
            //        }
            //    }
            //}.start();

            EventSink<int> et = new EventSink<int>();
            Behavior<int> t = et.Hold(0);
            EventSink<int> evt = new EventSink<int>();
            Behavior<Behavior<int>> oout = evt.Map(x => t).Hold(t);
            Behavior<int> o = Behavior<int>.SwitchB(oout);
            IListener l = o.Value().Listen(tt =>
            {
                //System.out.println(tt)
            });
            int i = 0;
            while (i < 1000000000)
            {
                evt.Send(i);
                i++;
            }

            l.Unlisten();
        }
    }
}