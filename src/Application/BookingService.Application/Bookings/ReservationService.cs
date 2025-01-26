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
        if (!await accommodationGateway.RoomExistsAsync(
                request.RoomId,
                cancellationToken))
        {
            return new CreateBooking.Result.RoomNotFound();
        }

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

        BookingInfo bookingInfo = await persistenceContext.BookingInfos.AddAsync(
            new BookingInfo(
                BookingInfoId.Default,
                request.RoomId,
                request.UserEmail,
                request.CheckInDate,
                request.CheckOutDate),
            cancellationToken);

        Booking booking = await persistenceContext.Bookings.AddAsync(
            new Booking(
                BookingId.Default,
                BookingState.Created,
                bookingInfo.BookingInfoId,
                DateTimeOffset.UtcNow),
            cancellationToken);

        var evt = new BookingCreatedEvent(
            booking.BookingId,
            booking.BookingState,
            bookingInfo.BookingInfoRoomId,
            bookingInfo.BookingInfoUserEmail,
            bookingInfo.BookingInfoCheckInDate,
            bookingInfo.BookingInfoCheckOutDate);

        await eventPublisher.PublishAsync(evt, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new CreateBooking.Result.Created(
            booking.BookingId,
            booking.BookingState,
            bookingInfo.BookingInfoRoomId,
            bookingInfo.BookingInfoUserEmail,
            bookingInfo.BookingInfoCheckInDate,
            bookingInfo.BookingInfoCheckOutDate,
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
        {
            return new PostponeBooking.Result.RoomNotFound();
        }

        List<BookingState> validStates = [BookingState.Created];
        if (!validStates.Contains(booking.BookingState))
        {
            return new PostponeBooking.Result.InvalidBookingState(booking.BookingState);
        }

        BookingInfoQuery bookingInfoQuery = new BookingInfoQuery.Builder()
            .WithBookingInfoId(booking.BookingInfoId)
            .WithPageSize(1)
            .Build();

        BookingInfo bookingInfo = await persistenceContext.BookingInfos.QueryAsync(bookingInfoQuery, cancellationToken)
            .FirstAsync(cancellationToken);

        if (!await RoomAvailableAsync(
                bookingInfo.BookingInfoRoomId,
                request.NewCheckInDate,
                request.NewCheckOutDate,
                bookingInfo.BookingInfoId,
                cancellationToken))
        {
            return new PostponeBooking.Result.RoomNotAvailable();
        }

        BookingInfo updatedBookingInfo = await persistenceContext.BookingInfos.UpdateAsync(
            bookingInfo with
            {
                BookingInfoCheckInDate = request.NewCheckInDate,
                BookingInfoCheckOutDate = request.NewCheckOutDate,
            },
            cancellationToken);

        var evt = new BookingUpdatedEvent(
            booking.BookingId,
            booking.BookingState,
            updatedBookingInfo.BookingInfoRoomId,
            updatedBookingInfo.BookingInfoUserEmail,
            updatedBookingInfo.BookingInfoCheckInDate,
            updatedBookingInfo.BookingInfoCheckOutDate);

        await eventPublisher.PublishAsync(evt, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new PostponeBooking.Result.Updated(
            booking.BookingId,
            updatedBookingInfo.BookingInfoCheckInDate,
            updatedBookingInfo.BookingInfoCheckOutDate);
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
                BookingState.Submitted,
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
        {
            throw new InvalidBookingStateException(booking.BookingState);
        }

        Booking updatedBooking = await persistenceContext.Bookings.UpdateStateAsync(
            booking.BookingId,
            bookingState,
            cancellationToken);

        BookingInfoQuery bookingInfoQuery = new BookingInfoQuery.Builder()
            .WithBookingInfoId(booking.BookingInfoId)
            .WithPageSize(1)
            .Build();

        BookingInfo bookingInfo = await persistenceContext.BookingInfos.QueryAsync(bookingInfoQuery, cancellationToken)
            .FirstAsync(cancellationToken);

        var evt = new BookingUpdatedEvent(
            updatedBooking.BookingId,
            updatedBooking.BookingState,
            bookingInfo.BookingInfoRoomId,
            bookingInfo.BookingInfoUserEmail,
            bookingInfo.BookingInfoCheckInDate,
            bookingInfo.BookingInfoCheckOutDate);

        await eventPublisher.PublishAsync(evt, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return updatedBooking.BookingId;
    }

    private async Task<bool> RoomAvailableAsync(
        RoomId roomId,
        DateTimeOffset checkInDate,
        DateTimeOffset checkOutDate,
        BookingInfoId? bookingInfoId,
        CancellationToken cancellationToken)
    {
        return !await persistenceContext.BookingInfos.QueryAsync(
                new BookingInfoQuery.Builder()
                    .WithRoomId(roomId)
                    .WithBookingInfoToExcludeIds(bookingInfoId is null ? [] : [bookingInfoId.Value])
                    .WithDateRange(new BookingInfoQuery.DateRangeModel(checkInDate, checkOutDate))
                    .WithPageSize(1)
                    .Build(),
                cancellationToken)
            .AnyAsync(cancellationToken);
    }
}