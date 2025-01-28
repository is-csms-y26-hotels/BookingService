using Accommodation.Service.Presentation.Grpc;
using BookingService.Application.Abstractions.Gateways;
using BookingService.Application.Models.ObjectValues;

namespace BookingService.Infrastructure.Gateways.Gateways;

public class AccommodationGateway(
    RoomService.RoomServiceClient client) : IAccommodationGateway
{
    public async Task<bool> RoomExistsAsync(RoomId roomId, CancellationToken cancellationToken)
    {
        var request = new ValidateRoomRequest { RoomId = roomId.Value, };

        ValidateRoomResponse? response = await client.ValidateRoomAsync(
            request,
            cancellationToken: cancellationToken);

        return response.Result;
    }
}