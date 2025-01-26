using BookingService.Application.Abstractions.Persistence.Queries;
using BookingService.Application.Abstractions.Persistence.Repositories;
using BookingService.Application.Models.Models;
using BookingService.Application.Models.ObjectValues;
using Itmo.Dev.Platform.Persistence.Abstractions.Commands;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace BookingService.Infrastructure.Persistence.Repositories;

public class BookingInfoRepository(
    IPersistenceConnectionProvider connectionProvider) : IBookingInfoRepository
{
    public async Task<BookingInfo> AddAsync(BookingInfo bookingInfo, CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into booking_infos (booking_info_room_id,
                                                      booking_info_user_email,
                                                      booking_info_checkin_date,
                                                      booking_info_checkout_date)
                           values (:booking_info_room_id,
                                   :booking_info_user_email,
                                   :booking_info_checkin_date,
                                   :booking_info_checkout_date)
                           returning booking_info_id,
                                     booking_info_room_id,
                                     booking_info_user_email,
                                     booking_info_checkin_date,
                                     booking_info_checkout_date
                           """;

        await using IPersistenceConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IPersistenceCommand command = connection.CreateCommand(sql)
            .AddParameter("booking_info_room_id", bookingInfo.BookingInfoRoomId.Value)
            .AddParameter("booking_info_user_email", bookingInfo.BookingInfoUserEmail.Value)
            .AddParameter("booking_info_checkin_date", bookingInfo.BookingInfoCheckInDate)
            .AddParameter("booking_info_checkout_date", bookingInfo.BookingInfoCheckOutDate);

        DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);

        return ReadBookingInfo(reader);
    }

    public async Task<BookingInfo> UpdateAsync(BookingInfo bookingInfo, CancellationToken cancellationToken)
    {
        const string sql = """
                           update booking_infos
                           set  booking_info_room_id = :booking_info_room_id,
                                booking_info_user_email = :booking_info_user_email,
                                booking_info_checkin_date = :booking_info_checkin_date,
                                booking_info_checkout_date = :booking_info_checkout_date
                           where booking_info_id = :booking_info_id
                           returning booking_info_id,
                                     booking_info_room_id,
                                     booking_info_user_email,
                                     booking_info_checkin_date,
                                     booking_info_checkout_date
                           """;

        await using IPersistenceConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IPersistenceCommand command = connection.CreateCommand(sql)
            .AddParameter("booking_info_id", bookingInfo.BookingInfoId.Value)
            .AddParameter("booking_info_room_id", bookingInfo.BookingInfoRoomId.Value)
            .AddParameter("booking_info_user_email", bookingInfo.BookingInfoUserEmail.Value)
            .AddParameter("booking_info_checkin_date", bookingInfo.BookingInfoCheckInDate)
            .AddParameter("booking_info_checkout_date", bookingInfo.BookingInfoCheckOutDate);

        DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        await reader.ReadAsync(cancellationToken);

        return ReadBookingInfo(reader);
    }

    public async IAsyncEnumerable<BookingInfo> QueryAsync(
        BookingInfoQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
                           select   bi.booking_info_id,
                                    bi.booking_info_room_id,
                                    bi.booking_info_user_email,
                                    bi.booking_info_checkin_date,
                                    bi.booking_info_checkout_date
                           from booking_infos as bi
                           where
                               (cardinality(:booking_info_ids) = 0 or bi.booking_info_id = any(:booking_info_ids))
                               and (cardinality(:booking_info_to_exclude_ids) = 0 or bi.booking_info_id != any(:booking_info_to_exclude_ids))
                               and (cardinality(:room_ids) = 0 or bi.booking_info_room_id = any(:room_ids))
                               and (cardinality(:user_emails) = 0 or bi.booking_info_user_email = any(:user_emails))
                               AND (:date_range_start IS NULL OR :date_range_end IS NULL OR 
                                   (bi.booking_info_checkin_date <= :date_range_end AND bi.booking_info_checkout_date >= :date_range_start))
                               and bi.booking_info_id > :cursor
                           limit :page_size;
                           """;

        await using IPersistenceConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IPersistenceCommand command = connection.CreateCommand(sql)
            .AddParameter("booking_info_ids", query.BookingInfoIds.Select(id => id.Value).ToArray())
            .AddParameter("booking_info_to_exclude_ids", query.BookingInfoToExcludeIds.Select(id => id.Value).ToArray())
            .AddParameter("room_ids", query.RoomIds.Select(id => id.Value).ToArray())
            .AddParameter("user_emails", query.UserEmails.Select(email => email.Value).ToArray())
            .AddParameter("date_range_start", query.DateRange?.Start)
            .AddParameter("date_range_end", query.DateRange?.End)
            .AddParameter("cursor", query.Cursor)
            .AddParameter("page_size", query.PageSize);

        DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return ReadBookingInfo(reader);
        }
    }

    private static BookingInfo ReadBookingInfo(DbDataReader reader)
    {
        return new BookingInfo(
            new BookingInfoId(reader.GetInt64("booking_info_id")),
            new RoomId(reader.GetInt64("booking_info_room_id")),
            new UserEmail(reader.GetString("booking_info_user_email")),
            reader.GetFieldValue<DateTimeOffset>("booking_info_checkin_date"),
            reader.GetFieldValue<DateTimeOffset>("booking_info_checkout_date"));
    }
}