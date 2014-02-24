namespace Sodium
{
    using System;
    using System.Collections.Generic;

    internal sealed class Transaction : IDisposable
    {
        /// <summary>
        /// Fine-grained lock that protects listeners and nodes. 
        /// </summary>
        internal static readonly object ListenersLock = new object();

        /// <summary>
        /// Coarse-grained lock that's held during the whole transaction. 
        /// </summary>
        internal static readonly object TransactionLock = new object();

        private readonly PriorityQueue<Entry> prioritized = new PriorityQueue<Entry>();
        private readonly List<Action> last = new List<Action>();
        private readonly List<Action> post = new List<Action>();

        /// <summary>
        /// True if we need to re-generate the priority queue.
        /// </summary>
        private bool nodeRanksModified;

        private bool disposed;

        /// <summary>
        /// Run the specified function inside a single transaction
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <remarks>
        /// In most cases this is not needed, because all APIs will create their own
        /// transaction automatically. It is useful where you want to run multiple
        /// reactive operations atomically.
        /// </remarks>
        public static TA Apply<TA>(Func<Transaction, TA> code)
        {
            lock (TransactionLock)
            {
                using (var context = new TransactionContext())
                {
                    context.Open();
                    return code(context.Transaction);
                }
            }
        }

        /// <summary>
        /// Run the specified code inside a single transaction.
        /// </summary>
        /// <param name="code"></param>
        /// <remarks>
        /// In most cases this is not needed, because all APIs will create their own
        /// transaction automatically. It is useful where you want to run multiple
        /// reactive operations atomically.
        /// </remarks>
        public static void Run(Action<Transaction> code)
        {
            lock (TransactionLock)
            {
                using (var context = new TransactionContext())
                {
                    context.Open();
                    code(context.Transaction);
                }
            }
        }

        /// <summary>
        /// Add an action to run before all Last() and Post() actions. Actions are prioritized by node rank.
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="action"></param>
        public void Prioritized(Node rank, Action<Transaction> action)
        {
            prioritized.Add(new Entry(rank, action));
        }

        /// <summary>
        /// Add an action to run after all Prioritized() actions.
        /// </summary>
        /// <param name="action"></param>
        public void Last(Action action)
        {
            last.Add(action);
        }

        /// <summary>
        /// Add an action to run after all last() actions.
        /// </summary>
        /// <param name="action"></param>
        public void Post(Action action)
        {
            post.Add(action);
        }

        public void LinkNodes(Node node, Node target)
        {
            if (node.LinkTo(target))
            {
                nodeRanksModified = true;
            }
        }

        public void Close()
        {
            ClosePrioritizedActions();
            CloseLastActions();
            ClosePostActions();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                this.Close();
                this.disposed = true;
            }
        }

        private void ClosePrioritizedActions()
        {
            while (true)
            {
                // If the priority queue has entries in it when we modify any of the nodes'
                // ranks, then we need to re-generate it to make sure it's up-to-date.
                if (nodeRanksModified)
                {
                    nodeRanksModified = false;
                    prioritized.Regenerate();
                }

                if (prioritized.IsEmpty())
                {
                    break;
                }

                prioritized.Remove().Action(this);
            }
        }

        private void CloseLastActions()
        {
            foreach (var action in last)
            {
                action();
            }

            last.Clear();
        }

        private void ClosePostActions()
        {
            foreach (var action in post)
            {
                action();
            }

            post.Clear();
        }
    }
}