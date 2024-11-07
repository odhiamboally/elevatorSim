namespace ES.Application.Abstractions.Messaging;
public interface IMessageConsumer
{
    Task ConsumeAsync<T>(Func<T, Task> handler) where T : class;
}
