using BookingService.Application.Abstractions.Gateways;
using BookingService.Application.Models.ObjectValues;

namespace BookingService.Infrastructure.Gateways.Gateways;

public class AccommodationGateway : IAccommodationGateway
{
    public Task<bool> RoomExistsAsync(RoomId roomId, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}