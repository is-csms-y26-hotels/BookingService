using BookingService.Application.Abstractions.Gateways.Models.Accommodation;

namespace BookingService.Application.Abstractions.Gateways;

public interface IAccommodationGateway
{
    public Task<bool> RoomExistsAsync(RoomInfoModel model, CancellationToken cancellationToken);
}