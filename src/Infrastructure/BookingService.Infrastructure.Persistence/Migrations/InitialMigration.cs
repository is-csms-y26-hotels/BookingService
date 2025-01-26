using FluentMigrator;
using Itmo.Dev.Platform.Persistence.Postgres.Migrations;

namespace BookingService.Infrastructure.Persistence.Migrations;

[Migration(1, "Initial Migration")]
public class InitialMigration : SqlMigration
{
    protected override string GetUpSql(IServiceProvider serviceProvider)
    {
        return """
               create table booking_infos
               (
                   booking_info_id              bigint primary key generated always as identity,
                   booking_info_hotel_id         bigint not null,
                   booking_info_room_id         bigint not null,
                   booking_info_user_email      text not null,
                   booking_info_checkin_date    timestamp with time zone not null,
                   booking_info_checkout_date   timestamp with time zone not null
               );
               
               create type booking_state as enum ('created', 'submitted', 'cancelled', 'completed');

               create table bookings
               (
                   booking_id           bigint primary key generated always as identity,
                   booking_state        booking_state not null,
                   booking_info_id      bigint not null,
                   booking_created_at   timestamp with time zone not null
               );
               """;
    }

    protected override string GetDownSql(IServiceProvider serviceProvider)
    {
        return """
               drop table if exists     bookings;
               drop table if exists     booking_infos;
               
               drop type if exists      booking_state;
               """;
    }
}