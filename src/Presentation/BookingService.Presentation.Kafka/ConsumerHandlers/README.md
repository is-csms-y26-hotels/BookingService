Folder for `IKafkaMessageHandler<,>` implementations.

Implement a message handler for concrete types of `TKey` and `TValue`, and register it 
in `ServiceCollectionExtensions` in `AddKafka` method via `AddConsumer` method.