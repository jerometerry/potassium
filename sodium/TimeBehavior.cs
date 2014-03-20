namespace Sodium
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public abstract class TimeBehavior : DisposableObject, IBehavior<DateTime>
    {
        public abstract DateTime Value { get; }
    }
}
