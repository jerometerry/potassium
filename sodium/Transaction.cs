namespace Sodium
{
    using System;
    using System.Collections.Generic;

    internal sealed class Transaction
    {
        /// <summary>
        /// Coarse-grained lock that's held during the whole transaction. 
        /// </summary>
        private static readonly Object TransactionLock = new Object();

        /// <summary>
        /// Fine-grained lock that protects listeners and nodes. 
        /// </summary>
        internal static readonly Object ListenersLock = new Object();

        /// <summary>
        /// True if we need to re-generate the priority queue.
        /// </summary>
        private bool toRegen = false;

        internal bool ToRegen
        {
            get { return toRegen; }
            set { toRegen = value; }
        }

        private readonly PriorityQueue<Entry> prioritized = new PriorityQueue<Entry>();
        private readonly ISet<Entry> entries = new HashSet<Entry>();
        private readonly List<IRunnable> last = new List<IRunnable>();
        private readonly List<IRunnable> post = new List<IRunnable>();

        private static Transaction _currentTransaction;

        /// <summary>
        /// Run the specified code inside a single transaction.
        ///
        /// In most cases this is not needed, because all APIs will create their own
        /// transaction automatically. It is useful where you want to run multiple
        /// reactive operations atomically.
        /// </summary>
        public static void Run(IRunnable code)
        {
            lock (TransactionLock)
            {
                // If we are already inside a transaction (which must be on the same
                // thread otherwise we wouldn't have acquired transactionLock), then
                // keep using that same transaction.
                Transaction transWas = _currentTransaction;
                try
                {
                    if (_currentTransaction == null)
                    { 
                        _currentTransaction = new Transaction();
                    }

                    code.Run();
                }
                finally
                {
                    if (transWas == null)
                    { 
                        _currentTransaction.Close();
                    }

                    _currentTransaction = transWas;
                }
            }
        }

        internal static void Run(IHandler<Transaction> code)
        {
            lock (TransactionLock)
            {
                // If we are already inside a transaction (which must be on the same
                // thread otherwise we wouldn't have acquired transactionLock), then
                // keep using that same transaction.
                Transaction transWas = _currentTransaction;
                try
                {
                    if (_currentTransaction == null)
                    { 
                        _currentTransaction = new Transaction();
                    }

                    code.Run(_currentTransaction);
                }
                finally
                {
                    if (transWas == null)
                    { 
                        _currentTransaction.Close();
                    }

                    _currentTransaction = transWas;
                }
            }
        }

        internal static A Apply<A>(ILambda1<Transaction, A> code)
        {
            lock (TransactionLock)
            {
                // If we are already inside a transaction (which must be on the same
                // thread otherwise we wouldn't have acquired transactionLock), then
                // keep using that same transaction.
                Transaction transWas = _currentTransaction;
                try
                {
                    if (_currentTransaction == null)
                    { 
                        _currentTransaction = new Transaction();
                    }

                    return code.Apply(_currentTransaction);
                }
                finally
                {
                    _currentTransaction.Close();
                    _currentTransaction = transWas;
                }
            }
        }

        public void Prioritized(Node rank, IHandler<Transaction> action)
        {
            var e = new Entry(rank, action);
            prioritized.Add(e);
            entries.Add(e);
        }

        /// <summary>
        /// Add an action to run after all prioritized() actions.
        /// </summary>
        public void Last(IRunnable action)
        {
            last.Add(action);
        }

        /// <summary>
        /// Add an action to run after all last() actions.
        /// </summary>
        public void Post(IRunnable action)
        {
            post.Add(action);
        }

        /// <summary>
        /// If the priority queue has entries in it when we modify any of the nodes'
        /// ranks, then we need to re-generate it to make sure it's up-to-date.
        /// </summary>
        private void CheckRegen()
        {
            if (ToRegen)
            {
                ToRegen = false;
                prioritized.Clear();
                foreach (var e in entries)
                { 
                    prioritized.Add(e);
                }
            }
        }

        public void Close()
        {
            while (true)
            {
                CheckRegen();
                if (prioritized.IsEmpty())
                { 
                    break;
                }

                var e = prioritized.Remove();
                entries.Remove(e);
                e.Action.Run(this);
            }

            foreach (var action in last)
            { 
                action.Run();
            }

            last.Clear();
            
            foreach (var action in post)
            { 
                action.Run();
            }

            post.Clear();   
        }
    }
}