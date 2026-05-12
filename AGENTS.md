# Paragon Robotics DiamondScout Coding Conventions

This document outlines the coding standards and architectural patterns for the DiamondScout project. AI agents should follow these rules when generating or refactoring code.

## General Principles
- **Language**: F# latest (net10.0).
- **Architecture**: Domain-Driven Design (DDD) with a focus on functional purity and event sourcing principles.
- **Namespaces**: Use `ParagonRobotics.DiamondScout.ProjectName.*` hierarchy, where ProjectName is the name of the project (e.g., `ParagonRobotics.DiamondScout.Common` for the Common library).

## Domain Modeling

### Basic Design Principles

- Constructors should be `private` for complex domain objects. Value types should be marked as `[<Struct>]` and should not have `private` constructors.
- This project is expected to be consumed from both C# and F#.
  - F# functional-style module definitions and let bindings should be included in an `[<AutoOpen>]` `Functional` module.
  - Domain types should be extended with `static member` functions that call the functional-style code in the `Functional` module.
    - The API of the `static member` functions should use generic interfaces to prevent C# consumers from having to deal with F# types.
      - For example, if a Functional function's argument is a `list<T>`, the `static member` function should accept `IReadOnlyCollection<T>` instead.
      - Type conversions should match against the underlying type to make sure it is the same to avoid unnecessary boxing or reconstruction of the entire collection.
  - Domain types and associated modules should be marked as `[<RequireQualifiedAccess>]` to avoid ambiguity.
- Entity types should be immutable and have a `private` constructor.
  - They should not contain references to their own identifier types.
  - Only invariants should be defined in an Entity type.
  - Entity types should define functions to create and manipulate their invariants based on the design principles above.

### Value Objects and IDs
- Define IDs and value types as `[<Struct>]` single-case discriminated unions.
- Make the constructor `private`.
- Include `static member Zero = TypeName Guid.Empty` for identifier types. Other value types that have a logical zero or empty state should also include a `static member Zero`.
- Include `static member Create() = Guid.CreateVersion7() |> TypeName` for identifier types. Other value types should simply allow the creation of new instances using their single DU case.
- Include a `Value` member to extract the underlying data.
- Example:
  ```fsharp
  [<Struct>]
  type TeamId =
      private | TeamId of Guid
      static member Zero = TeamId Guid.Empty
      static member Create() = Guid.CreateVersion7() |> TeamId
      member this.Value = let (TeamId guid) = this in guid
  ```

### Types and Records
- Use records for multi-field data structures.
- Use Discriminated Unions (DUs) for state and choice representation.
- Prefer DUs over enums.
- All domain concepts should be a concrete type. Eliminate primitive obsession.
- DUs should have a method to allow C# consumers to easily match on the DU. The function does not necessarily need to be named `Match` but should be descriptive based on the domain concept the DU is modeling.
- Apply `[<RequireQualifiedAccess>]` to DUs and modules where ambiguity might occur.
- Use `[<Struct>]` for small value-like types to optimize performance.

### Aggregate Patterns
- Use the `decide` and `evolve` pattern for domain logic by creating an instance of the `AggregateDefinition` type.
- The `Init` member of the `AggregateDefinition` should express the initial state of the aggregate. Many times, this will be a DU with a case representing the initial state of the aggregate and a case representing an initialized aggregate with instance data.
  - The initial state might have a perfectly logical zero or empty state that can be used instead of creating a DU.
  - The initial state should be a proper domain representation of an aggregate that has not been created yet; the name of the initial state should adequately express the domain concept.
- `evolve`: `'state -> 'event -> 'state` (Updates state based on events).
- `decide`: `'command -> 'state -> Validation<'event list, 'error>` (Validates commands and produces events).
- Wrap core functional logic in a `Functional` module with `[AutoOpen]`.
- Extend the aggregate type with `static member` functions that call the functional-style code in the `Functional` module to enhance ergonomics when consuming from C#.

## Error Handling
- Use `FsToolkit.ErrorHandling`.
- Prefer `Validation` over `Result` for command validation to accumulate multiple errors.
  - Use the `and!` operator to chain validations, unless validation must be short-circuited.
  - Use `Validation.map` to transform results.
  - Use `Validation.mapError` to transform errors.
  - Prefer `validation { ... }` computation expressions to mapping, applying, and chaining validations.
- Define domain-specific `Error` DUs.
    - All errors should be defined in an `Errors` DU. Relevant data to the error should be included in an error's DU case.
    - Errors are expected to be evaluated and handled by the application layer. Assumptions should not be made about the severity of an error.
    - Only the most exceptional errors in the domain should be raised exceptions. This should imply that the error is not recoverable and the application should exit to protect the system or data integrity.

## Style and Formatting
- **Indentation**: 4 spaces.
- **Record Formatting**: Align fields vertically.
  ```fsharp
  { Field1: Type1
    Field2: Type2 }
  ```
- **Piping**: Use `|>` for forward piping and `||>` for double-argument piping where appropriate.
- **Documentation**: Use XML doc comments (`///`) for all public types, modules, and members.
- **Naming**: 
  - PascalCase for types, modules, and members.
  - camelCase for local variables and parameters.
  - Use descriptive names reflecting the domain (e.g., `TeamNumber`, `Points`).

## Project Structure
- `Common`: Domain models, value objects, domain services, and core logic.
- `Application`: Command handlers, event storage, and application services.
- `SyncContract`: Data transfer objects and serialization logic.
- `DiamondScout.Api`: ASP.NET Core API.

## Specific Patterns
- Use `Guid.CreateVersion7()` for new ID generation (available in .NET 9+).
- Leverage `static member` on types to provide a "clean" API for command entry points that appears idiomatic to C# consumers.
- Use `[<AutoOpen>]` sparingly, mainly for modules that extend existing types or provide essential functional combinators.
