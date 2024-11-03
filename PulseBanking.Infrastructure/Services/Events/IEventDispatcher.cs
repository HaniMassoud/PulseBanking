
using System.Threading.Tasks;

namespace PulseBanking.Infrastructure.Services.Events;

public interface IEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent @event) where TEvent : class;
}

