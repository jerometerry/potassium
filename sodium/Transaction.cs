namespace Sodium
{
    using System;
    using System.Collections.Generic;

    internal sealed class Transaction
    {
        private readonly PriorityQueue<PrioritizedAction> prioritized = new PriorityQueue<PrioritizedAction>();
        
        private readonly List<Action> last = new List<Action>();
        
        private readonly List<Action> post = new List<Action>();

        /// <summary>
        /// True if we need to re-generate the priority queue.
        /// </summary>
        public bool NodeRanksModified { get; set; }

        /// <summary>
        /// Run the specified function inside a single transaction
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <param name="f">Function that accepts a transaction and returns a value</param>
        /// <returns></returns>
        /// <remarks>
        /// In most cases this is not needed, because all APIs will create their own
        /// transaction automatically. It is useful where you want to run multiple
        /// reactive operations atomically.
        /// </remarks>
        public static TA Run<TA>(Func<Transaction, TA> f)
        {
            lock (Constants.TransactionLock)
            {
                var context = new TransactionContext();
                try
                {
                    context.Open();
                    return f(context.Transaction);
                }
                finally
                {
                    context.Close();
                }
            }
        }

        /// <summary>
        /// Add an action to run before all Last() and Post() actions. Actions are prioritized by rank.
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="action"></param>
        public void Prioritize(Action<Transaction> action, Rank rank)
        {
            prioritized.Add(new PrioritizedAction(action, rank));
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

        public void Close()
        {
            this.RunPrioritizedActions();
            this.RunLastActions();
            this.RunPostActions();
        }

        private void RunPrioritizedActions()
        {
            while (true)
            {
                // If the priority queue has entries in it when we modify any of the nodes'
                // ranks, then we need to re-generate it to make sure it's up-to-date.
                if (this.NodeRanksModified)
                {
                    this.NodeRanksModified = false;
                    prioritized.Regenerate();
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

            last.Clear();
        }

        private void RunPostActions()
        {
            foreach (var action in post)
            {
                action();
            }

            post.Clear();
        }
    }
}