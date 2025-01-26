using Itmo.Dev.Platform.Events;

namespace BookingService.Application.Contracts.Bookings.Events;

public record BookingSubmissionEvent(long BookingId) : IEvent;