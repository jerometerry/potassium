namespace Sodium
{
    using System;

    internal sealed class PrioritizedAction : IComparable<PrioritizedAction>
    {
        private static long nextSequence;

        private readonly Action<Transaction> action;
        private readonly Priority rank;
        private readonly long sequence;

        public PrioritizedAction(Action<Transaction> action, Priority rank)
        {
            this.action = action;
            this.rank = rank;
            this.sequence = nextSequence++;
        }

        public Action<Transaction> Action
        {
            get
            {
                return action;
            }
        }

        public int CompareTo(PrioritizedAction e)
        {
            var answer = this.rank.CompareTo(e.rank);
            if (answer != 0)
            {
                return answer;
            }

            // Same rank: preserve chronological sequence.
            return this.sequence.CompareTo(e.sequence);
        }
    }
}