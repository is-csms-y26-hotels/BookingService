using BookingService.Application.Abstractions.Gateways;
using BookingService.Application.Abstractions.Gateways.Models.Accommodation;
using BookingService.Application.Abstractions.Persistence;
using BookingService.Application.Abstractions.Persistence.Queries;
using BookingService.Application.Contracts.Bookings;
using BookingService.Application.Contracts.Bookings.Events;
using BookingService.Application.Contracts.Bookings.Operations;
using BookingService.Application.Models.Enums;
using BookingService.Application.Models.Models;
using BookingService.Application.Models.ObjectValues;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Persistence.Abstractions.Transactions;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Application.Bookings;

public class BookingService(
    IPersistenceContext persistenceContext,
    IPersistenceTransactionProvider transactionProvider,
    IAccommodationGateway accommodationGateway,
    IEventPublisher eventPublisher) : IBookingService
{
    public async Task<CreateBooking.Result> CreateBookingAsync(
        CreateBooking.Request request,
        CancellationToken cancellationToken)
    {
        if (!await accommodationGateway.RoomExistsAsync(
                new RoomInfoModel(request.HotelId, request.RoomId),
                cancellationToken))
        {
            return new CreateBooking.Result.RoomNotFound();
        }

        if (await persistenceContext.BookingInfos.QueryAsync(
                    new BookingInfoQuery.Builder()
                        .WithHotelId(request.HotelId)
                        .WithRoomId(request.RoomId)
                        .WithMinCheckInDate(request.CheckInDate)
                        .WithMaxCheckOutDate(request.CheckOutDate)
                        .WithPageSize(1)
                        .Build(),
                    cancellationToken)
                .AnyAsync(cancellationToken))
        {
            return new CreateBooking.Result.RoomNotAvailable();
        }

        await using IPersistenceTransaction transaction = await transactionProvider.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        BookingInfo bookingInfo = await persistenceContext.BookingInfos.AddAsync(
            new BookingInfo(
                BookingInfoId.Default,
                request.HotelId,
                request.RoomId,
                request.UserEmail,
                request.CheckInDate,
                request.CheckOutDate),
            cancellationToken);

        Booking booking = await persistenceContext.Bookings.AddAsync(
            new Booking(
                BookingId.Default,
                BookingState.Created,
                bookingInfo.Id,
                DateTimeOffset.Now),
            cancellationToken);

        var evt = new BookingSubmissionEvent(booking.Id.Value);

        await eventPublisher.PublishAsync(evt, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new CreateBooking.Result.Created(
            booking.Id,
            booking.State,
            bookingInfo.HotelId,
            bookingInfo.RoomId,
            bookingInfo.UserEmail,
            bookingInfo.CheckInDate,
            bookingInfo.CheckOutDate,
            booking.CreatedAt);
    }
}