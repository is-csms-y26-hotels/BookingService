using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Abstractions.Gateways;

public interface IAccommodationGateway
{
    public Task<bool> RoomExistsAsync(RoomId roomId, CancellationToken cancellationToken);
}