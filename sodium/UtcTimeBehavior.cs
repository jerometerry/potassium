namespace Sodium
{
    using System;

    /// <summary>
    /// UtcTimeBehavior is a continuous Behavior that contains the current UTC Time
    /// </summary>
    public class UtcTimeBehavior : TimeBehavior
    {
        public override DateTime Value
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}