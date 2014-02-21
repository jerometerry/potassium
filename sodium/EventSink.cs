namespace sodium
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class EventSink<A> : Event<A> {
        public EventSink() {}

	    public void send(A a) {
		    Transaction.Run(new HandlerImpl<Transaction>(t => send(t, a)));
	    }

        internal void send(Transaction trans, A a) {
            if (!Firings.Any())
                trans.Last(new Runnable(() => Firings.Clear()));
            Firings.Add(a);
        
		    List<ITransactionHandler<A>> listeners = new List<ITransactionHandler<A>>(this.Actions);
    	    foreach (ITransactionHandler<A> action in listeners) {
    		    try {
                    action.Run(trans, a);
    		    }
    		    catch (Exception t) {
    		        System.Diagnostics.Debug.WriteLine("{0}", t);
    		    }
    	    }
        }
    }
}