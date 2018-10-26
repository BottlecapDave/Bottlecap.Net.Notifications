namespace Bottlecap.Net.Notifications
{
    public interface INotificationContext<TData> : INotificationContext
        where TData : class
    {
        new TData Content { get; }
    }
}
