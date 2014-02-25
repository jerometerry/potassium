namespace Sodium
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A Transaction is used to order a stream of actions
    /// </summary>
    /// <remarks>
    /// Transactions are run in the following order when the transaction is committed
    /// 
    ///     1. Prioritized actions
    ///     2. Last actions
    ///     3. Post actions
    /// 
    /// Prioritized actions are ordered by Rank using a Priority Queue 
    /// </remarks>
    public sealed class Transaction : IDisposable
    {
        private PriorityQueue<PrioritizedAction> prioritized;
        private List<Action> last;
        private List<Action> post;
        private bool disposed;

        public Transaction()
        {
            prioritized = new PriorityQueue<PrioritizedAction>();
            last = new List<Action>();
            post = new List<Action>();
        }

        /// <summary>
        /// If the Rank of any action is modified, set Reprioritize to true to reprioritize
        /// the PriorityQueue before running the Prioritized Actions.
        /// </summary>
        public bool Reprioritize { get; set; }

        /// <summary>
        /// Add an action to run before all Last() and Post() actions. Actions are prioritized by rank.
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="action"></param>
        public void Prioritize(Action<Transaction> action, Rank rank)
        {
            this.AssertNotDisposed();
            prioritized.Add(new PrioritizedAction(action, rank));
        }

        /// <summary>
        /// Add an action to run after all Prioritized() actions.
        /// </summary>
        /// <param name="action"></param>
        public void Last(Action action)
        {
            this.AssertNotDisposed();
            last.Add(action);
        }

        /// <summary>
        /// Add an action to run after all last() actions.
        /// </summary>
        /// <param name="action"></param>
        public void Post(Action action)
        {
            this.AssertNotDisposed();
            post.Add(action);
        }

        /// <summary>
        /// Run all scheduled actions, in the correct order
        /// </summary>
        public void Commit()
        {
            this.RunPrioritizedActions();
            this.prioritized.Clear();

            this.RunLastActions();
            this.last.Clear();

            this.RunPostActions();
            this.post.Clear();
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            Commit();

            prioritized = null;
            last = null;
            post = null;

            this.disposed = true;
        }

        private void RunPrioritizedActions()
        {
            while (true)
            {
                // If the priority queue has entries in it when we modify any of the nodes'
                // ranks, then we need to re-generate it to make sure it's up-to-date.
                if (this.Reprioritize)
                {
                    this.Reprioritize = false;
                    prioritized.Reprioritize();
                }

                if (prioritized.IsEmpty())
                {
                    break;
                }

                var e = prioritized.Remove();
                e.Action(this);
            }
        }

        private void RunLastActions()
        {
            foreach (var action in last)
            {
                action();
            }
        }

        private void RunPostActions()
        {
            foreach (var action in post)
            {
                action();
            }
        }

        private void AssertNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("Transaction used after being disposed");
            }
        }
    }
}