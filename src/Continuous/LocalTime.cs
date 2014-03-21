namespace JT.Rx.Net.Continuous
{
    using System;
    
    public class LocalTime : Time
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