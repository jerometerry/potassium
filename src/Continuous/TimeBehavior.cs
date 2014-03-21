namespace JT.Rx.Net.Continuous
{
    using System;
    using JT.Rx.Net.Core;

    /// <summary>
    /// TimeBehavior is the base class for continuous Behaviors of DateTimes.
    /// </summary>
    public abstract class TimeBehavior : Monad<DateTime>
    {
    }
}
