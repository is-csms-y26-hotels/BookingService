using BookingService.Application.Contracts.Bookings.Operations;

namespace BookingService.Application.Contracts.Bookings;

public interface IReservationService
{
    public Task<CreateBooking.Result> CreateBookingAsync(
        CreateBooking.Request request,
        CancellationToken cancellationToken);

    public Task<PostponeBooking.Result> PostponeBookingAsync(
        PostponeBooking.Request request,
        CancellationToken cancellationToken);

    public Task<SubmitBooking.Result> SubmitBookingAsync(
        SubmitBooking.Request request,
        CancellationToken cancellationToken);

    public Task<CompleteBooking.Result> CompleteBookingAsync(
        CompleteBooking.Request request,
        CancellationToken cancellationToken);

    public Task<CancelBooking.Result> CancelBookingAsync(
        CancelBooking.Request request,
        CancellationToken cancellationToken);

    public Task<GetRoomAvailableDateRanges.Result> GetRoomAvailableDateRangesAsync(
        GetRoomAvailableDateRanges.Request request,
        CancellationToken cancellationToken);
}