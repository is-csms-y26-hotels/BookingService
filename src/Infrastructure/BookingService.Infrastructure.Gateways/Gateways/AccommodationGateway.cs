using BookingService.Application.Abstractions.Gateways;
using BookingService.Application.Abstractions.Gateways.Models.Accommodation;

namespace BookingService.Infrastructure.Gateways.Gateways;

public class AccommodationGateway : IAccommodationGateway
{
    public Task<bool> RoomExistsAsync(RoomInfoModel model, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}