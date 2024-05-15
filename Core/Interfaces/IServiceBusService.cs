namespace Core.Interfaces;

public interface IServiceBusService
{
    Task SendDataAsync(IEnumerable<string> data, string topic);
}