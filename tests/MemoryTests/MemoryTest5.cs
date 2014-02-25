namespace Sodium.MemoryTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest5
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

            EventSink<int> eChange = new EventSink<int>();
            Behavior<int> o = eChange.Hold(0);
            IListener l = o.Value().Listen(tt =>
            {
                //System.out.println(tt)
            });
            int i = 0;
            while (i < 1000000000)
            {
                eChange.Send(i);
                i++;
            }

            l.Unlisten();
        }
    }
}