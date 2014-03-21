namespace JT.Rx.Net.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Priority is used to prioritize actions. A Priority has a value, along with a set of
    /// superior Priority. The class invariant is that all superiors must have a higher value
    /// than their subordinates.
    /// </summary>
    public sealed class Priority : IComparable<Priority>
    {
        /// <summary>
        /// The Highest Priority possible.
        /// </summary>
        public static readonly Priority Max = new Priority(long.MaxValue);
        
        private readonly ISet<Priority> superiors = new HashSet<Priority>();
        private long value;

        /// <summary>
        /// Constructs a new Priority, of value zero.
        /// </summary>
        public Priority() 
            : this(0L)
        {
        }

        private Priority(long value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the priority of the current Priority
        /// </summary>
        public long Value
        {
            get { return value; }
        }

        /// <summary>
        /// Compare two Priorities, by comparing their values.
        /// </summary>
        /// <param name="priority">The Priority to compare against the current Priority</param>
        /// <returns>Negative value if the current priority is less than the given priority,
        /// positive value if the current priority is greater than the given priority, zero
        /// if the priorities are equal.</returns>
        public int CompareTo(Priority priority)
        {
            return this.value.CompareTo(priority.value);
        }

        /// <summary>
        /// Add the given superior priority to the list of superiors for the current priority. As a result,
        /// the superiors priority will be increased until it outranks the current priority. The superiors
        /// superiors will have their ranks increased recursively.
        /// </summary>
        /// <param name="superior">Superior to add to the current Priority</param>
        /// <returns>True if any Priority priorities where changed.</returns>
        public bool AddSuperior(Priority superior)
        {
            if (superior == Max)
            { 
                return false;
            }

            var visited = new HashSet<Priority>();
            var changed = superior.Outrank(this, visited);
            this.superiors.Add(superior);
            visited.Clear();
            return changed;
        }

        /// <summary>
        /// Remove the given priority from the list of superiors of the current Priority
        /// </summary>
        /// <param name="superior">Priority to be removed as a superior of the current Priority</param>
        /// <returns>True if the superior was removed, false otherwise</returns>
        public bool RemoveSuperior(Priority superior)
        {
            if (superior == Max)
            { 
                return false;
            }

            return this.superiors.Remove(superior);
        }

        /// <summary>
        /// Determine if the current Priority has a higher priority than the given Priority
        /// </summary>
        /// <param name="priority">The Priority to compare priority to</param>
        /// <returns>True if the current Eank outranks the given Priority, false otherwise</returns>
        public bool Outranks(Priority priority)
        {
            return this.value > priority.value;
        }

        /// <summary>
        /// Ensure the current priority outranks the given priority
        /// </summary>
        /// <param name="priority">The priority that needs to be outranked</param>
        /// <param name="visited">Set of Ranks, used to break out if a cycle is detected</param>
        /// <returns>True if the current priority was increased, false otherwise</returns>
        private bool Outrank(Priority priority, ISet<Priority> visited)
        {
            if (this.Outranks(priority) || visited.Contains(this))
            { 
                return false;
            }

            visited.Add(this);
            this.value = priority.value + 1;

            foreach (var superior in this.superiors)
            {
                superior.Outrank(this, visited);
            }

            return true;
        }
    }
}