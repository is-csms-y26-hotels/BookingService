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
                RoomId = created.BookingRoomId.Value,
                CheckInDate = created.BookingCheckInDate.ToTimestamp(),
                CheckOutDate = created.BookingCheckOutDate.ToTimestamp(),
                CreatedAt = created.BookingCreatedAt.ToTimestamp(),
            },

            CreateBooking.Result.RoomNotAvailable _ => ThrowRoomNotAvailableRpcException<CreateBookingResponse>(),

            CreateBooking.Result.RoomNotFound _ => ThrowRoomNotFoundRpcException<CreateBookingResponse>(),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<PostponeBookingResponse> PostponeBooking(PostponeBookingRequest request, ServerCallContext context)
    {
        var bookingRequest = new PostponeBooking.Request(
            new BookingId(request.BookingId),
            request.NewCheckInDate.ToDateTimeOffset(),
            request.NewCheckOutDate.ToDateTimeOffset());

        PostponeBooking.Result result = await reservationService.PostponeBookingAsync(
            bookingRequest,
            context.CancellationToken);

        return result switch
        {
            PostponeBooking.Result.Updated created => new PostponeBookingResponse
            {
                BookingId = created.BookingId.Value,
                CheckInDate = created.BookingCheckInDate.ToTimestamp(),
                CheckOutDate = created.BookingCheckOutDate.ToTimestamp(),
            },

            PostponeBooking.Result.RoomNotAvailable _ => ThrowRoomNotAvailableRpcException<PostponeBookingResponse>(),

            PostponeBooking.Result.InvalidBookingState invalidBookingState =>
                ThrowInvalidBookingStateRpcException<PostponeBookingResponse>(invalidBookingState.BookingState),

            PostponeBooking.Result.RoomNotFound _ => ThrowRoomNotFoundRpcException<PostponeBookingResponse>(),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<SubmitBookingResponse> SubmitBooking(SubmitBookingRequest request, ServerCallContext context)
    {
        var bookingRequest = new SubmitBooking.Request(
            new BookingId(request.BookingId));

        SubmitBooking.Result result = await reservationService.SubmitBookingAsync(
            bookingRequest,
            context.CancellationToken);

        return result switch
        {
            SubmitBooking.Result.Submitted submitted => new SubmitBookingResponse
            {
                BookingId = submitted.BookingId.Value,
            },

            SubmitBooking.Result.RoomNotFound _ => ThrowRoomNotFoundRpcException<SubmitBookingResponse>(),

            SubmitBooking.Result.InvalidBookingState invalidBookingState =>
                ThrowInvalidBookingStateRpcException<SubmitBookingResponse>(invalidBookingState.BookingState),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<CompleteBookingResponse> CompleteBooking(CompleteBookingRequest request, ServerCallContext context)
    {
        var bookingRequest = new CompleteBooking.Request(
            new BookingId(request.BookingId));

        CompleteBooking.Result result = await reservationService.CompleteBookingAsync(
            bookingRequest,
            context.CancellationToken);

        return result switch
        {
            CompleteBooking.Result.Completed completed => new CompleteBookingResponse
            {
                BookingId = completed.BookingId.Value,
            },

            CompleteBooking.Result.RoomNotFound _ => ThrowRoomNotFoundRpcException<CompleteBookingResponse>(),

            CompleteBooking.Result.InvalidBookingState invalidBookingState =>
                ThrowInvalidBookingStateRpcException<CompleteBookingResponse>(invalidBookingState.BookingState),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<CancelBookingResponse> CancelBooking(CancelBookingRequest request, ServerCallContext context)
    {
        var bookingRequest = new CancelBooking.Request(
            new BookingId(request.BookingId));

        CancelBooking.Result result = await reservationService.CancelBookingAsync(
            bookingRequest,
            context.CancellationToken);

        return result switch
        {
            CancelBooking.Result.Cancelled cancelled => new CancelBookingResponse
            {
                BookingId = cancelled.BookingId.Value,
            },

            CancelBooking.Result.RoomNotFound _ => ThrowRoomNotFoundRpcException<CancelBookingResponse>(),

            CancelBooking.Result.InvalidBookingState invalidBookingState =>
                ThrowInvalidBookingStateRpcException<CancelBookingResponse>(invalidBookingState.BookingState),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<GetRoomAvailableDateRangesResponse> GetRoomAvailableDateRanges(
        GetRoomAvailableDateRangesRequest request,
        ServerCallContext context)
    {
        var rangesRequest = new Application.Contracts.Bookings.Operations.GetRoomAvailableDateRanges.Request(
            new RoomId(request.RoomId),
            request.StartDate.ToDateTimeOffset(),
            request.EndDate.ToDateTimeOffset());

        GetRoomAvailableDateRanges.Result result = await reservationService.GetRoomAvailableDateRangesAsync(
            rangesRequest,
            context.CancellationToken);

        return result switch
        {
            GetRoomAvailableDateRanges.Result.Success success => new GetRoomAvailableDateRangesResponse
            {
                Available =
                {
                    success.Ranges.Select(range => new GetRoomAvailableDateRangesResponse.Types.Range
                    {
                        Start = range.Start.ToTimestamp(),
                        End = range.End.ToTimestamp(),
                    }).ToArray(),
                },
            },
            GetRoomAvailableDateRanges.Result.RoomNotFound _ =>
                ThrowRoomNotFoundRpcException<GetRoomAvailableDateRangesResponse>(),

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

    private static T ThrowRoomNotFoundRpcException<T>()
    {
        throw new RpcException(new Status(StatusCode.NotFound, "Room not found"));
    }

    private static T ThrowInvalidBookingStateRpcException<T>(BookingService.Application.Models.Enums.BookingState bookingState)
    {
        throw new RpcException(new Status(
            StatusCode.InvalidArgument,
            $"Invalid booking state: {bookingState}"));
    }

    private static T ThrowRoomNotAvailableRpcException<T>()
    {
        throw new RpcException(new Status(StatusCode.InvalidArgument, "Room not available"));
    }
}