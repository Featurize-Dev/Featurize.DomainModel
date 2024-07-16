# Featurize.DomainModel

An abstract aggregate root designed to manage event-sourced models.

## Overview

**Featurize.DomainModel** is part of the Featurize suite, designed to support Feature Driven Development using Vertical Slice Architecture. This library provides a foundation for managing complex domain logic with event-sourced models, making it easier to build scalable and maintainable applications.

## Features

- **Event-Sourced Models**: Simplifies the management of domain events.
- **Compatibility**: Supports .NET 8.0 and higher.
- **Integration**: Seamlessly integrates with other Featurize libraries.

## Installation

Install via NuGet Package Manager Console:

```sh
NuGet\Install-Package Featurize.DomainModel -Version 1.0.0
```

Or include it in your project file:

```xml
<PackageReference Include="Featurize.DomainModel" Version="1.0.0" />
```

## Usage

### Create an `User` aggregate to store user information.

```csharp

public sealed class User : AggregateRoot<Guid>
{
    public string Firstname { get; private set; }
    public string Lastname { get; private set; }

    public string Password { get; private set; }

    private User() : base(Guid.NewGuid())
    {

    }

    public static User Create(string firstname, string lastname)
    {
        var aggregate = new User();
        aggregate.RecordEvent(new UserCreated(firstname, lastname));
        return aggregate;
    }

    public void ChangePassword(string password) 
    {
        RecordEvent(new PasswordChanged(password));
    }

    internal void Apply(UserCreated e)
    {
        Firstname = e.Firstname;
        Lastname = e.Lastname;
        Password = Guid.NewGuid().ToString();
    }

    internal void Apply(PasswordChanged e)
    {
        Password = e.Password;
    }
}

public record UserCreated(string Firstname, string Lastname) : EventRecord;
public record PasswordChanged(string Password) : EventRecord;

```

#### Use the `User` aggregate and store the events in some storage.

```csharp

var storage = new Storage();

var user = User.Create("Alice", "Cooper");

user.ChangePassword("1234567");

var events = user.GetUncommittedEvents();

storage.Save(events);

```

#### Load an `User` aggregate from an `EventCollection`

```csharp

var aggregateId = Guid.NewGuid();

var events = EventCollection.Create(aggregateId, new[]
{
    new UserCreated("Alice", "Cooper"),
    new PasswordChanged("ABC1234")
});

var aggregate = AggregateRoot.LoadFromHistory<User, Guid>(events);

```

## Contributing

We welcome contributions! Please follow these steps to contribute:

1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Commit your changes.
4. Push your branch to GitHub.
5. Submit a Pull Request.

Ensure your code adheres to the project's coding standards and passes all tests.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact

For more information, visit the [official website](http://www.featurize-dev.io) or contact us at info@featurize-dev.io.

Happy coding!

The Featurize Team