namespace sodium
{
    using System;

    public interface IListener : IDisposable
    {
        void Unlisten();
        IListener Append(IListener listener);
    }
}
