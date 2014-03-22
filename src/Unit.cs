namespace JT.Rx.Net
{
    /// <summary>
    /// Unit is an empty class to represent no value
    /// </summary>
    public sealed class Unit
    {
        /// <summary>
        /// A value that represents nothing
        /// </summary>
        public static readonly Unit Nothing = new Unit();

        private Unit() 
        { 
        }
    }
}