namespace Sodium
{
    using System;

    /// <summary>
    /// 
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