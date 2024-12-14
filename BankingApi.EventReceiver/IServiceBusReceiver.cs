namespace BankingApi.EventReceiver
{
    public interface IServiceBusReceiver
    {
        //Used to manually test the queue
        Task AddMessageToQueue(EventMessage message);

        Task<EventMessage?> Peek();

        Task Abandon(EventMessage message);
        
        Task Complete(EventMessage message);
        Task ReSchedule(EventMessage message, DateTime nextAvailableTime);
        Task MoveToDeadLetter(EventMessage message);
    }
}
