namespace JT.Rx.Net
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