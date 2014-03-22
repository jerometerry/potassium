namespace JT.Rx.Net.Monads
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