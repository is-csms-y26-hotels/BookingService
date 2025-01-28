using BookingService.Application.Abstractions.Gateways;
using BookingService.Application.Abstractions.Persistence;
using BookingService.Application.Abstractions.Persistence.Queries;
using BookingService.Application.Contracts.Bookings;
using BookingService.Application.Contracts.Bookings.Events;
using BookingService.Application.Contracts.Bookings.Operations;
using BookingService.Application.Exceptions;
using BookingService.Application.Models.Enums;
using BookingService.Application.Models.Models;
using BookingService.Application.Models.ObjectValues;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using System.Data;

namespace BookingService.Application.Bookings;

public class ReservationService(
    IPersistenceContext persistenceContext,
    IPersistenceTransactionProvider transactionProvider,
    IAccommodationGateway accommodationGateway,
    IEventPublisher eventPublisher) : IReservationService
{
    public async Task<CreateBooking.Result> CreateBookingAsync(
        CreateBooking.Request request,
        CancellationToken cancellationToken)
    {
        if (!await accommodationGateway.RoomExistsAsync(request.RoomId, cancellationToken))
            return new CreateBooking.Result.RoomNotFound();

        await using IPersistenceTransaction transaction = await transactionProvider.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        if (!await RoomAvailableAsync(
                request.RoomId,
                request.CheckInDate,
                request.CheckOutDate,
                null,
                cancellationToken))
        {
            return new CreateBooking.Result.RoomNotAvailable();
        }

        Booking booking = await persistenceContext.Bookings.AddAsync(
            new Booking(
                BookingId.Default,
                BookingState.Created,
                new BookingInfo(
                    BookingInfoId.Default,
                    BookingId.Default,
                    request.RoomId,
                    request.UserEmail,
                    request.CheckInDate,
                    request.CheckOutDate),
                DateTimeOffset.UtcNow),
            cancellationToken);

        var evt = new BookingCreatedEvent(
            booking.BookingId,
            booking.BookingInfo.BookingInfoRoomId,
            booking.BookingInfo.BookingInfoUserEmail,
            booking.BookingInfo.BookingInfoCheckInDate,
            booking.BookingInfo.BookingInfoCheckOutDate,
            booking.BookingCreatedAt);

        await eventPublisher.PublishAsync(evt, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new CreateBooking.Result.Created(
            booking.BookingId,
            booking.BookingState,
            booking.BookingInfo.BookingInfoRoomId,
            booking.BookingInfo.BookingInfoUserEmail,
            booking.BookingInfo.BookingInfoCheckInDate,
            booking.BookingInfo.BookingInfoCheckOutDate,
            booking.BookingCreatedAt);
    }

    public async Task<PostponeBooking.Result> PostponeBookingAsync(PostponeBooking.Request request, CancellationToken cancellationToken)
    {
        await using IPersistenceTransaction transaction = await transactionProvider.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        BookingQuery bookingQuery = new BookingQuery.Builder()
            .WithBookingId(request.BookingId)
            .WithPageSize(1)
            .Build();

        Booking? booking = await persistenceContext.Bookings.QueryAsync(bookingQuery, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (booking is null)
            return new PostponeBooking.Result.RoomNotFound();

        List<BookingState> validStates = [BookingState.Created];
        if (!validStates.Contains(booking.BookingState))
            return new PostponeBooking.Result.InvalidBookingState(booking.BookingState);

        if (!await RoomAvailableAsync(
                booking.BookingInfo.BookingInfoRoomId,
                request.NewCheckInDate,
                request.NewCheckOutDate,
                booking.BookingId,
                cancellationToken))
        {
            return new PostponeBooking.Result.RoomNotAvailable();
        }

        Booking updatedBooking = await persistenceContext.Bookings.UpdateAsync(
            booking with
            {
                BookingInfo = booking.BookingInfo with
                {
                    BookingInfoCheckInDate = request.NewCheckInDate,
                    BookingInfoCheckOutDate = request.NewCheckOutDate,
                },
            },
            cancellationToken);

        var evt = new BookingUpdatedEvent(
            updatedBooking.BookingId,
            updatedBooking.BookingState,
            updatedBooking.BookingInfo.BookingInfoRoomId,
            updatedBooking.BookingInfo.BookingInfoUserEmail,
            updatedBooking.BookingInfo.BookingInfoCheckInDate,
            updatedBooking.BookingInfo.BookingInfoCheckOutDate);

        await eventPublisher.PublishAsync(evt, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new PostponeBooking.Result.Updated(
            booking.BookingId,
            updatedBooking.BookingInfo.BookingInfoCheckInDate,
            updatedBooking.BookingInfo.BookingInfoCheckOutDate);
    }

    public async Task<SubmitBooking.Result> SubmitBookingAsync(SubmitBooking.Request request, CancellationToken cancellationToken)
    {
        try
        {
            BookingId bookingId = await UpdateBookingState(
                request.BookingId,
                BookingState.Submitted,
                [BookingState.Created],
                cancellationToken);

            return new SubmitBooking.Result.Submitted(bookingId);
        }
        catch (BookingNotFoundException)
        {
            return new SubmitBooking.Result.RoomNotFound();
        }
        catch (InvalidBookingStateException exception)
        {
            return new SubmitBooking.Result.InvalidBookingState(exception.BookingState);
        }
    }

    public async Task<CompleteBooking.Result> CompleteBookingAsync(CompleteBooking.Request request, CancellationToken cancellationToken)
    {
        try
        {
            BookingId bookingId = await UpdateBookingState(
                request.BookingId,
                BookingState.Completed,
                [BookingState.Submitted],
                cancellationToken);

            return new CompleteBooking.Result.Completed(bookingId);
        }
        catch (BookingNotFoundException)
        {
            return new CompleteBooking.Result.RoomNotFound();
        }
        catch (InvalidBookingStateException exception)
        {
            return new CompleteBooking.Result.InvalidBookingState(exception.BookingState);
        }
    }

    public async Task<CancelBooking.Result> CancelBookingAsync(CancelBooking.Request request, CancellationToken cancellationToken)
    {
        try
        {
            BookingId bookingId = await UpdateBookingState(
                request.BookingId,
                BookingState.Submitted,
                [BookingState.Created, BookingState.Submitted],
                cancellationToken);

            return new CancelBooking.Result.Cancelled(bookingId);
        }
        catch (BookingNotFoundException)
        {
            return new CancelBooking.Result.RoomNotFound();
        }
        catch (InvalidBookingStateException exception)
        {
            return new CancelBooking.Result.InvalidBookingState(exception.BookingState);
        }
    }

    public async Task<GetRoomAvailableDateRanges.Result> GetRoomAvailableDateRangesAsync(GetRoomAvailableDateRanges.Request request, CancellationToken cancellationToken)
    {
        if (!await accommodationGateway.RoomExistsAsync(request.RoomId, cancellationToken))
            return new GetRoomAvailableDateRanges.Result.RoomNotFound();

        BookingQuery bookingsQuery = new BookingQuery.Builder()
            .WithBookingInfoDateRange((request.StartDate, request.EndDate))
            .WithPageSize(int.MaxValue)
            .Build();

        IAsyncEnumerable<Booking> bookings = persistenceContext.Bookings.QueryAsync(bookingsQuery, cancellationToken);

        IReadOnlyCollection<(DateTimeOffset Start, DateTimeOffset End)> bookingsRanges = await bookings
            .Select(
                booking => (
                    booking.BookingInfo.BookingInfoCheckInDate,
                    booking.BookingInfo.BookingInfoCheckOutDate))
            .ToListAsync(cancellationToken);

        IReadOnlyCollection<(DateTimeOffset Start, DateTimeOffset End)> availableRanges = GetAvailableRanges(
            bookingsRanges,
            request.StartDate,
            request.EndDate);

        return new GetRoomAvailableDateRanges.Result.Success(availableRanges);
    }

    private static IReadOnlyCollection<(DateTimeOffset Start, DateTimeOffset End)> GetAvailableRanges(
        IReadOnlyCollection<(DateTimeOffset Start, DateTimeOffset End)> bookedRanges,
        DateTimeOffset searchStart,
        DateTimeOffset searchEnd)
    {
        var availableRanges = new List<(DateTimeOffset Start, DateTimeOffset End)>();

        DateTimeOffset currentStart = searchStart;

        foreach ((DateTimeOffset start, DateTimeOffset end) in bookedRanges)
        {
            if (currentStart < start)
            {
                availableRanges.Add((currentStart, start.AddDays(-1)));
            }

            currentStart = end.AddDays(1);
        }

        if (currentStart <= searchEnd)
        {
            availableRanges.Add((currentStart, searchEnd));
        }

        return availableRanges;
    }

    private async Task<BookingId> UpdateBookingState(
        BookingId bookingId,
        BookingState bookingState,
        IReadOnlyCollection<BookingState> validBookingStates,
        CancellationToken cancellationToken)
    {
        await using IPersistenceTransaction transaction = await transactionProvider.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        BookingQuery bookingQuery = new BookingQuery.Builder()
            .WithBookingId(bookingId)
            .WithPageSize(1)
            .Build();

        Booking booking = await persistenceContext.Bookings.QueryAsync(bookingQuery, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken)
                          ?? throw new BookingNotFoundException();

        if (!validBookingStates.Contains(booking.BookingState))
            throw new InvalidBookingStateException(booking.BookingState);

        Booking updatedBooking = await persistenceContext.Bookings.UpdateAsync(
            booking with { BookingState = bookingState },
            cancellationToken);

        var evt = new BookingUpdatedEvent(
            updatedBooking.BookingId,
            updatedBooking.BookingState,
            updatedBooking.BookingInfo.BookingInfoRoomId,
            updatedBooking.BookingInfo.BookingInfoUserEmail,
            updatedBooking.BookingInfo.BookingInfoCheckInDate,
            updatedBooking.BookingInfo.BookingInfoCheckOutDate);

        await eventPublisher.PublishAsync(evt, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return updatedBooking.BookingId;
    }

    private async Task<bool> RoomAvailableAsync(
        RoomId roomId,
        DateTimeOffset checkInDate,
        DateTimeOffset checkOutDate,
        BookingId? bookingId,
        CancellationToken cancellationToken)
    {
        return !await persistenceContext.Bookings.QueryAsync(
                new BookingQuery.Builder()
                    .WithBookingInfoRoomId(roomId)
                    .WithBookingToExcludeIds(bookingId is null ? [] : [bookingId.Value])
                    .WithBookingInfoDateRange((checkInDate, checkOutDate))
                    .WithPageSize(1)
                    .Build(),
                cancellationToken)
            .AnyAsync(cancellationToken);
    }
}