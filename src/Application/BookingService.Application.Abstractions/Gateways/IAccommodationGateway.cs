using BookingService.Application.Abstractions.Gateways.Models.Accommodation;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Abstractions.Gateways;

public interface IAccommodationGateway
{
    public Task<bool> RoomExistsAsync(RoomInfoModel model, CancellationToken cancellationToken);
}