<samp>

# Code snippets and resources for Enterprise development
## Index
- [x] [Option](./Monads/Option): Implementation of Option<> generic type based on [Monads](https://en.wikipedia.org/wiki/Monad_(functional_programming)) to manage nullable
  - [x] Extensions: [Enumerable](./Monads/Option/EnumerableExtensions.cs), [Dictionary](./Monads/Option/DictionaryExtensions.cs), [Object](./Monads/Option/ObjectExtensions.cs)
- [x] Domain-Driven-Design Seeds: Collection of DDD snippets for Enterprise projects
  - About DDD:
    - [Wikipedia](https://en.wikipedia.org/wiki/Domain-driven_design)
    - [Martin Fowler](https://martinfowler.com/tags/domain%20driven%20design.html) 
  - [x] [Domain](./Ddd/Domain/): Base classes for Domain Entities and Value Objects 
    - [x] Entity
    - [x] ValueObject
    - [x] Event Sourcing
  - [x] [Infrastructure](./Ddd/Infrastructure/): Base classes for Infrastructure
    - [x] [Event Sourced Repository](./Ddd/Infrastructure/Repositories/EventSourcedRepository.cs)
    - [x] [WebSocket Manager](./Ddd/Infrastructure/Managers/WebSocket)
    - [x] Middlewares
      - [x] [WebSocket](./Ddd/Infrastructure/Middlewares/WebSocketMiddleware.cs)
      - [ ] Exception handling
    - [ ] Message bus
      - [ ] RabbitMQ
      - [ ] Kafka
  - [ ] [Application](./Ddd/Application/): Base classes for Application
    - [ ] CQRS: Command and Query segregation
## License
[MIT](./license)

</samp>
