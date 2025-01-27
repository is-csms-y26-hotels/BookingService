using BookingService.Application.Abstractions.Persistence.Queries;
using BookingService.Application.Abstractions.Persistence.Repositories;
using BookingService.Application.Models.Enums;
using BookingService.Application.Models.Models;
using BookingService.Application.Models.ObjectValues;
using Itmo.Dev.Platform.Persistence.Abstractions.Commands;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace BookingService.Infrastructure.Persistence.Repositories;

public class BookingRepository(
    IPersistenceConnectionProvider connectionProvider) : IBookingRepository
{
    public async Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        const string sql = """
                           with booking_insert as (insert into bookings (booking_state, booking_created_at)
                           values (@booking_state, @booking_created_at)
                           returning booking_id, booking_state, booking_created_at),
                           
                           booking_info_insert as (insert into booking_infos (
                                                        booking_id,
                                                        booking_info_room_id,
                                                        booking_info_user_email,
                                                        booking_info_checkin_date,
                                                        booking_info_checkout_date)
                           values (
                                    (select booking_id FROM booking_insert),
                                    @booking_info_room_id,
                                    @booking_info_user_email,
                                    @booking_info_checkin_date,
                                    @booking_info_checkout_date)
                           returning    booking_info_id,
                                        booking_id,
                                        booking_info_room_id,
                                        booking_info_user_email,
                                        booking_info_checkin_date,
                                        booking_info_checkout_date)
                                        
                           select
                               b.booking_id,
                               b.booking_state,
                               b.booking_created_at,
                               bi.booking_info_id,
                               bi.booking_info_room_id,
                               bi.booking_info_user_email,
                               bi.booking_info_checkin_date,
                               bi.booking_info_checkout_date
                           from booking_insert b
                           cross join booking_info_insert bi
                           """;

        await using IPersistenceConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IPersistenceCommand command = connection.CreateCommand(sql)
            .AddParameter("booking_state", booking.BookingState)
            .AddParameter("booking_created_at", booking.BookingCreatedAt)
            .AddParameter("booking_info_room_id", booking.BookingInfo.BookingInfoRoomId.Value)
            .AddParameter("booking_info_user_email", booking.BookingInfo.BookingInfoUserEmail.Value)
            .AddParameter("booking_info_checkin_date", booking.BookingInfo.BookingInfoCheckInDate)
            .AddParameter("booking_info_checkout_date", booking.BookingInfo.BookingInfoCheckOutDate);

        DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);

        return ReadBooking(reader);
    }

    public async Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken)
    {
        const string sql = """
                           with update_booking as (update bookings
                           set  booking_state = @booking_state,
                                booking_created_at = @booking_created_at
                           where booking_id = @booking_id
                           returning booking_id, booking_state, booking_created_at),
                           
                           update_booking_info as (update booking_infos
                           set  booking_info_room_id = @booking_info_room_id,
                                booking_info_user_email = @booking_info_user_email,
                                booking_info_checkin_date = @booking_info_checkin_date,
                                booking_info_checkout_date = @booking_info_checkout_date
                           where booking_id = @booking_id
                           returning    booking_info_id,
                                        booking_id,
                                        booking_info_room_id,
                                        booking_info_user_email,
                                        booking_info_checkin_date,
                                        booking_info_checkout_date)
                           
                           select
                               b.booking_id,
                               b.booking_state,
                               b.booking_created_at,
                               bi.booking_info_id,
                               bi.booking_info_room_id,
                               bi.booking_info_user_email,
                               bi.booking_info_checkin_date,
                               bi.booking_info_checkout_date
                           from update_booking b
                           cross join update_booking_info bi;
                           """;

        await using IPersistenceConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IPersistenceCommand command = connection.CreateCommand(sql)
            .AddParameter("booking_id", booking.BookingId.Value)
            .AddParameter("booking_state", booking.BookingState)
            .AddParameter("booking_created_at", booking.BookingCreatedAt)
            .AddParameter("booking_info_room_id", booking.BookingInfo.BookingInfoRoomId.Value)
            .AddParameter("booking_info_user_email", booking.BookingInfo.BookingInfoUserEmail.Value)
            .AddParameter("booking_info_checkin_date", booking.BookingInfo.BookingInfoCheckInDate)
            .AddParameter("booking_info_checkout_date", booking.BookingInfo.BookingInfoCheckOutDate);

        DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);

        return ReadBooking(reader);
    }

    public async IAsyncEnumerable<Booking> QueryAsync(
        BookingQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
                           select   b.booking_id,
                                    b.booking_state,
                                    b.booking_created_at,
                                    bi.booking_info_id,
                                    bi.booking_info_room_id,
                                    bi.booking_info_user_email,
                                    bi.booking_info_checkin_date,
                                    bi.booking_info_checkout_date
                           from bookings as b
                           left join booking_infos as bi on bi.booking_id = b.booking_id
                           where
                               (cardinality(@booking_ids) = 0 or b.booking_id = any(@booking_ids))
                               and (cardinality(@booking_to_exclude_ids) = 0 or b.booking_id != any(@booking_to_exclude_ids))
                               and (cardinality(@booking_info_room_ids) = 0 or bi.booking_info_room_id = any(@booking_info_room_ids))
                               and (cardinality(@booking_info_user_emails) = 0 or bi.booking_info_user_email = any(@booking_info_user_emails))
                               and (@booking_info_date_range_start is null or @booking_info_date_range_end is null or
                                   (bi.booking_info_checkin_date <= @booking_info_date_range_end and bi.booking_info_checkout_date >= @booking_info_date_range_start))
                               and b.booking_id > @cursor
                           order by bi.booking_info_checkin_date
                           limit @page_size;
                           """;

        await using IPersistenceConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IPersistenceCommand command = connection.CreateCommand(sql)
            .AddParameter("booking_ids", query.BookingIds.Select(id => id.Value).ToArray())
            .AddParameter("booking_to_exclude_ids", query.BookingToExcludeIds.Select(id => id.Value).ToArray())
            .AddParameter("booking_info_room_ids", query.BookingInfoRoomIds.Select(id => id.Value).ToArray())
            .AddParameter("booking_info_user_emails", query.BookingInfoUserEmails.Select(email => email.Value).ToArray())
            .AddParameter("booking_info_date_range_start", query.BookingInfoDateRange?.Start)
            .AddParameter("booking_info_date_range_end", query.BookingInfoDateRange?.End)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize);

        DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
            yield return ReadBooking(reader);
    }

    private static Booking ReadBooking(DbDataReader reader)
    {
        return new Booking(
            new BookingId(reader.GetInt64("booking_id")),
            reader.GetFieldValue<BookingState>("booking_state"),
            new BookingInfo(
                new BookingInfoId(reader.GetInt64("booking_info_id")),
                new BookingId(reader.GetInt64("booking_id")),
                new RoomId(reader.GetInt64("booking_info_room_id")),
                new UserEmail(reader.GetString("booking_info_user_email")),
                reader.GetFieldValue<DateTimeOffset>("booking_info_checkin_date"),
                reader.GetFieldValue<DateTimeOffset>("booking_info_checkout_date")),
            reader.GetFieldValue<DateTimeOffset>("booking_created_at"));
    }
}