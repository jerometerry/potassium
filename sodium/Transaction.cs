namespace Sodium
{
    using System;
    using System.Collections.Generic;

    internal sealed class Transaction
    {
        /// <summary>
        /// Fine-grained lock that protects listeners and nodes. 
        /// </summary>
        internal static readonly object ListenersLock = new object();

        /// <summary>
        /// Coarse-grained lock that's held during the whole transaction. 
        /// </summary>
        private static readonly object TransactionLock = new object();

        private static Transaction current;
        
        private readonly PriorityQueue<Entry> prioritized = new PriorityQueue<Entry>();
        private readonly ISet<Entry> entries = new HashSet<Entry>();
        private readonly List<IRunnable> last = new List<IRunnable>();
        private readonly List<IRunnable> post = new List<IRunnable>();

        /// <summary>
        /// True if we need to re-generate the priority queue.
        /// </summary>
        private bool toRegen = false;

        internal bool ToRegen
        {
            get { return toRegen; }
            set { toRegen = value; }
        }

        /// <summary>
        /// Run the specified code inside a single transaction.
        ///
        /// In most cases this is not needed, because all APIs will create their own
        /// transaction automatically. It is useful where you want to run multiple
        /// reactive operations atomically.
        /// </summary>
        public static void Run(IRunnable code)
        {
            Run(new Handler<Transaction>((t) => code.Run()));
        }

        /// <summary>
        /// Overload of Prioritized to support C# lambdas
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="action"></param>
        public void Prioritized(Node rank, Action<Transaction> action)
        {
            Prioritized(rank, new Handler<Transaction>(action));
        }

        public void Prioritized(Node rank, IHandler<Transaction> action)
        {
            var e = new Entry(rank, action);
            prioritized.Add(e);
            entries.Add(e);
        }

        /// <summary>
        /// Overload of Last to support C# lambdas
        /// </summary>
        /// <param name="action"></param>
        public void Last(Action action)
        {
            last.Add(new Runnable(action));
        }

        /// <summary>
        /// Add an action to run after all prioritized() actions.
        /// </summary>
        public void Last(IRunnable action)
        {
            last.Add(action);
        }

        /// <summary>
        /// Overload of the Post method to support C# lambdas
        /// </summary>
        /// <param name="action"></param>
        public void Post(Action action)
        {
            Post(new Runnable(action));
        }

        /// <summary>
        /// Add an action to run after all last() actions.
        /// </summary>
        public void Post(IRunnable action)
        {
            post.Add(action);
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

        internal static A Apply<A>(ILambda1<Transaction, A> code)
        {
            lock (TransactionLock)
            {
                // If we are already inside a transaction (which must be on the same
                // thread otherwise we wouldn't have acquired transactionLock), then
                // keep using that same transaction.
                Transaction transWas = current;
                try
                {
                    if (current == null)
                    {
                        current = new Transaction();
                    }

                    return code.Apply(current);
                }
                finally
                {
                    current.Close();
                    current = transWas;
                }
            }
        }

        /// <summary>
        /// Overload of Run to support C# lambdas
        /// </summary>
        /// <param name="code"></param>
        internal static void Run(Action<Transaction> code)
        {
            Run(new Handler<Transaction>(code));
        }

        internal static void Run(IHandler<Transaction> code)
        {
            lock (TransactionLock)
            {
                // If we are already inside a transaction (which must be on the same
                // thread otherwise we wouldn't have acquired transactionLock), then
                // keep using that same transaction.
                Transaction previous = current;
                try
                {
                    if (current == null)
                    {
                        current = new Transaction();
                    }

                    code.Run(current);
                }
                finally
                {
                    if (previous == null)
                    {
                        current.Close();
                    }

                    current = previous;
                }
            }
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
    }
}