using Bookings.Kafka.Contracts;
using BookingService.Application.Contracts.Bookings.Events;
using Google.Protobuf.WellKnownTypes;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;

namespace BookingService.Presentation.Kafka.ProducerHandlers.Bookings;

internal class BookingCreatedHandler(
    IKafkaMessageProducer<BookingKey, BookingValue> producer) : IEventHandler<BookingCreatedEvent>
{
    public async ValueTask HandleAsync(BookingCreatedEvent evt, CancellationToken cancellationToken)
    {
        var key = new BookingKey { BookingId = evt.BookingId.Value };

        var value = new BookingValue
        {
            BookingCreated = new BookingValue.Types.BookingCreated
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

    private static BookingState MapBookingStateEnum(Application.Models.Enums.BookingState bookingState)
    {
        return bookingState switch
        {
            Application.Models.Enums.BookingState.Created => BookingState.Created,
            Application.Models.Enums.BookingState.Submitted => BookingState.Submitted,
            Application.Models.Enums.BookingState.Cancelled => BookingState.Cancelled,
            Application.Models.Enums.BookingState.Completed => BookingState.Completed,
            _ => BookingState.Unspecified,
        };
    }
}