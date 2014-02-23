namespace Sodium.Tests
{
    using System;
    using System.Threading;

    public class MemoryTest1
    {
        public static void main(string[] args)
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

            EventSink<int?> et = new EventSink<int?>();
            Behavior<int?> t = et.Hold(0);
            Event<int?> etens = et.Map(x => x / 10);
            Event<int?> changeTens = et.Snapshot(t, (neu, old) =>
                neu.Equals(old) ? null : neu).FilterNotNull();
            Behavior<Behavior<Tuple2<int?, int?>>> oout =
                changeTens.Map(tens => t.Map(tt => new Tuple2<int?, int?>(tens, tt)))
                .Hold(t.Map(tt => new Tuple2<int?, int?>(0, tt)));
            Behavior<Tuple2<int?, int?>> o = Behavior<Tuple2<int?, int?>>.SwitchB(oout);
            IListener l = o.Value().Listen(tu =>
            {
                //System.out.println(tu.a+","+tu.b);
            });
            int i = 0;
            while (i < 1000000000)
            {
                et.Send(i);
                i++;
            }

            l.Unlisten();
        }
    }
}