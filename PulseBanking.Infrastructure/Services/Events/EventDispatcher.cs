using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseBanking.Infrastructure.Services.Events;
public class EventDispatcher : IEventDispatcher
{
    private readonly ILogger<EventDispatcher> _logger;
    private readonly IPublisher _mediator;

    public EventDispatcher(
        ILogger<EventDispatcher> logger,
        IPublisher mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task DispatchAsync<TEvent>(TEvent @event) where TEvent : class
    {
        try
        {
            await _mediator.Publish(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching event {EventType}", typeof(TEvent).Name);
            throw;
        }
    }
}