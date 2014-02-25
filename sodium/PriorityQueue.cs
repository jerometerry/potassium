namespace Sodium
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// PriorityQueue is adapted from the article Priority Queues with C# by James McCaffrey.
    /// http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx?m=1
    /// </summary>
    /// <remarks>PriorityQueue is implemented using a binary heap. 
    /// http://en.wikipedia.org/wiki/Binary_heap
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public sealed class PriorityQueue<T> where T : IComparable<T>
    {
        private readonly List<T> data = new List<T>();

        public void Regenerate()
        {
            var entries = new List<T>(data);
            Clear();
            foreach (var e in entries)
            {
                Add(e);
            }
        }

        public void Add(T item)
        {
            data.Add(item);
            var childIndex = data.Count - 1; // child index; start at end
            
            while (childIndex > 0)
            {
                var parentIndex = (childIndex - 1) / 2; // parent index
                if (data[childIndex].CompareTo(data[parentIndex]) >= 0)
                { 
                    break; // child item is larger than (or equal) parent so we're done
                }

                Swap(childIndex, parentIndex);
                childIndex = parentIndex;
            }
        }

        public T Remove()
        {
            // Assumes pq is not empty; up to calling code
            var lastIndex = data.Count - 1; // last index (before removal)
            
            // Get the item at the front
            var frontItem = data[0];   
            
            // Replace the front item with the last item
            data[0] = data[lastIndex];
            data.RemoveAt(lastIndex);

            --lastIndex; // last index (after removal)
            var parentIndex = 0; // parent index. start at front of pq
            while (true)
            {
                var childIndex = (parentIndex * 2) + 1; // left child index of parent
                if (childIndex > lastIndex)
                { 
                    break;  // no children so done
                }

                var rightChild = childIndex + 1; 
                if (rightChild <= lastIndex && data[rightChild].CompareTo(data[childIndex]) < 0) 
                {
                    // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                    childIndex = rightChild;
                }

                if (data[parentIndex].CompareTo(data[childIndex]) <= 0)
                { 
                    break; // parent is smaller than (or equal to) smallest child so done
                }

                Swap(childIndex, parentIndex);
                parentIndex = childIndex;
            }

            return frontItem;
        }

        public T Peek()
        {
            var frontItem = data[0];
            return frontItem;
        }

        public void Clear()
        {
            data.Clear();
        }

        public int Count()
        {
            return data.Count;
        }

        public bool IsEmpty()
        {
            return Count() == 0;
        }

        /// <summary>
        /// Determine if the heap property true for all data
        /// </summary>
        /// <returns>If the heap property is true for all data, false otherwise</returns>
        public bool IsConsistent()
        {
            if (data.Count == 0)
            { 
                return true;
            }

            var lastIndex = data.Count - 1;
            for (var parentIndex = 0; parentIndex < data.Count; ++parentIndex)
            {
                var leftChildIndex = (2 * parentIndex) + 1;
                var rightChildIndex = (2 * parentIndex) + 2;

                if (leftChildIndex <= lastIndex && data[parentIndex].CompareTo(data[leftChildIndex]) > 0)
                { 
                    return false; // if lc exists and it's greater than parent then bad.
                }

                if (rightChildIndex <= lastIndex && data[parentIndex].CompareTo(data[rightChildIndex]) > 0)
                { 
                    return false; // check the right child too.
                }
            }

            return true;
        }

        private void Swap(int index1, int index2)
        {
            T item1 = data[index1];
            T item2 = data[index2];
            data[index1] = item2;
            data[index2] = item1;
        }
    }
}