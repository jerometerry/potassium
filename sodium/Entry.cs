namespace Sodium
{
    using System;

    internal sealed class Entry : IComparable<Entry>
    {
        private static long nextSequence;

        private readonly Action<Transaction> action;
        private readonly Node rank;
        private readonly long sequence;

        public Entry(Node rank, Action<Transaction> action)
        {
            this.rank = rank;
            this.action = action;
            this.sequence = nextSequence++;
        }

        public Action<Transaction> Action
        {
            get
            {
                return action;
            }
        }

        public int CompareTo(Entry e)
        {
            var answer = this.rank.CompareTo(e.rank);
            if (answer == 0)
            {  // Same rank: preserve chronological sequence.
                if (this.sequence < e.sequence) 
                {
                    answer = -1;
                }
                else
                {
                    if (this.sequence > e.sequence) 
                    {
                        answer = 1;
                    }
                }
            }

            return answer;
        }
    }
}