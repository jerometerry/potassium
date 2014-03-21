namespace JT.Rx.Net.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A Transaction is used to order a stream of actions. Events that occur in the same Transaction
    /// are known as Simultaneous Events.
    /// </summary>
    /// <remarks>
    /// Actions are run in the following order when the transaction is ran
    /// 
    ///     1. High priority actions
    ///     2. Medium priority actions
    ///     3. Low priority actions
    /// 
    /// High priority actions are ordered by Rank using a Priority Queue. Medium 
    /// and Low priority actions are run in the order they are added.
    /// </remarks>
    public sealed class Transaction : Disposable
    {
        private PriorityQueue<PrioritizedAction> high;
        private List<Action> medium;
        private List<Action> low;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Transaction()
        {
            this.high = new PriorityQueue<PrioritizedAction>();
            this.medium = new List<Action>();
            this.low = new List<Action>();
        }

        /// <summary>
        /// If the Rank of any action is modified, set Reprioritize to true to re-prioritize
        /// the PriorityQueue before running the Prioritized Actions.
        /// </summary>
        public bool Reprioritize { get; set; }

        /// <summary>
        /// Run the given Function using a Transaction obtained from TransactionContext.Current
        /// </summary>
        /// <typeparam name="TR">The return type of the Function</typeparam>
        /// <param name="f">The Function to run</param>
        /// <returns>The result of the Function</returns>
        public static TR Start<TR>(Func<Transaction, TR> f)
        {
            return TransactionContext.Current.Run(f);
        }

        /// <summary>
        /// Add an action to run before all Medium() and Low() actions. Actions are prioritized by rank.
        /// </summary> 
        /// <param name="action">The action to schedule high</param>
        /// <param name="rank">The rank of the action</param>
        public void High(Action<Transaction> action, Priority rank)
        {
            this.high.Add(new PrioritizedAction(action, rank));
        }

        /// <summary>
        /// Add an action to run after all High() actions, and before all Low() actions
        /// </summary>
        /// <param name="action">The action to schedule medium</param>
        public void Medium(Action action)
        {
            this.medium.Add(action);
        }

        /// <summary>
        /// Add an action to run after all High() and Medium() actions.
        /// </summary>
        /// <param name="action">The action to schedule low</param>
        public void Low(Action action)
        {
            this.low.Add(action);
        }

        /// <summary>
        /// Close the current Transaction, running all scheduled actions, in the order High, Medium, Low.
        /// </summary>
        public void Close()
        {
            this.RunHigh();
            this.high.Clear();

            this.RunMedium();
            this.medium.Clear();

            this.RunLow();
            this.low.Clear();
        }

        /// <summary>
        /// Clean up the current Transaction, purging any actions without publishing
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            this.high.Clear();
            this.medium.Clear();
            this.low.Clear();

            this.high = null;
            this.medium = null;
            this.low = null;

            base.Dispose(disposing);
        }

        private void RunHigh()
        {
            while (true)
            {
                // If the priority queue has entries in it when we modify any of the nodes'
                // ranks, then we need to re-generate it to make sure it's up-to-date.
                if (this.Reprioritize)
                {
                    this.Reprioritize = false;
                    this.high.Reprioritize();
                }

                if (this.high.IsEmpty())
                {
                    break;
                }

                var e = this.high.Remove();
                e.Action(this);
            }
        }

        private void RunMedium()
        {
            foreach (var action in this.medium)
            {
                action();
            }
        }

        private void RunLow()
        {
            foreach (var action in this.low)
            {
                action();
            }
        }
    }
}