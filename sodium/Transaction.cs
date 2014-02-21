namespace sodium
{
    using System;
    using System.Collections.Generic;

    public class Transaction
    {
        // Coarse-grained lock that's held during the whole transaction.
        static readonly Object TransactionLock = new Object();
        // Fine-grained lock that protects listeners and nodes.
        internal static Object ListenersLock = new Object();

        // True if we need to re-generate the priority queue.
        internal bool ToRegen = false;

        private PriorityQueue<Entry> prioritizedQ = new PriorityQueue<Entry>();
        private ISet<Entry> entries = new HashSet<Entry>();

        private List<IRunnable> lastQ = new List<IRunnable>();
        private List<IRunnable> postQ;

        public Transaction()
        {
        }

        private static Transaction currentTransaction;

        ///
        /// Run the specified code inside a single transaction.
        ///
        /// In most cases this is not needed, because all APIs will create their own
        /// transaction automatically. It is useful where you want to run multiple
        /// reactive operations atomically.
        ///
        public static void run(IRunnable code)
        {
            lock (TransactionLock)
            {
                // If we are already inside a transaction (which must be on the same
                // thread otherwise we wouldn't have acquired transactionLock), then
                // keep using that same transaction.
                Transaction transWas = currentTransaction;
                try
                {
                    if (currentTransaction == null)
                        currentTransaction = new Transaction();
                    code.Run();
                }
                finally
                {
                    if (transWas == null)
                        currentTransaction.close();
                    currentTransaction = transWas;
                }
            }
        }

        internal static void run(IHandler<Transaction> code)
        {
            lock (TransactionLock)
            {
                // If we are already inside a transaction (which must be on the same
                // thread otherwise we wouldn't have acquired transactionLock), then
                // keep using that same transaction.
                Transaction transWas = currentTransaction;
                try
                {
                    if (currentTransaction == null)
                        currentTransaction = new Transaction();
                    code.run(currentTransaction);
                }
                finally
                {
                    if (transWas == null)
                        currentTransaction.close();
                    currentTransaction = transWas;
                }
            }
        }

        internal static A apply<A>(ILambda1<Transaction, A> code)
        {
            lock (TransactionLock)
            {
                // If we are already inside a transaction (which must be on the same
                // thread otherwise we wouldn't have acquired transactionLock), then
                // keep using that same transaction.
                Transaction transWas = currentTransaction;
                try
                {
                    if (currentTransaction == null)
                        currentTransaction = new Transaction();
                    return code.apply(currentTransaction);
                }
                finally
                {
                    currentTransaction.close();
                    currentTransaction = transWas;
                }
            }
        }

        public void prioritized(Node rank, IHandler<Transaction> action)
        {
            Entry e = new Entry(rank, action);
            prioritizedQ.Enqueue(e);
            entries.Add(e);
        }

        ///
        /// Add an action to run after all prioritized() actions.
        ///
        public void last(IRunnable action)
        {
            lastQ.Add(action);
        }

        ///
        /// Add an action to run after all last() actions.
        ///
        public void post(IRunnable action)
        {
            if (postQ == null)
                postQ = new List<IRunnable>();
            postQ.Add(action);
        }

        ///
        /// If the priority queue has entries in it when we modify any of the nodes'
        /// ranks, then we need to re-generate it to make sure it's up-to-date.
        ///
        private void checkRegen()
        {
            if (ToRegen)
            {
                ToRegen = false;
                prioritizedQ.clear();
                foreach (Entry e in entries)
                    prioritizedQ.Enqueue(e);
            }
        }

        public void close()
        {
            while (true)
            {
                checkRegen();
                if (prioritizedQ.isEmpty()) break;
                Entry e = prioritizedQ.Dequeue();
                entries.Remove(e);
                e.Action.run(this);
            }
            foreach (IRunnable action in lastQ)
                action.Run();
            lastQ.Clear();
            if (postQ != null)
            {
                foreach (IRunnable action in postQ)
                    action.Run();
                postQ.Clear();
            }
        }
    }
}