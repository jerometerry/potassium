namespace Sodium
{
    using System;
    using System.Collections.Generic;

    public sealed class Rank : IComparable<Rank>
    {
        public static readonly Rank Null = new Rank(long.MaxValue);
        private readonly ISet<Rank> listeners = new HashSet<Rank>();
        private long priority;

        public Rank() 
            : this(0L)
        {
        }

        private Rank(long priority)
        {
            this.priority = priority;
        }

        /// <summary>
        /// </summary>
        /// <returns>true if any changes were made. </returns>
        public bool LinkTo(Rank target)
        {
            if (target == Null)
            { 
                return false;
            }

            var set = new HashSet<Rank>();
            var changed = target.EnsureHigherPriorityThan(this.priority, set);
            listeners.Add(target);
            set.Clear();

            return changed;
        }

        public void UnlinkTo(Rank target)
        {
            if (target == Null)
            { 
                return;
            }

            listeners.Remove(target);
        }

        public int CompareTo(Rank n)
        {
            return this.priority.CompareTo(n.priority);
        }

        private bool EnsureHigherPriorityThan(long limit, ISet<Rank> visited)
        {
            if (this.priority > limit || visited.Contains(this))
            { 
                return false;
            }

            visited.Add(this);
            this.priority = limit + 1;
            foreach (var l in listeners)
            { 
                l.EnsureHigherPriorityThan(this.priority, visited);
            }

            return true;
        }
    }
}