using BookingService.Application.Models.ObjectValues;
using SourceKit.Generators.Builder.Annotations;
using System;

namespace BookingService.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record BookingInfoQuery(
    BookingInfoId[] BookingInfoIds,
    HotelId[] HotelIds,
    RoomId[] RoomIds,
    UserEmail[] UserEmails,
    DateTimeOffset? MinCheckInDate,
    DateTimeOffset? MaxCheckOutDate,
    [RequiredValue] int PageSize,
    long Cursor);