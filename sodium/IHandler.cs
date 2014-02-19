namespace sodium
{
    using System;
    public interface IHandler<in T> : IDisposable
    {
        void Run(T p);
    }
}