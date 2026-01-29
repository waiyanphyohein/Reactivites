using System;
using MediatR;
using Domain;
using Persistence;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Application.Events.Commands;

public class EditEvent
{
    public class Command(Event Event) : IRequest<Event>
    {
        public Event Event { get; } = Event;
    }

    public class Handler(AppDbContext context, IMapper mapper, ILogger<Handler> logger) : IRequestHandler<Command, Event>
    {
        private readonly AppDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<Handler> _logger = logger;

        public async Task<Event> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var eventEntity = await _context.Events.FindAsync(new object[] { request.Event.EventId }, cancellationToken);
                
                if (eventEntity == null)
                {
                    _logger.LogWarning("Event with ID {EventId} not found for update", request.Event.EventId);
                    throw new KeyNotFoundException("Event not found");
                }

                // Update only the fields that are provided (not null)
                _mapper.Map(request.Event, eventEntity);

                _logger.LogInformation("Event with ID {EventId} updated successfully: {EventName}", eventEntity.EventId, eventEntity.EventName);
                
                await _context.SaveChangesAsync(cancellationToken);

                return eventEntity;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Request timed out while updating event with ID {EventId}", request.Event.EventId);
                throw new HttpRequestException("Request timed out", null, System.Net.HttpStatusCode.RequestTimeout);
            }
            catch (KeyNotFoundException)
            {
                throw; // Rethrow to be handled by the controller
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating event with ID {EventId}", request.Event.EventId);
                throw new HttpRequestException("An error occurred while updating the event", ex, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}

