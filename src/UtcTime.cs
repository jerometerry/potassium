namespace JT.Rx.Net
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