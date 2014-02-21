namespace sodium
{

    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class EventLoop<TA> : Event<TA>
    {
        private Event<TA> _eaOut;

        // TODO: Copy & paste from EventSink. Can we improve this?
        private void Send(Transaction trans, TA a)
        {
            if (!Firings.Any())
                trans.Last(new Runnable(() => Firings.Clear()));
            Firings.Add(a);

            var listeners = new List<ITransactionHandler<TA>>(Actions);
            foreach (var action in listeners)
            {
                try
                {
                    action.Run(trans, a);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("{0}", ex);
                }
            }
        }

        public void Loop(Event<TA> eaOut)
        {
            if (_eaOut != null)
                throw new ApplicationException("EventLoop looped more than once");
            _eaOut = eaOut;
            var me = this;
            RegisterListener(eaOut.Listen(Node, new TransactionHandler<TA>(me.Send)));
        }
    }

}