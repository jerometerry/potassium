namespace sodium
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class EventSink<TA> : Event<TA>
    {
        public void Send(TA a)
        {
            Transaction.Run(new Handler<Transaction>(t => Send(t, a)));
        }

        internal void Send(Transaction trans, TA a)
        {
            if (!Firings.Any())
                trans.Last(new Runnable(() => Firings.Clear()));
            Firings.Add(a);

            var listeners = new List<ITransactionHandler<TA>>(this.Actions);
            foreach (var action in listeners)
            {
                try
                {
                    action.Run(trans, a);
                }
                catch (Exception t)
                {
                    System.Diagnostics.Debug.WriteLine("{0}", t);
                }
            }
        }
    }
}