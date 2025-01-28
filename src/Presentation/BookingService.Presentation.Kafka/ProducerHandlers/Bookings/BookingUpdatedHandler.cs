using Bookings.Kafka.Contracts;
using BookingService.Application.Contracts.Bookings.Events;
using BookingService.Application.Models.Enums;
using Google.Protobuf.WellKnownTypes;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;

namespace BookingService.Presentation.Kafka.ProducerHandlers.Bookings;

internal class BookingUpdatedHandler(
    IKafkaMessageProducer<BookingKey, BookingValue> producer) : IEventHandler<BookingUpdatedEvent>
{
    public async ValueTask HandleAsync(BookingUpdatedEvent evt, CancellationToken cancellationToken)
    {
        var key = new BookingKey { BookingId = evt.BookingId.Value };

        var value = new BookingValue
        {
            BookingUpdated = new BookingValue.Types.BookingUpdated
            {
                BookingId = evt.BookingId.Value,
                BookingState = MapBookingStateEnum(evt.BookingState),
                RoomId = evt.BookingRoomId.Value,
                UserEmail = evt.BookingUserEmail.Value,
                CheckInDate = evt.BookingCheckInDate.ToTimestamp(),
                CheckOutDate = evt.BookingCheckOutDate.ToTimestamp(),
            },
        };

        var message = new KafkaProducerMessage<BookingKey, BookingValue>(key, value);
        await producer.ProduceAsync(message, cancellationToken);
    }

    private static BookingValue.Types.BookingState MapBookingStateEnum(BookingState bookingState)
    {
        return bookingState switch
        {
            BookingState.Created => BookingValue.Types.BookingState.Created,
            BookingState.Submitted => BookingValue.Types.BookingState.Submitted,
            BookingState.Cancelled => BookingValue.Types.BookingState.Cancelled,
            BookingState.Completed => BookingValue.Types.BookingState.Completed,
            _ => BookingValue.Types.BookingState.Unspecified,
        };
    }
}