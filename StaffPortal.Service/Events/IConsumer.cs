namespace StaffPortal.Service.Events
{
    public interface IConsumer<T>
    {
        void HandleEvent(T eventMessage);
    }
}
