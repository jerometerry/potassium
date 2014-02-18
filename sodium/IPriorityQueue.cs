namespace sodium
{
    using System.Collections.Generic;

    public interface IPriorityQueue<T>
    {
        void Add(T item);

        void AddRange(IEnumerable<T> items);

        void Clear();
        bool Remove(T item);
        T Remove();
        bool IsEmpty();
    }
}