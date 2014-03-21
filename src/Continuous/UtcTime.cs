namespace JT.Rx.Net.Continuous
{
    using System;
    
    public class UtcTime : Time
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