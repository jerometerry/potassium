namespace Sodium
{
    using System;

    internal sealed class PrioritizedAction : IComparable<PrioritizedAction>
    {
        private static long nextSequence;

        private readonly Action<ActionScheduler> action;
        private readonly Rank rank;
        private readonly long sequence;

        public PrioritizedAction(Action<ActionScheduler> action, Rank rank)
        {
            this.action = action;
            this.rank = rank;
            this.sequence = nextSequence++;
        }

        public Action<ActionScheduler> Action
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