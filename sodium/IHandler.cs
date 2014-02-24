namespace Sodium
{
    internal interface IHandler<in TA>
    {
        void Run(Transaction t, TA a);
    }
}