namespace Sodium
{
    using System;

    /// <summary>
    /// 
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