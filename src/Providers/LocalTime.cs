namespace Potassium.Providers
{
    using System;
    
    /// <summary>
    /// LocalTime is an IProvider that provides the current local time
    /// </summary>
    /// <remarks>DateTime.Now is evaluated when the value of the LocalTime is requested.</remarks>
    public class LocalTime : Time
    {
        /// <summary>
        /// Returns the current local time
        /// </summary>
        public override DateTime Value
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}