using Bookings.BookingsService.Contracts;
using BookingService.Application.Contracts.Bookings;
using BookingService.Application.Contracts.Bookings.Operations;
using BookingService.Application.Models.ObjectValues;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Diagnostics;

namespace BookingService.Presentation.Grpc.Controllers;

public class BookingController(
    IReservationService reservationService) : Bookings.BookingsService.Contracts.BookingService.BookingServiceBase
{
    public override async Task<CreateBookingResponse> CreateBooking(
        CreateBookingRequest request,
        ServerCallContext context)
    {
        var bookingRequest = new CreateBooking.Request(
            new UserEmail(request.UserEmail),
            new HotelId(request.HotelId),
            new RoomId(request.RoomId),
            request.CheckInDate.ToDateTimeOffset(),
            request.CheckOutDate.ToDateTimeOffset());

        CreateBooking.Result result = await reservationService.CreateBookingAsync(
            bookingRequest,
            context.CancellationToken);

        return result switch
        {
            CreateBooking.Result.Created created => new CreateBookingResponse
            {
                BookingId = created.BookingId.Value,
                BookingState = MapBookingStateEnum(created.BookingState),
                UserEmail = created.BookingUserEmail.Value,
                HotelId = created.BookingHotelId.Value,
                RoomId = created.BookingRoomId.Value,
                CheckInDate = created.BookingCheckInDate.ToTimestamp(),
                CheckOutDate = created.BookingCheckOutDate.ToTimestamp(),
                CreatedAt = created.BokingCreatedAt.ToTimestamp(),
            },

            CreateBooking.Result.RoomNotAvailable _ => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Room not available")),

            CreateBooking.Result.RoomNotFound _ => throw new RpcException(new Status(
                StatusCode.NotFound,
                "Room not found")),

            _ => throw new UnreachableException(),
        };
    }

    private static BookingState MapBookingStateEnum(Application.Models.Enums.BookingState bookingState)
    {
        return bookingState switch
        {
            Application.Models.Enums.BookingState.Created => BookingState.Created,
            Application.Models.Enums.BookingState.Submitted => BookingState.Submitted,
            Application.Models.Enums.BookingState.Cancelled => BookingState.Cancelled,
            Application.Models.Enums.BookingState.Completed => BookingState.Completed,
            _ => BookingState.Unspecified,
        };
    }
}