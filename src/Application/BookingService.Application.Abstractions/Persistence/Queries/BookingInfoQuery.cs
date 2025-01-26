using BookingService.Application.Models.ObjectValues;
using SourceKit.Generators.Builder.Annotations;

namespace BookingService.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public partial record BookingInfoQuery(
    BookingInfoId[] BookingInfoIds,
    BookingInfoId[] BookingInfoToExcludeIds,
    RoomId[] RoomIds,
    UserEmail[] UserEmails,
    BookingInfoQuery.DateRangeModel? DateRange,
    [RequiredValue] int PageSize,
    long Cursor)
{
    public record DateRangeModel(DateTimeOffset Start, DateTimeOffset End);
}