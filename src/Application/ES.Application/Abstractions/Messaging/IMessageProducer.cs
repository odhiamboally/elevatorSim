namespace ES.Application.Abstractions.Messaging;
public interface IMessageProducer
{
    Task ProduceAsync<T>(T message) where T : class;
}
