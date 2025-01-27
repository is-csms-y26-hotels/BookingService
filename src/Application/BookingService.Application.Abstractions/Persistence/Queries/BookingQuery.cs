using BookingService.Application.Models.ObjectValues;
using SourceKit.Generators.Builder.Annotations;

namespace BookingService.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record BookingQuery(
    BookingId[] BookingIds,
    BookingId[] BookingToExcludeIds,
    RoomId[] BookingInfoRoomIds,
    UserEmail[] BookingInfoUserEmails,
    (DateTimeOffset Start, DateTimeOffset End)? BookingInfoDateRange,
    [RequiredValue] int PageSize,
    long Cursor);