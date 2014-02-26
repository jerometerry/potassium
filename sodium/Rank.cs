namespace Sodium
{
    using System;
    using System.Collections.Generic;

    public sealed class Rank : IComparable<Rank>
    {
        public static readonly Rank Highest = new Rank(long.MaxValue);
        private readonly ISet<Rank> superiors = new HashSet<Rank>();
        private long priority;

        public Rank() 
            : this(0L)
        {
        }

        private Rank(long priority)
        {
            this.priority = priority;
        }

        public int CompareTo(Rank rank)
        {
            return this.priority.CompareTo(rank.priority);
        }

        /// <summary>
        /// Add the given superior rank to the list of superiors for the current rank. As a result,
        /// the superiors rank will be increased unti it outranks the current rank. The superiors
        /// superiors will have their ranks increased recursively.
        /// </summary>
        /// <returns>True if any Rank priorities where changed.</returns>
        public bool AddSuperior(Rank superior)
        {
            if (superior == Highest)
            { 
                return false;
            }

            var visited = new HashSet<Rank>();
            var changed = superior.Outrank(this, visited);
            this.superiors.Add(superior);
            visited.Clear();
            return changed;
        }

        public bool RemoveSuperior(Rank superior)
        {
            if (superior == Highest)
            { 
                return false;
            }

            return this.superiors.Remove(superior);
        }

        /// <summary>
        /// Determine if the current Rank has a higher priority than the given Rank
        /// </summary>
        /// <param name="rank">The Rank to compare priority to</param>
        /// <returns>True if the current Eank outranks the given Rank, false otherwise</returns>
        public bool Outranks(Rank rank)
        {
            return this.priority > rank.priority;
        }

        /// <summary>
        /// Ensure the current rank outranks the given rank
        /// </summary>
        /// <param name="rank">The rank that needs to be outranked</param>
        /// <param name="visited">Set of Ranks, used to break out if a cycle is detected</param>
        /// <returns>True if the current rank was increased, false otherwise</returns>
        private bool Outrank(Rank rank, ISet<Rank> visited)
        {
            if (this.Outranks(rank) || visited.Contains(this))
            { 
                return false;
            }

            visited.Add(this);
            this.priority = rank.priority + 1;

            foreach (var superior in this.superiors)
            {
                superior.Outrank(this, visited);
            }

            return true;
        }
    }
}