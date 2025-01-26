using Bookings.Kafka.Contracts;
using BookingService.Application.Contracts.Bookings.Events;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Kafka.Producer;

namespace BookingService.Presentation.Kafka.ProducerHandlers.Bookings;

internal class BookingSubmissionHandler(
    IKafkaMessageProducer<BookingKey, BookingValue> producer) : IEventHandler<BookingSubmissionEvent>
{
    public async ValueTask HandleAsync(BookingSubmissionEvent evt, CancellationToken cancellationToken)
    {
        var key = new BookingKey { BookingId = evt.BookingId };

        var value = new BookingValue
        {
            BookingSubmission = new BookingValue.Types.BookingSubmission
            {
                BookingId = evt.BookingId,
            },
        };

        var message = new KafkaProducerMessage<BookingKey, BookingValue>(key, value);
        await producer.ProduceAsync(message, cancellationToken);
    }
}