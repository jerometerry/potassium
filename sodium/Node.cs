namespace Sodium
{
    using System;
    using System.Collections.Generic;

    public sealed class Node : IComparable<Node>
    {
        public readonly static Node Null = new Node(long.MaxValue);
        private long rank;
        private readonly ISet<Node> listeners = new HashSet<Node>();

        public Node() 
            : this(0L)
        {
        }

        private Node(long rank)
        {
            this.rank = rank;
        }

        /// <summary>
        /// </summary>
        /// <returns>true if any changes were made. </returns>
        public bool LinkTo(Node target)
        {
            if (target == Null)
            { 
                return false;
            }

            bool changed = target.EnsureBiggerThan(rank, new HashSet<Node>());
            listeners.Add(target);
            return changed;
        }

        public void UnlinkTo(Node target)
        {
            if (target == Null)
            { 
                return;
            }

            listeners.Remove(target);
        }

        private bool EnsureBiggerThan(long limit, ISet<Node> visited)
        {
            if (rank > limit || visited.Contains(this))
            { 
                return false;
            }

            visited.Add(this);
            rank = limit + 1;
            foreach (var l in listeners)
            { 
                l.EnsureBiggerThan(rank, visited);
            }

            return true;
        }

        public int CompareTo(Node n)
        {
            return this.rank.CompareTo(n.rank);
        }
    }
}