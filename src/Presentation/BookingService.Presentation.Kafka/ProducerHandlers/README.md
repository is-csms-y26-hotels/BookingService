Folder for `IEventHandler<>` implementations.

Use `IEvent`s and `IEventHandler<>`s to produce kafka messages, decoupled from application.\
Publish event in application layer and handle it here, using `IKafkaMessageProducer<,>`.

Event handlers are automatically registered in DI via assembly scanning and `IAssemblyMarker`.\
But kafka producers are not, so you have to register them in `ServiceCollectionExtensions` in `AddKafka`
method via `AddProducer` method.