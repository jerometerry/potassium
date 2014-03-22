namespace Potassium.Providers
{
    using System;
    
    /// <summary>
    /// Time is a Monad that lazily returns a new DateTime
    /// </summary>
    public abstract class Time : Provider<DateTime>
    {
    }
}
