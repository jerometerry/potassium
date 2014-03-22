namespace JT.Rx.Net.Monads
{
    using System;
    
    /// <summary>
    /// Time is a Monad that lazily returns a new DateTime
    /// </summary>
    public abstract class Time : Monad<DateTime>
    {
    }
}
