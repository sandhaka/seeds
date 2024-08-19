<samp>

# Resources for Enterprise development
[![.NET](https://github.com/sandhaka/seeds/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sandhaka/seeds/actions/workflows/dotnet.yml)
## Index
- [x] [Option](./Monads/Option): Implementation of Option<> generic type based on [Monads](https://en.wikipedia.org/wiki/Monad_(functional_programming)) to manage nullable
- Domain-Driven-Design Seeds: Collection of DDD snippets
  - About DDD:
    - [Wikipedia](https://en.wikipedia.org/wiki/Domain-driven_design)
    - [Martin Fowler](https://martinfowler.com/tags/domain%20driven%20design.html) 
  - [Domain](./Ddd/Domain/): Base classes for Domain Entities and Value Objects 
    - [x] [Entity](./Ddd/Domain/Entity.cs)
    - [x] [ValueObject](./Ddd/Domain/ValueObject.cs)
    - [x] [Event Sourcing](./Ddd/Domain/EventSourcing)
  - [Infrastructure](./Ddd/Infrastructure/): Base classes for Infrastructure
    - [x] [Event Sourced Repository](./Ddd/Infrastructure/Repositories/EventSourcedRepository.cs)
    - [x] [WebSocket Manager](./Ddd/Infrastructure/Managers/WebSocket)
    - [x] Middlewares
      - [x] [WebSocket](./Ddd/Infrastructure/Middlewares/WebSocketMiddleware.cs)
      - [ ] Exception handling
    - [ ] Message bus
      - [ ] RabbitMQ
      - [ ] Kafka
  - [Application](./Ddd/Application/): Base classes for Application
    - [ ] CQRS: Command and Query segregation
- [ ] [Collection Extensions](./CollectionsExtensions/): Utilities for generic Collections
## License
[MIT](./license)

</samp>
