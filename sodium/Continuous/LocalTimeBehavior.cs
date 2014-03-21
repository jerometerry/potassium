namespace Sodium.Continuous
{
    using System;

    /// <summary>
    /// LocalTimeBehavior is a continuous Behavior that contains the current Local System Time
    /// </summary>
    public class LocalTimeBehavior : TimeBehavior
    {
        public override DateTime Value
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}