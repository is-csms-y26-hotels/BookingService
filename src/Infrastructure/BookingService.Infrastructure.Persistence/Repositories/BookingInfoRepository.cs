using BookingService.Application.Abstractions.Persistence.Queries;
using BookingService.Application.Abstractions.Persistence.Repositories;
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

public class BookingInfoRepository(
    IPersistenceConnectionProvider connectionProvider) : IBookingInfoRepository
{
    public async Task<BookingInfo> AddAsync(BookingInfo bookingInfo, CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into booking_infos (booking_info_hotel_id,
                                                      booking_info_room_id,
                                                      booking_info_user_email,
                                                      booking_info_checkin_date,
                                                      booking_info_checkout_date)
                           values (:booking_info_hotel_id,
                                   :booking_info_room_id,
                                   :booking_info_user_email,
                                   :booking_info_checkin_date,
                                   :booking_info_checkout_date)
                           returning booking_info_id,
                                     booking_info_hotel_id,
                                     booking_info_room_id,
                                     booking_info_user_email,
                                     booking_info_checkin_date,
                                     booking_info_checkout_date
                           """;

        await using IPersistenceConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IPersistenceCommand command = connection.CreateCommand(sql)
            .AddParameter("booking_info_hotel_id", bookingInfo.HotelId.Value)
            .AddParameter("booking_info_room_id", bookingInfo.RoomId.Value)
            .AddParameter("booking_info_user_email", bookingInfo.UserEmail.Value)
            .AddParameter("booking_info_checkin_date", bookingInfo.CheckInDate)
            .AddParameter("booking_info_checkout_date", bookingInfo.CheckOutDate);

        DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        return ReadBookingInfo(reader);
    }

    public async IAsyncEnumerable<BookingInfo> QueryAsync(
        BookingInfoQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
                           select   bi.booking_info_id,
                                    bi.booking_info_hotel_id,
                                    bi.booking_info_room_id,
                                    bi.booking_info_user_email,
                                    bi.booking_info_checkin_date,
                                    bi.booking_info_checkout_date
                           from booking_infos as bi
                           where
                               (cardinality(:booking_info_ids) = 0 or bi.booking_info_id = any(:booking_info_ids))
                               and (cardinality(:hotel_ids) = 0 or bi.booking_info_hotel_id = any(:hotel_ids))
                               and (cardinality(:room_ids) = 0 or bi.booking_info_room_id = any(:room_ids))
                               and (cardinality(:user_emails) = 0 or bi.booking_info_user_email = any(:user_emails))
                               and (:min_check_in_date is null or :min_check_in_date <= bi.booking_info_checkin_date)
                               and (:max_check_out_date is null or :max_check_out_date >= bi.booking_info_checkout_date)
                               and bi.booking_info_id > :cursor
                           limit :page_size;
                           """;

        await using IPersistenceConnection connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using IPersistenceCommand command = connection.CreateCommand(sql)
            .AddParameter("booking_info_ids", query.BookingInfoIds.Select(id => id.Value).ToArray())
            .AddParameter("hotel_ids", query.HotelIds.Select(id => id.Value).ToArray())
            .AddParameter("room_ids", query.RoomIds.Select(id => id.Value).ToArray())
            .AddParameter("user_emails", query.UserEmails.Select(email => email.Value).ToArray())
            .AddParameter("min_check_in_date", query.MinCheckInDate)
            .AddParameter("max_check_out_date", query.MaxCheckOutDate)
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
            new HotelId(reader.GetInt64("booking_info_hotel_id")),
            new RoomId(reader.GetInt64("booking_info_room_id")),
            new UserEmail(reader.GetString("booking_info_user_email")),
            reader.GetFieldValue<DateTimeOffset>("booking_info_checkin_date"),
            reader.GetFieldValue<DateTimeOffset>("booking_info_checkout_date"));
    }
}