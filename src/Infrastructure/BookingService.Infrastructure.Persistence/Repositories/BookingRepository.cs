using BookingService.Application.Abstractions.Persistence.Queries;
using BookingService.Application.Abstractions.Persistence.Repositories;
using BookingService.Application.Models.Enums;
using BookingService.Application.Models.Models;
using BookingService.Application.Models.ObjectValues;
using Itmo.Dev.Platform.Persistence.Abstractions.Commands;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BookingService.Infrastructure.Persistence.Repositories;

public class BookingRepository(
    IPersistenceConnectionProvider connectionProvider) : IBookingRepository
{
    public async Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into bookings (booking_state, booking_info_id, booking_created_at)
                           values (:booking_state, :booking_info_id, :booking_created_at)
                           returning booking_id, booking_state, booking_info_id, booking_created_at
                           """;

        await using IPersistenceConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IPersistenceCommand command = connection.CreateCommand(sql)
            .AddParameter("booking_state", booking.State)
            .AddParameter("booking_info_id", booking.InfoId.Value)
            .AddParameter("booking_created_at", booking.CreatedAt);

        DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return ReadBooking(reader);
    }

    public async Task<Booking> UpdateStateAsync(
        BookingId bookingId,
        BookingState bookingState,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           update bookings
                           set booking_state = :booking_state
                           where booking_id = :booking_id
                           returning booking_id, booking_state, booking_info_id, booking_created_at
                           """;

        await using IPersistenceConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IPersistenceCommand command = connection.CreateCommand(sql)
            .AddParameter("booking_id", bookingId.Value)
            .AddParameter("booking_state", bookingState);

        DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return ReadBooking(reader);
    }

    public async IAsyncEnumerable<Booking> QueryAsync(
        BookingQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
                           select   b.booking_id,
                                    b.booking_state,
                                    b.booking_info_id,
                                    b.booking_created_at
                           from bookings as b
                           where
                               (cardinality(:booking_ids) = 0 or booking_id = any(:booking_ids))
                               and b.booking_id > :cursor
                           limit :page_size;
                           """;

        await using IPersistenceConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IPersistenceCommand command = connection.CreateCommand(sql)
            .AddParameter("booking_ids", query.BookingIds.Select(id => id.Value).ToArray())
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize);

        DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return ReadBooking(reader);
        }
    }

    private static Booking ReadBooking(DbDataReader reader)
    {
        return new Booking(
            new BookingId(reader.GetInt64("booking_id")),
            reader.GetFieldValue<BookingState>("booking_state"),
            new BookingInfoId(reader.GetInt64("booking_info_id")),
            reader.GetFieldValue<DateTimeOffset>("booking_created_at"));
    }
}